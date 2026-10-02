using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace ExamGuide.Data;

public sealed record AppUser(int Id, string Login, string PasswordHash, string Role, int FailedAttempts, bool IsBlocked);
public sealed record Customer(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("inn")] string Inn,
    [property: JsonPropertyName("addres")] string Address,
    [property: JsonPropertyName("phone")] string Phone,
    [property: JsonPropertyName("type")] string Type);

public sealed class UserRepository(Database database)
{
    private readonly PasswordHasher<string> hasher = new();
    public bool Verify(AppUser user, string password) =>
        hasher.VerifyHashedPassword(user.Login, user.PasswordHash, password) != PasswordVerificationResult.Failed;

    public async Task<AppUser?> FindAsync(string login)
    {
        await using var connection = database.CreateConnection();
        await connection.OpenAsync();
        await using var command = Database.Command(connection,
            "SELECT id,login,password_hash,role,failed_attempts,is_blocked FROM users WHERE login=@login",
            ("@login", login.Trim().ToLowerInvariant()));
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? new AppUser(reader.GetInt32(0), reader.GetString(1), reader.GetString(2),
            reader.GetString(3), reader.GetInt32(4), reader.GetInt32(5) == 1) : null;
    }

    public async Task<List<AppUser>> AllAsync()
    {
        await using var connection = database.CreateConnection();
        await connection.OpenAsync();
        await using var command = Database.Command(connection,
            "SELECT id,login,password_hash,role,failed_attempts,is_blocked FROM users ORDER BY id");
        await using var reader = await command.ExecuteReaderAsync();
        var result = new List<AppUser>();
        while (await reader.ReadAsync()) result.Add(new AppUser(reader.GetInt32(0), reader.GetString(1), reader.GetString(2),
            reader.GetString(3), reader.GetInt32(4), reader.GetInt32(5) == 1));
        return result;
    }

    public async Task RegisterFailureAsync(int id)
    {
        await using var connection = database.CreateConnection();
        await connection.OpenAsync();
        // Счётчик хранится в БД. Новая вкладка не сбросит блокировку.
        await using var command = Database.Command(connection, """
            UPDATE users SET failed_attempts=failed_attempts+1,
              is_blocked=CASE WHEN failed_attempts+1>=3 THEN 1 ELSE is_blocked END
            WHERE id=@id
            """, ("@id", id));
        await command.ExecuteNonQueryAsync();
    }

    public async Task ResetFailuresAsync(int id)
    {
        await using var connection = database.CreateConnection();
        await connection.OpenAsync();
        await using var command = Database.Command(connection, "UPDATE users SET failed_attempts=0 WHERE id=@id", ("@id",id));
        await command.ExecuteNonQueryAsync();
    }

    public async Task AddAsync(string login, string password, string role)
    {
        login = login.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Заполните логин и пароль");
        if (role != "Администратор" && role != "Пользователь") throw new ArgumentException("Выберите допустимую роль");
        await using var connection = database.CreateConnection();
        await connection.OpenAsync();
        // ID выделяется внутри serializable-транзакции; UNIQUE дополнительно защищает логин.
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable);
        await using var check = Database.Command(connection, "SELECT COUNT(*) FROM users WHERE login=@login", ("@login",login));
        check.Transaction = transaction;
        if (Convert.ToInt32(await check.ExecuteScalarAsync()) > 0) throw new ArgumentException("Пользователь с таким логином уже существует");
        await using var nextId = Database.Command(connection,"SELECT COALESCE(MAX(id),0)+1 FROM users");
        nextId.Transaction = transaction;
        var id = Convert.ToInt32(await nextId.ExecuteScalarAsync());
        await using var insert = Database.Command(connection,
            "INSERT INTO users(id,login,password_hash,role) VALUES(@id,@login,@hash,@role)",
            ("@id",id),("@login",login),("@hash",hasher.HashPassword(login,password)),("@role",role));
        insert.Transaction = transaction;
        await insert.ExecuteNonQueryAsync();
        await transaction.CommitAsync();
    }

    public async Task EditAsync(int id, string login, string? password, string role, bool unblock)
    {
        login = login.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(login)) throw new ArgumentException("Заполните логин");
        if (role != "Администратор" && role != "Пользователь") throw new ArgumentException("Выберите допустимую роль");
        var existing = await FindAsync(login);
        if (existing is not null && existing.Id != id) throw new ArgumentException("Пользователь с таким логином уже существует");
        await using var connection = database.CreateConnection();
        await connection.OpenAsync();
        var sql = "UPDATE users SET login=@login,role=@role";
        var parameters = new List<(string, object?)> { ("@id",id),("@login",login),("@role",role) };
        if (!string.IsNullOrWhiteSpace(password)) { sql += ",password_hash=@hash"; parameters.Add(("@hash",hasher.HashPassword(login,password))); }
        if (unblock) sql += ",failed_attempts=0,is_blocked=0";
        sql += " WHERE id=@id";
        await using var command = Database.Command(connection,sql,parameters.ToArray());
        if (await command.ExecuteNonQueryAsync() != 1) throw new ArgumentException("Пользователь не найден. Обновите страницу");
    }

    public async Task SeedAsync()
    {
        if (await FindAsync("admin") is null) await AddAsync("admin","Admin123!","Администратор");
        if (await FindAsync("user") is null) await AddAsync("user","User123!","Пользователь");
        var user = (await FindAsync("user"))!;
        await using var connection = database.CreateConnection();
        await connection.OpenAsync();
        for (var i = 1; i <= 3; i++)
        {
            await using var command = Database.Command(connection, """
                INSERT INTO notes(id,title,content,id_user,created_at)
                SELECT @id,@title,@content,@user,@date
                WHERE NOT EXISTS (SELECT 1 FROM notes WHERE id=@id)
                """, ("@id",i),("@title",i==1 ? "Конференция ИТ" : $"Заметка {i}"),
                ("@content",$"Содержание заметки {i}"),("@user",user.Id),("@date",new DateTime(2027,3,15+i-1)));
            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task ImportCustomersAsync(string path)
    {
        // В исходном JSON ключ адреса написан именно addres.
        var customers = JsonSerializer.Deserialize<List<Customer>>(await File.ReadAllTextAsync(path))
            ?? throw new InvalidOperationException("JSON не содержит массива");
        await using var connection = database.CreateConnection();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        foreach (var customer in customers)
        {
            await using var command = Database.Command(connection, """
                INSERT INTO customers(id,name,inn,address,phone,customer_type)
                SELECT @id,@name,@inn,@address,@phone,@type
                WHERE NOT EXISTS (SELECT 1 FROM customers WHERE id=@id)
                """, ("@id",customer.Id),("@name",customer.Name),("@inn",customer.Inn),
                ("@address",customer.Address),("@phone",customer.Phone),("@type",customer.Type));
            command.Transaction = transaction;
            await command.ExecuteNonQueryAsync();
        }
        await transaction.CommitAsync();
    }
}
