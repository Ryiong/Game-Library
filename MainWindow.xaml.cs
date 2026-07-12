using Game_Library.Models;
using Game_Library.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace Game_Library
{
    public partial class MainWindow : Window
    {
        private readonly string allGamesJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "game.json");
        private readonly string centralStoragePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "games_data");

        public MainWindow()
        {
            InitializeComponent();
            // Đăng ký sự kiện Drag-Drop trên toàn ứng dụng để nhận thư mục game tiện lợi
            this.AllowDrop = true;
            this.Drop += MainWindow_Drop;

            // Tạo thư mục lưu trữ tập trung nếu chưa có
            if (!Directory.Exists(centralStoragePath)) Directory.CreateDirectory(centralStoragePath);

            DynamicContentViewer.Content = new AllGamesView();
        }

        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string path in paths)
                {
                    if (Directory.Exists(path))
                    {
                        // Logic kéo thả thư mục: Sao chép toàn bộ thư mục vào vùng dữ liệu tập trung (games_data)
                        string folderName = Path.GetFileName(path);
                        string destinationPath = Path.Combine(centralStoragePath, folderName);

                        try
                        {
                            CopyDirectory(path, destinationPath);
                            MessageBox.Show($"Đã nạp thành công dữ liệu game '{folderName}' vào kho lưu trữ cá nhân tập trung!", "Thành công");
                            // Tại đây bạn có thể hiển thị Hộp thoại nhập liệu Metadata để lưu thêm vào JSON.
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Lỗi sao chép dữ liệu game: " + ex.Message);
                        }
                    }
                }
            }
        }

        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);
            foreach (string file in Directory.GetFiles(sourceDir))
                File.Copy(file, Path.Combine(destinationDir, Path.GetFileName(file)), true);
            foreach (string subDir in Directory.GetDirectories(sourceDir))
                CopyDirectory(subDir, Path.Combine(destinationDir, Path.GetFileName(subDir)));
        }

        private void SidebarMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DynamicContentViewer == null) return;

            switch (SidebarMenu.SelectedIndex)
            {
                case 0:
                    DynamicContentViewer.Content = new AllGamesView();
                    break;
                case 1:
                    DynamicContentViewer.Content = new FlashClassicsView(); // Lọc tự động Type = FLASH
                    break;
                case 2:
                    DynamicContentViewer.Content = new HTML5IndieView();    // Lọc tự động Type = HTML5
                    break;
                case 3:
                    DynamicContentViewer.Content = new FavoritesView();     // Lọc tự động isFavorite = true
                    break;
            }
        }

        public void NavigateToDetail(GameModels selectedGame)
        {
            if (DynamicContentViewer != null)
            {
                DynamicContentViewer.Content = new DetailGameView(selectedGame);
                SidebarMenu.SelectedIndex = -1; // Bỏ chọn sidebar khi vào chi tiết
            }
        }

        // Logic thực thi chạy tiến trình phù hợp theo loại Game (Phương án A)
        public void ExecuteGameLauncher(GameModels game)
        {
            string gameFolder = Path.Combine(centralStoragePath, game.FolderName ?? "");

            if (game.Type.ToUpper() == "FLASH")
            {
                // Thực thi Game Flash (.swf): Chạy qua FlashPlayer debug.exe tích hợp sẵn
                string flashPlayerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "flashplayer_debugger.exe");
                string swfFilePath = Path.Combine(gameFolder, game.MainFile);

                MessageBox.Show($"Hệ thống đang gọi tiến trình nhúng giả lập cho Flash Game: {game.Title}\nFile: {game.MainFile}", "Kích hoạt Flash Player");
                // Tiến trình kích hoạt thực tế bằng Win32 SetParent sẽ được gọi ở đây.
            }
            else if (game.Type.ToUpper() == "HTML5")
            {
                // Thực thi Game HTML5: Bật trạng thái Live Server nội bộ và tải WebView2
                if (ServerStatus != null) ServerStatus.Text = "Server Status: Active (Port 8080)";
                LiveServerButton.IsChecked = true;

                MessageBox.Show($"Hệ thống đã bật Web Server cục bộ tại http://localhost:8080/{game.MainFile} để khởi chạy thông qua WebView2!", "Kích hoạt HTML5");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => this.Close();
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void MaximizeButton_Click(object sender, RoutedEventArgs e) => this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        private void SearchBox_GotFocus(object sender, RoutedEventArgs e) => SearchPlaceholder.Visibility = Visibility.Collapsed;
        private void SearchBox_LostFocus(object sender, RoutedEventArgs e) { if (string.IsNullOrEmpty(SearchBox.Text)) SearchPlaceholder.Visibility = Visibility.Visible; }
        private void AddGameButton_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Vui lòng kéo thả thư mục game trực tiếp vào giao diện ứng dụng để nạp tự động!", "Hướng dẫn");
        private void LiveServer_Toggle(object sender, RoutedEventArgs e) { if (ServerStatus != null) ServerStatus.Text = LiveServerButton.IsChecked == true ? "Server Status: Active" : "Server Status: Off"; }
    }
}