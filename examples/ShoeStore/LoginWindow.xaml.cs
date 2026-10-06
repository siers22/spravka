using System.Windows;
namespace ShoeStore;
public partial class LoginWindow : Window
{
    public LoginWindow() { InitializeComponent(); }
    private async void Enter_Click(object sender, RoutedEventArgs e)
    {
        Enter.IsEnabled = false;
        try { await App.Store.LoginAsync(Login.Text); DialogResult = true; }
        catch (Exception ex) { App.Error(this, ex); }
        finally { Enter.IsEnabled = true; }
    }
    private void Back_Click(object sender, RoutedEventArgs e) => Close();
}
