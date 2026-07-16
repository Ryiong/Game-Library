using Game_Library.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        private readonly GameModels _gameData;
        private Process _flashProcess = null;

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
            _gameData = selectedGame;
            lblPlayingGameTitle.Text = _gameData.Title.ToUpper();

            Loaded += (s, e) => LaunchFlashGame();
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

                string fullSwfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "games_data", _gameData.Id, _gameData.MainFile);

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

        private void btnExitGame_Click(object sender, RoutedEventArgs e)
        {
            CloseGameProcess();

            if (Application.Current.MainWindow is MainWindow main)
            {
                main.NavigateToDetail(_gameData);
            }
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
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            CloseGameProcess();
        }
    }
}
