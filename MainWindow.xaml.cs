using Game_Library.Models;
using Game_Library.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Game_Library
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private static bool _isNsfwEnabled = false;
        public static bool IsNsfwEnabled
        {
            get => _isNsfwEnabled;
            set
            {
                _isNsfwEnabled = value;
                Instance?.OnPropertyChanged(nameof(IsNsfwEnabled));
            }
        }
        public static void SetNsfwStatus(bool status)
        {
            IsNsfwEnabled = status;
        }
        public static MainWindow Instance { get; private set; }
        public MainViewModel ViewModel { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public MainWindow()
        {
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
            Instance = this;
            ViewModel = new MainViewModel();
            this.DataContext = ViewModel;
        }
        public void NavigateToDetail(GameModels selectedGame) => ViewModel.NavigateToDetail(selectedGame);
        public void NavigateToPlay(GameModels selectedGame) => ViewModel.NavigateToPlay(selectedGame);
        public void NavigateToList() => ViewModel.SidebarSelectedIndex = 0;

        #region Window System Action (Giữ nguyên cấu trúc điều khiển kéo đóng cửa sổ)
        private void CloseButton_Click(object sender, RoutedEventArgs e) => this.Close();
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void MaximizeButton_Click(object sender, RoutedEventArgs e) => this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        #endregion
    }
}
