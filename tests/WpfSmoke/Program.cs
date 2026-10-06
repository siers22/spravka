using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        try
        {
            var application = new ExamGuide.App(); application.InitializeComponent(); application.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var login = new ExamGuide.LoginWindow();
            if (login.FindName("Tiles") is not System.Windows.Controls.Primitives.UniformGrid { Children.Count: 4 }) throw new Exception("Puzzle images missing");
            var windows = new Window[] { login, new ExamGuide.AdminWindow(), new ShoeStore.LoginWindow(), new ShoeStore.MainWindow(), new ShoeStore.ProductWindow(new(1,"Модель","Категория","Подкатегория","Производитель","missing.png","Описание","Кожа",4000,3000,3)), new ShoeStore.CartWindow(), new ShoeStore.OrdersWindow() };
            Directory.CreateDirectory("wpf-smoke");
            foreach (var window in windows)
            {
                // Measure and render the actual compiled XAML on Windows without DB-dependent Loaded events.
                var content = (FrameworkElement)window.Content;
                content.Measure(new Size(window.Width, window.Height)); content.Arrange(new Rect(0,0,window.Width,window.Height)); content.UpdateLayout();
                var bitmap = new RenderTargetBitmap((int)window.Width,(int)window.Height,96,96,PixelFormats.Pbgra32); bitmap.Render(content);
                var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using var file = File.Create(Path.Combine("wpf-smoke",window.GetType().FullName+".png")); encoder.Save(file);
                Console.WriteLine("PASS: rendered " + window.GetType().FullName); window.Close();
            }
            if (new ShoeStore.ProductRow(new(1,"Test","C","S","M","","","",100,75,3)).Background != "#ff8080") throw new Exception("Stock boundary color");
            Console.WriteLine("PASS: WPF windows, actual images and stock boundary"); return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
}
