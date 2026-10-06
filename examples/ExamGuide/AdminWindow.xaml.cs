using System.Windows;
using System.Windows.Controls;
using ExamGuide.Data;
namespace ExamGuide;
public partial class AdminWindow : Window
{
    private int? editingId;
    public AdminWindow() { InitializeComponent(); Loaded += async (_, _) => await LoadUsersAsync(); }
    private async Task LoadUsersAsync()
    {
        try { await App.Auth.RequireUserAsync(administrator: true); UsersGrid.ItemsSource = await App.Users.AllAsync(); }
        catch (Exception ex) { Error(ex); }
    }
    private void SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (UsersGrid.SelectedItem is not AppUser user) return;
        editingId = user.Id; Login.Text = user.Login; Password.Clear(); Unblock.IsChecked = false;
        Role.SelectedIndex = user.Role == "Администратор" ? 1 : 0;
    }
    private void New_Click(object sender, RoutedEventArgs e) { editingId = null; UsersGrid.UnselectAll(); Login.Clear(); Password.Clear(); Unblock.IsChecked = false; Role.SelectedIndex = 0; }
    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        Save.IsEnabled = false;
        try
        {
            await App.Auth.RequireUserAsync(administrator: true); // Проверка роли при каждом действии, а не только видимости кнопки.
            var role = ((ComboBoxItem)Role.SelectedItem).Content.ToString()!;
            if (editingId is int id) await App.Users.EditAsync(id, Login.Text, Password.Password, role, Unblock.IsChecked == true);
            else await App.Users.AddAsync(Login.Text, Password.Password, role);
            Password.Clear(); editingId = null;
            MessageBox.Show(this, "Данные сохранены", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            await LoadUsersAsync();
        }
        catch (Exception ex) { Error(ex); }
        finally { Save.IsEnabled = true; }
    }
    private void Error(Exception ex) => MessageBox.Show(this, ex is ArgumentException or UnauthorizedAccessException ? ex.Message : "Не удалось выполнить действие. Проверьте БД и обновите список.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
    private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadUsersAsync();
    private void Back_Click(object sender, RoutedEventArgs e) => Close();
}
