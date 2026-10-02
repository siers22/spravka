using System.Globalization;
using System.Text.Json.Serialization;

namespace ExamGuide.Data;

public sealed record NoteResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("title_user")] string TitleUser,
    [property: JsonPropertyName("content")] string Content,
    [property: JsonPropertyName("formatted_date")] string FormattedDate);

public sealed class NotesRepository(Database database)
{
    public async Task<List<NoteResponse>> GetAsync(int? userId)
    {
        await using var connection = database.CreateConnection();
        await connection.OpenAsync();
        var sql = """
            SELECT n.id, n.title, n.content, n.created_at, u.login
            FROM notes n JOIN users u ON u.id = n.id_user
            """;
        if (userId.HasValue) sql += " WHERE n.id_user = @userId";
        sql += " ORDER BY n.id";
        await using var command = userId.HasValue
            ? Database.Command(connection, sql, ("@userId", userId.Value))
            : Database.Command(connection, sql);
        await using var reader = await command.ExecuteReaderAsync();
        var result = new List<NoteResponse>();
        while (await reader.ReadAsync())
        {
            result.Add(new NoteResponse(
                reader.GetInt32(0),
                $"{reader.GetString(1)} - {reader.GetString(4)}",
                reader.GetString(2),
                reader.GetDateTime(3).ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)));
        }
        return result;
    }
}
