using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace Game_Library
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private static readonly string LogPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "crash_log.txt");

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1) Bắt exception xảy ra trên UI thread (Dispatcher)
            this.DispatcherUnhandledException += (s, args) =>
            {
                LogCrash("DispatcherUnhandledException", args.Exception);
                MessageBox.Show(
                    $"Lỗi UI thread:\n{args.Exception}",
                    "Crash", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true; // Ngăn app tự tắt để đọc được lỗi
            };

            // 2) Bắt exception nghiêm trọng ở cấp AppDomain (kể cả từ thread khác)
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                LogCrash("AppDomain.UnhandledException", ex);
                // Lưu ý: IsTerminating thường = true ở đây, app sẽ tắt ngay
                // sau khi log xong, không cản được (đặc biệt là StackOverflow)
            };

            // 3) Bắt exception từ Task chưa được await (fire-and-forget)
            TaskScheduler.UnobservedTaskException += (s, args) =>
            {
                LogCrash("TaskScheduler.UnobservedTaskException", args.Exception);
                args.SetObserved(); // Ngăn app crash vì lỗi này
            };
        }

        private void LogCrash(string source, Exception ex)
        {
            try
            {
                string content =
                    $"===== {DateTime.Now:yyyy-MM-dd HH:mm:ss} | {source} =====\n" +
                    $"{ex}\n\n";
                File.AppendAllText(LogPath, content);
            }
            catch { /* không để lỗi log làm crash thêm */ }
        }
    }

}
