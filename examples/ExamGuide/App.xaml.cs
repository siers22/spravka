using System.Windows;
using Microsoft.Extensions.Configuration;
using ExamGuide.Data;
namespace ExamGuide;
public partial class App : Application
{
    public static UserRepository Users { get; private set; } = null!;
    public static AuthService Auth { get; private set; } = null!;
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            var config = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("appsettings.json").Build();
            Users = new UserRepository(new Database(config));
            Auth = new AuthService(Users);
            MainWindow = new LoginWindow();
            MainWindow.Show();
        }
        catch (Exception ex) { MessageBox.Show("Не удалось запустить приложение. Проверьте appsettings.json.\n" + ex.Message, "Ошибка запуска", MessageBoxButton.OK, MessageBoxImage.Error); Shutdown(1); }
    }
}
