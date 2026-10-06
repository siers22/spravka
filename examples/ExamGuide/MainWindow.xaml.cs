using System.Windows;
namespace ExamGuide;
public partial class MainWindow : Window
{
    public MainWindow() { InitializeComponent(); RefreshIdentity(); }
    private void RefreshIdentity()
    {
        Welcome.Text = $"Пользователь: {App.Auth.Current?.Login} · Роль: {App.Auth.Current?.Role}";
        Admin.Visibility = App.Auth.Current?.Role == "Администратор" ? Visibility.Visible : Visibility.Collapsed;
    }
    private async void Admin_Click(object sender, RoutedEventArgs e)
    {
        try { await App.Auth.RequireUserAsync(administrator: true); new AdminWindow { Owner = this }.ShowDialog(); await App.Auth.RequireUserAsync(); RefreshIdentity(); }
        catch (UnauthorizedAccessException ex) { MessageBox.Show(this, ex.Message, "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning); if (App.Auth.Current is null) Logout(); else RefreshIdentity(); }
        catch (Exception) { MessageBox.Show(this, "Не удалось проверить доступ. Проверьте подключение к БД.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }
    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        try { await App.Auth.RequireUserAsync(); RefreshIdentity(); }
        catch (UnauthorizedAccessException) { Logout(); }
        catch (Exception) { MessageBox.Show(this, "Не удалось проверить учётную запись. Проверьте БД.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }
    private void Logout_Click(object sender, RoutedEventArgs e) => Logout();
    private void Logout() { App.Auth.Logout(); var login = new LoginWindow(); Application.Current.MainWindow = login; login.Show(); Close(); }
}
