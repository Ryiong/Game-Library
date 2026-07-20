using Game_Library.Extensions;
using Game_Library.Models;
using Game_Library.ViewModels;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using Image = System.Windows.Controls.Image;
using ListBox = System.Windows.Controls.ListBox;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;


namespace Game_Library
{
    /// <summary>
    /// Interaction logic for AddGameWindow.xaml
    /// </summary>
    public partial class AddGameWindow : Window
    {
        public AddGameViewModel ViewModel { get; private set; }

        public AddGameWindow(GameModels gameToEdit = null)
        {
            InitializeComponent();

            ViewModel = new AddGameViewModel(gameToEdit);
            this.DataContext = ViewModel;
        }

        private void GifPreviewPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.MediaElement mediaElement)
            {
                mediaElement.Position = TimeSpan.FromMilliseconds(1);
                mediaElement.Play();
            }
        }

        public void ClearPreviewMedia()
        {
            try
            {
                foreach (var media in FindVisualChildren<MediaElement>(this))
                {
                    media.Stop();
                    media.Source = null;
                }
            }
            catch { }
        }

        #region Window System Window Action (Giữ nguyên các hàm điều hướng kéo đóng thanh bar)
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            ClearPreviewMedia();
            this.Close();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void MaximizeButton_Click(object sender, RoutedEventArgs e) => this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        #endregion

        private static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield break;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T t) yield return t;
                foreach (T childOfChild in FindVisualChildren<T>(child)) yield return childOfChild;
            }
        }
    }
}
