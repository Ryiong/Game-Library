using Game_Library.Models;
using Game_Library.Services;
using Game_Library.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;

namespace Game_Library.Views
{
    /// <summary>
    /// Interaction logic for GameView.xaml
    /// </summary>
    public partial class GameView : System.Windows.Controls.UserControl
    {
        public GameViewModel ViewModel { get; private set; }
        private Process _flashProcess = null;
        private GameHttpServer _htmlServer = null;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetMenu(IntPtr hWnd, IntPtr hMenu);

        private const int GWL_STYLE = -16;
        private const int WS_VISIBLE = 0x10000000;

        public GameView(GameModels selectedGame)
        {
            InitializeComponent();
            ViewModel = new GameViewModel(selectedGame);
            this.DataContext = ViewModel;
            ViewModel.RequestCloseGame += CloseGameProcess;

            Loaded += (s, e) => GameView_Load(s, e);
        }

        private void GameView_Load(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameData.Type.Equals("FLASH", StringComparison.OrdinalIgnoreCase))
            {
                wfHost.Visibility = Visibility.Visible;
                htmlWebView.Visibility = Visibility.Collapsed;
                LaunchFlashGame(); // Chạy hàm xử lý Win32 API cũ của bạn
            }
            else if (ViewModel.GameData.Type.Equals("HTML5", StringComparison.OrdinalIgnoreCase))
            {
                wfHost.Visibility = Visibility.Collapsed;
                htmlWebView.Visibility = Visibility.Visible;
                LaunchHtml5Game();
            }
        }

        private async void LaunchHtml5Game()
        {
            try
            {
                string gameFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "games_data", ViewModel.GameData.Id);

                _htmlServer = new GameHttpServer();
                _htmlServer.Start(gameFolderPath);

                GameDataService.Instance.SaveGameAsync(ViewModel.GameData, true);

                //if (Application.Current.MainWindow is MainWindow mainWindow)
                //{
                //    mainWindow.ServerStatus.Text = "Server Status: Active";
                //}

                await htmlWebView.EnsureCoreWebView2Async();
                await htmlWebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(
                    "window.open = function() { console.log('App Launcher: Đã chặn đứng lệnh mở quảng cáo pop-up/clickunder ngầm!'); return null; };"
                );
                htmlWebView.CoreWebView2.NewWindowRequested += (sender, args) =>
                {
                    args.Handled = true;
                };
                string gameUrl = _htmlServer.BaseUrl + ViewModel.GameData.MainFile;

                htmlWebView.Source = new Uri(gameUrl);

                txtLoadingStatus.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể nạp game HTML5: " + ex.Message, "Lỗi Server", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LaunchFlashGame()
        {
            try
            {
                string debugPlayerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "flashplayer_debug.exe");

                if (!File.Exists(debugPlayerPath))
                {
                    debugPlayerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "flashplayer_debug.exe");
                    if (!File.Exists(debugPlayerPath))
                    {
                        MessageBox.Show("Không tìm thấy tệp phần mềm nền 'flashplayer_debug.exe'. Vui lòng kiểm tra lại thư mục Resources!", "Thiếu tài nguyên", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                string fullSwfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "games_data", ViewModel.GameData.Id, ViewModel.GameData.MainFile);

                if (!File.Exists(fullSwfPath))
                {
                    MessageBox.Show($"Không tìm thấy file game chính tại: {fullSwfPath}", "Lỗi file game", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _flashProcess = new Process();
                _flashProcess.StartInfo.FileName = debugPlayerPath;
                _flashProcess.StartInfo.Arguments = $"\"{fullSwfPath}\"";
                _flashProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                _flashProcess.Start();

                int timeout = 0;
                while (_flashProcess.MainWindowHandle == IntPtr.Zero && timeout < 50)
                {
                    await Task.Delay(100);
                    _flashProcess.Refresh();
                    timeout++;
                }

                IntPtr flashWindowHandle = _flashProcess.MainWindowHandle;

                if (flashWindowHandle != IntPtr.Zero)
                {
                    IntPtr hostPanelHandle = pnlGameContainer.Handle;

                    SetParent(flashWindowHandle, hostPanelHandle);
                    SetMenu(flashWindowHandle, IntPtr.Zero);

                    SetWindowLong(flashWindowHandle, GWL_STYLE, WS_VISIBLE);

                    txtLoadingStatus.Visibility = Visibility.Collapsed;
                    ResizeEmbeddedGame();
                }
                else
                {
                    txtLoadingStatus.Text = "LỖI KHÔNG THỂ KHỞI ĐỘNG FLASH WINDOW!";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra khi nạp game Flash: " + ex.Message, "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResizeEmbeddedGame()
        {
            if (_flashProcess != null && !_flashProcess.HasExited)
            {
                IntPtr flashWindowHandle = _flashProcess.MainWindowHandle;
                if (flashWindowHandle != IntPtr.Zero)
                {
                    int width = (int)pnlGameContainer.Width;
                    int height = (int)pnlGameContainer.Height;

                    MoveWindow(flashWindowHandle, 0, 0, width, height, true);
                }
            }
        }

        private void GameContainer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            pnlGameContainer.Width = (int)e.NewSize.Width;
            pnlGameContainer.Height = (int)e.NewSize.Height;

            ResizeEmbeddedGame();
        }

        private void CloseGameProcess()
        {
            try
            {
                if (_flashProcess != null && !_flashProcess.HasExited)
                {
                    _flashProcess.Kill();
                    _flashProcess.Dispose();
                    _flashProcess = null;
                }
            }
            catch { /* Bỏ qua nếu tiến trình đã tự đóng trước đó */ }

            try
            {
                if (htmlWebView != null && htmlWebView.CoreWebView2 != null)
                {
                    htmlWebView.Source = new Uri("about:blank");
                    htmlWebView.Dispose(); 
                }
            }
            catch { /* Bỏ qua nếu WebView2 đã bị hủy trước đó */ }

            if (_htmlServer != null)
            {
                _htmlServer.Stop();
                _htmlServer = null;

                //if (Application.Current.MainWindow is MainWindow main)
                //{
                //    main.ServerStatus.Text = "Server Status: Off";
                //}
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.RequestCloseGame -= CloseGameProcess;
            CloseGameProcess();
        }
    }
}
