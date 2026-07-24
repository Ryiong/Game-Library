using Game_Library.Models;
using Game_Library.Services;
using Game_Library.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
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
        private DispatcherTimer _resizeDebounceTimer;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetMenu(IntPtr hWnd, IntPtr hMenu);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const int GWL_STYLE = -16;
        private const int WS_VISIBLE = 0x10000000;

        public GameView(GameModels selectedGame)
        {
            InitializeComponent();
            ViewModel = new GameViewModel(selectedGame);
            this.DataContext = ViewModel;

            ViewModel.RequestCloseGame += CloseGameProcess;

            ViewModel.SetMuteAction = OnSetMute;

            ViewModel.SetVolumeAction = OnSetVolume;

            ViewModel.ToggleFullscreenAction = OnToggleFullscreen;

            Loaded += (s, e) => GameView_Load(s, e);
            Unloaded += GameView_Unloaded;
            InitResizeTimer();
        }

        private void InitResizeTimer()
        {
            _resizeDebounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(20)
            };
            _resizeDebounceTimer.Tick += (s, e) =>
            {
                _resizeDebounceTimer.Stop();
                ResizeEmbeddedGame();
            };
        }

        private void GameView_Load(object sender, RoutedEventArgs e)
        {
            if (ViewModel.GameData.Type.Equals("FLASH", StringComparison.OrdinalIgnoreCase))
            {
                wfHost.Visibility = Visibility.Visible;
                htmlWebView.Visibility = Visibility.Collapsed;
                pnlAudioControls.Visibility = Visibility.Collapsed;
                LaunchFlashGame();
            }
            else if (ViewModel.GameData.Type.Equals("HTML5", StringComparison.OrdinalIgnoreCase))
            {
                wfHost.Visibility = Visibility.Collapsed;
                htmlWebView.Visibility = Visibility.Visible;
                pnlAudioControls.Visibility = Visibility.Visible;
                LaunchHtml5Game();
            }
        }

        private void GameView_Unloaded(object sender, RoutedEventArgs e)
        {
            Dispose();
        }

        private async void LaunchHtml5Game()
        {
            try
            {
                string gameFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "games_data", ViewModel.GameData.Id);

                _htmlServer = new GameHttpServer();
                _htmlServer.Start(gameFolderPath);
                GameDataService.Instance.Log($"Live Server khởi chạy tại cổng 8080 cho game {ViewModel.GameData.Title}");
                GameDataService.Instance.SaveGameAsync(ViewModel.GameData, true);

                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.ServerStatus.Text = "Server Status: Active";
                }

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

                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.ServerStatus.Text = "Flash Player: Active";
                }

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
            _resizeDebounceTimer?.Stop();
            _resizeDebounceTimer?.Start();
        }

        private void CloseGameProcess()
        {
            if (_flashProcess != null && !_flashProcess.HasExited)
            {
                try { _flashProcess.Kill(); } catch { }
                _flashProcess.Dispose();
                _flashProcess = null;
                if (Application.Current.MainWindow is MainWindow main)
                    main.ServerStatus.Text = "Status: Sleep";
            }

            if (_htmlServer != null)
            {
                _htmlServer.Stop();
                _htmlServer = null;

                if (Application.Current.MainWindow is MainWindow main)
                    main.ServerStatus.Text = "Status: Sleep";
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.RequestCloseGame -= CloseGameProcess;
            CloseGameProcess();
        }

        private void OnSetMute(bool isMuted)
        {
            if (htmlWebView?.CoreWebView2 != null)
                htmlWebView.CoreWebView2.IsMuted = isMuted;

            if (_flashProcess?.HasExited == false)
            {
                const int WM_APPCOMMAND = 0x0319;
                const int APPCOMMAND_VOLUME_MUTE = 0x80000;
                SendMessage(_flashProcess.MainWindowHandle, WM_APPCOMMAND, _flashProcess.MainWindowHandle, (IntPtr)APPCOMMAND_VOLUME_MUTE);
            }
        }

        private void OnSetVolume(double volumeRatio)
        {
            if (htmlWebView?.CoreWebView2 != null)
                htmlWebView.CoreWebView2.IsMuted = (volumeRatio == 0 || ViewModel.IsMuted);
        }

        private void OnToggleFullscreen()
        {
            var parent = Window.GetWindow(this);
            if (parent != null)
                parent.WindowState = parent.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        public void Dispose()
        {
            ViewModel.RequestCloseGame -= CloseGameProcess;
            CloseGameProcess();

            if (htmlWebView != null)
            {
                try
                {
                    htmlWebView.Source = new Uri("about:blank");
                    htmlWebView.Dispose();
                }
                catch { }
            }

            GC.SuppressFinalize(this);
        }
    }
}
