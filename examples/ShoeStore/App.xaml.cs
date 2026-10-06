using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using ShoeStore.Data;
namespace ShoeStore;
public partial class App : Application
{
    public static StoreService Store { get; private set; } = null!;
    public static ImportService Importer { get; private set; } = null!;
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            var config = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("appsettings.json").Build();
            var database = new Database(config);
            Store = new StoreService(new CatalogRepository(database), new OrderRepository(database), new StoreClock(config));
            Importer = new ImportService(database, Path.Combine(AppContext.BaseDirectory, "Data"));
            MainWindow = new MainWindow(); MainWindow.Show();
        }
        catch (Exception ex) { MessageBox.Show("Не удалось запустить магазин. Проверьте appsettings.json.\n" + ex.Message, "Ошибка запуска", MessageBoxButton.OK, MessageBoxImage.Error); Shutdown(1); }
    }
    public static void Error(Window owner, Exception ex) => MessageBox.Show(owner,
        ex is ArgumentException or UnauthorizedAccessException or InvalidOperationException ? ex.Message : "Не удалось выполнить действие. Проверьте службу БД и настройки подключения, затем обновите список.",
        "Чудо Обувь · Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
}
