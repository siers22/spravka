using System.Data.Common;
using Microsoft.Data.SqlClient;
using Npgsql;

namespace ExamGuide.Data;

// Один интерфейс ADO.NET, два конкретных провайдера.
public sealed class Database(IConfiguration config)
{
    public DbConnection CreateConnection()
    {
        var provider = config["Database:Provider"] ?? "Postgres";
        var connectionString = config.GetConnectionString(provider)
            ?? throw new InvalidOperationException("Не задана строка подключения");
        return provider switch
        {
            "Postgres" => new NpgsqlConnection(connectionString),
            "SqlServer" => new SqlConnection(connectionString),
            _ => throw new InvalidOperationException("Неизвестный провайдер БД")
        };
    }

    public static DbCommand Command(DbConnection connection, string sql,
        params (string Name, object? Value)[] parameters)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;
        foreach (var (name, value) in parameters)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }
        return command;
    }
}
