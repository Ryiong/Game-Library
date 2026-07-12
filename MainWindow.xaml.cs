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
        private object _previousView = null;
        private int _previousSidebarIndex = 0;

        public MainWindow()
        {
            InitializeComponent();

            if (!Directory.Exists(centralStoragePath)) Directory.CreateDirectory(centralStoragePath);

            DynamicContentViewer.Content = new AllGamesView();
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
                    DynamicContentViewer.Content = new FlashClassicsView();
                    break;
                case 2:
                    DynamicContentViewer.Content = new HTML5IndieView();  
                    break;
                case 3:
                    DynamicContentViewer.Content = new FavoritesView(); 
                    break;
            }
        }

        public void NavigateToDetail(GameModels selectedGame)
        {
            if (DynamicContentViewer != null)
            {
                _previousView = DynamicContentViewer.Content;
                _previousSidebarIndex = SidebarMenu.SelectedIndex;
                DynamicContentViewer.Content = new DetailGameView(selectedGame);
                SidebarMenu.SelectedIndex = -1;
                DefaultHeaderGrid.Visibility = Visibility.Collapsed;
                DetailHeaderGrid.Visibility = Visibility.Visible;
            }
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            if (_previousView != null)
            {
                DynamicContentViewer.Content = _previousView;
                SidebarMenu.SelectedIndex = _previousSidebarIndex;

                DefaultHeaderGrid.Visibility = Visibility.Visible;
                DetailHeaderGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                SidebarMenu.SelectedIndex = 0;
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