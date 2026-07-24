using Game_Library.Models;
using Game_Library.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace Game_Library.Views
{
    public partial class DetailGameView : System.Windows.Controls.UserControl
    {
        public DetailGameViewModel ViewModel { get; private set; }

        public DetailGameView(GameModels selectedGame)
        {
            InitializeComponent();
            ViewModel = new DetailGameViewModel(selectedGame);
            ViewModel.ReleaseMediaAction = ClearMediaElements;
            this.DataContext = ViewModel;

            this.Unloaded += DetailGameView_Unloaded;

            ViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ViewModel.SlideshowImages))
                {
                    UpdatePaginationDots();
                }
            };
            UpdatePaginationDots();
        }

        private void DetailGameView_Unloaded(object sender, RoutedEventArgs e)
        {
            ClearMediaElements();
            this.Unloaded -= DetailGameView_Unloaded;
            //try
            //{
            //    var itemsControl = icSlideshowImages;
            //    if (itemsControl != null)
            //    {
            //        for (int i = 0; i < itemsControl.Items.Count; i++)
            //        {
            //            var container = itemsControl.ItemContainerGenerator.ContainerFromIndex(i) as ContentPresenter;
            //            if (container != null && VisualTreeHelper.GetChildrenCount(container) > 0)
            //            {
            //                var grid = VisualTreeHelper.GetChild(container, 0) as Grid;
            //                if (grid != null)
            //                {
            //                    foreach (var child in grid.Children)
            //                    {
            //                        if (child is MediaElement mediaElement)
            //                        {
            //                            mediaElement.Stop();
            //                            mediaElement.Source = null; // Gỡ luồng Uri để HĐH tự động giải phóng RAM ngay lập tức
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            //catch { /* Chặn lỗi âm thầm khi dọn bộ nhớ */ }
        }

        private void UpdatePaginationDots()
        {
            if (ViewModel == null || ViewModel.SlideshowImages == null) return;

            DotsContainer.Children.Clear();
            for (int i = 0; i < ViewModel.SlideshowImages.Count; i++)
            {
                DotsContainer.Children.Add(new System.Windows.Shapes.Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Margin = new Thickness(4, 0, 4, 0),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(
                        i == ViewModel.CurrentSlideIndex ? "#046B59" : "#D4E6E2"))
                });
            }
        }

        private void AnimateSlide()
        {
            double currentWidth = SlideshowViewContainer.ActualWidth > 0 ? SlideshowViewContainer.ActualWidth : 380.0;
            DoubleAnimation slideAnimation = new DoubleAnimation
            {
                To = -(ViewModel.CurrentSlideIndex * currentWidth),
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            SlideshowTranslate.BeginAnimation(TranslateTransform.XProperty, slideAnimation);
            UpdatePaginationDots();
            try
            {
                var container = icSlideshowImages.ItemContainerGenerator.ContainerFromIndex(ViewModel.CurrentSlideIndex) as ContentPresenter;
                if (container != null && VisualTreeHelper.GetChildrenCount(container) > 0)
                {
                    var grid = VisualTreeHelper.GetChild(container, 0) as Grid;
                    if (grid != null)
                    {
                        foreach (var child in grid.Children)
                        {
                            if (child is MediaElement gif && gif.Visibility == Visibility.Visible)
                            {
                                gif.Position = TimeSpan.FromMilliseconds(1);
                                gif.Play();
                            }
                        }
                    }
                }
            }
            catch { /* Chặn lỗi truy vấn UI visual tree */ }
        }

        private void PrevSlide_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SlideshowImages.Count <= 1) return;
            ViewModel.CurrentSlideIndex = (ViewModel.CurrentSlideIndex == 0)
                ? ViewModel.SlideshowImages.Count - 1
                : ViewModel.CurrentSlideIndex - 1;
            AnimateSlide();
        }

        private void NextSlide_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SlideshowImages.Count <= 1) return;
            ViewModel.CurrentSlideIndex = (ViewModel.CurrentSlideIndex == ViewModel.SlideshowImages.Count - 1)
                ? 0
                : ViewModel.CurrentSlideIndex + 1;
            AnimateSlide();
        }

        private void GifPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (sender is MediaElement mediaElement)
            {
                mediaElement.Play();
            }
        }

        private void GifPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (sender is MediaElement mediaElement)
            {
                mediaElement.Position = TimeSpan.FromMilliseconds(1);
                mediaElement.Play();
            }
        }

        public void ClearMediaElements()
        {
            try
            {
                if (icSlideshowImages != null)
                {
                    icSlideshowImages.ItemsSource = null;
                }
                if (icSlideshowImages != null)
                {
                    for (int i = 0; i < icSlideshowImages.Items.Count; i++)
                    {
                        var container = icSlideshowImages.ItemContainerGenerator.ContainerFromIndex(i) as System.Windows.Controls.ContentPresenter;
                        if (container != null && System.Windows.Media.VisualTreeHelper.GetChildrenCount(container) > 0)
                        {
                            var grid = System.Windows.Media.VisualTreeHelper.GetChild(container, 0) as System.Windows.Controls.Grid;
                            if (grid != null)
                            {
                                foreach (var child in grid.Children)
                                {
                                    if (child is System.Windows.Controls.MediaElement mediaElement)
                                    {
                                        mediaElement.Stop();
                                        mediaElement.Close();
                                        mediaElement.Source = null;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch {}
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            ClearMediaElements();
            ViewModel.EditGameCommand.Execute(null);
        }
    }
}