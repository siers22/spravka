using System.Text.Json;
namespace ShoeStore.Data;
public static class BasketSession
{
    public static List<BasketLine> Read(ISession session) =>
        JsonSerializer.Deserialize<List<BasketLine>>(session.GetString("Basket") ?? "[]") ?? [];
    public static void Write(ISession session,List<BasketLine> lines) => session.SetString("Basket",JsonSerializer.Serialize(lines));
}
