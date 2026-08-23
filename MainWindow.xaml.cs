using Game_Library.Models;
using Game_Library.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;

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
        public void NavigateToPlay(GameModels selectedGame) => ViewModel.NavigateToPlay(selectedGame);
        public void NavigateToList() => ViewModel.SidebarSelectedIndex = 0;

        private void Window_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null || files.Length == 0) return;

            string droppedPath = files[0];

            if (Directory.Exists(droppedPath))
            {
                var addDialog = new AddGameWindow();
                addDialog.Owner = this;

                if (addDialog.DataContext is AddGameViewModel vm)
                {
                    vm.LoadFromDirectory(droppedPath);
                }
                if (addDialog.ShowDialog() == true)
                {
                    if (DataContext is MainViewModel mainVM)
                    {
                        int currentTab = mainVM.SidebarSelectedIndex == -1 ? 0 : mainVM.SidebarSelectedIndex;
                        mainVM.SidebarSelectedIndex = currentTab;
                    }
                }
            }    
        }

        #region Window System Action (Giữ nguyên cấu trúc điều khiển kéo đóng cửa sổ)
        private void CloseButton_Click(object sender, RoutedEventArgs e) => this.Close();
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void MaximizeButton_Click(object sender, RoutedEventArgs e) => this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        #endregion
    }
}
