using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ExamGuide.Data;
namespace ExamGuide;
public partial class LoginWindow : Window
{
    private readonly Puzzle puzzle = new();
    private int? selected;
    public LoginWindow() { InitializeComponent(); DrawPuzzle(); }
    private void DrawPuzzle()
    {
        Tiles.Children.Clear(); selected = null;
        for (var index = 0; index < 4; index++)
        {
            var image = new BitmapImage(new Uri(Path.Combine(AppContext.BaseDirectory, "Assets", "puzzle", $"{puzzle.Tiles[index]}.png")));
            var button = new Button { Tag = index, Margin = new Thickness(1), Padding = new Thickness(0),
                Content = new Image { Source = image, Stretch = Stretch.Uniform } };
            button.Click += Tile_Click; Tiles.Children.Add(button);
        }
    }
    private void Tile_Click(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender; var index = (int)button.Tag;
        if (selected is null) { selected = index; button.BorderBrush = Brushes.DodgerBlue; button.BorderThickness = new Thickness(4); }
        else { puzzle.Swap(selected.Value, index); DrawPuzzle(); }
    }
    private void Shuffle_Click(object sender, RoutedEventArgs e) { puzzle.Shuffle(); DrawPuzzle(); }
    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        Enter.IsEnabled = Seed.IsEnabled = false;
        try
        {
            await App.Auth.LoginAsync(Login.Text, Password.Password, puzzle);
            MessageBox.Show(this, "Вы успешно авторизовались", "Вход выполнен", MessageBoxButton.OK, MessageBoxImage.Information);
            var desktop = new MainWindow(); Application.Current.MainWindow = desktop; desktop.Show(); Close();
        }
        catch (Exception ex) { ShowError(ex); }
        finally { Password.Clear(); DrawPuzzle(); Enter.IsEnabled = Seed.IsEnabled = true; }
    }
    private async void Seed_Click(object sender, RoutedEventArgs e)
    {
        Seed.IsEnabled = Enter.IsEnabled = false;
        try
        {
            await App.Users.SeedAsync();
            await App.Users.ImportCustomersAsync(Path.Combine(AppContext.BaseDirectory, "Data", "customers.json"));
            MessageBox.Show(this, "Подготовлены admin / Admin123!, user / User123!, 3 заметки и 6 контрагентов.", "Данные готовы", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) { ShowError(ex); }
        finally { Seed.IsEnabled = Enter.IsEnabled = true; }
    }
    private void ShowError(Exception ex) => MessageBox.Show(this, ex is ArgumentException or UnauthorizedAccessException ? ex.Message : "Операция не выполнена. Проверьте БД, schema/seed и настройки подключения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
}
