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
        private readonly string centralStoragePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "games_data");
        private object _previousView = null;
        private int _previousSidebarIndex = 0;
        public static bool IsNsfwEnabled { get; private set; } = false;

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
            if (SidebarMenu.SelectedIndex != -1)
            {
                NavigateToTab(SidebarMenu.SelectedIndex);
            }
        }

        private void NavigateToTab(int tabIndex)
        {
            if (DynamicContentViewer == null) return;

            switch (tabIndex)
            {
                case 0:
                    DynamicContentViewer.Content = new AllGamesView();
                    DisplayHeader(1);
                    break;
                case 1:
                    DynamicContentViewer.Content = new FlashClassicsView();
                    DisplayHeader(1);
                    break;
                case 2:
                    DynamicContentViewer.Content = new HTML5IndieView();
                    DisplayHeader(1);
                    break;
                case 3:
                    DynamicContentViewer.Content = new FavoritesView();
                    DisplayHeader(1);
                    break;
            }
        }

        public void NavigateToDetail(GameModels selectedGame)
        {
            if (DynamicContentViewer != null)
            {
                if (DynamicContentViewer.Content != null &&
                    !DynamicContentViewer.Content.GetType().Name.Equals("GameView", StringComparison.OrdinalIgnoreCase) &&
                    !DynamicContentViewer.Content.GetType().Name.Equals("DetailGameView", StringComparison.OrdinalIgnoreCase))
                {
                    _previousView = DynamicContentViewer.Content;
                    _previousSidebarIndex = SidebarMenu.SelectedIndex;
                }

                DynamicContentViewer.Content = new DetailGameView(selectedGame);
                SidebarMenu.SelectedIndex = -1;
                DisplayHeader(2);
            }
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            if (_previousView != null)
            {
                DynamicContentViewer.Content = _previousView;
                SidebarMenu.SelectedIndex = _previousSidebarIndex;
                DisplayHeader(1);
            }
            else
            {
                SidebarMenu.SelectedIndex = 0;
                NavigateToTab(0);
            }
        }

        private void DisplayHeader(int a)
        {
            switch (a)
            {
                case 1:
                    DefaultHeaderGrid.Visibility = Visibility.Visible;
                    DetailHeaderGrid.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    DetailHeaderGrid.Visibility= Visibility.Visible;
                    DefaultHeaderGrid.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => this.Close();
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void MaximizeButton_Click(object sender, RoutedEventArgs e) => this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        private void AddGameButton_Click(object sender, RoutedEventArgs e)
        {
            AddGameWindow addDialog = new AddGameWindow();
            addDialog.Owner = this;
            if (addDialog.ShowDialog() == true)
            {
                int currentTab = SidebarMenu.SelectedIndex == -1 ? 0 : SidebarMenu.SelectedIndex;
                SidebarMenu.SelectedIndex = currentTab;
                NavigateToTab(currentTab);
            }
        }
        
        internal void NavigateToList()
        {
            SidebarMenu.SelectedIndex = 0;
            NavigateToTab(0);
        }

        private void NsfwToggle_Checked(object sender, RoutedEventArgs e)
        {
            PasswordDialog authDialog = new PasswordDialog();
            authDialog.Owner = this;

            if (authDialog.ShowDialog() == true)
            {
                IsNsfwEnabled = true;

                int currentTab = SidebarMenu.SelectedIndex == -1 ? 0 : SidebarMenu.SelectedIndex;
        NavigateToTab(currentTab); 
    }
            else
            {
                NsfwToggleButton.IsChecked = false;
                IsNsfwEnabled = false;
            }
        }

        private void NsfwToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            IsNsfwEnabled = false;

            int currentTab = SidebarMenu.SelectedIndex == -1 ? 0 : SidebarMenu.SelectedIndex;
    NavigateToTab(currentTab); 
}
    }
}