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
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using MessageBox = System.Windows.MessageBox;

namespace Game_Library.Views
{
    public partial class DetailGameView : System.Windows.Controls.UserControl
    {
        private const double SlideWidth = 425.0;
        public DetailGameViewModel ViewModel { get; private set; }

        public DetailGameView(GameModels selectedGame)
        {
            InitializeComponent();
            ViewModel = new DetailGameViewModel(selectedGame);
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
            this.Unloaded -= DetailGameView_Unloaded;
            try
            {
                var itemsControl = icSlideshowImages;
                if (itemsControl != null)
                {
                    for (int i = 0; i < itemsControl.Items.Count; i++)
                    {
                        var container = itemsControl.ItemContainerGenerator.ContainerFromIndex(i) as ContentPresenter;
                        if (container != null && VisualTreeHelper.GetChildrenCount(container) > 0)
                        {
                            var grid = VisualTreeHelper.GetChild(container, 0) as Grid;
                            if (grid != null)
                            {
                                foreach (var child in grid.Children)
                                {
                                    if (child is MediaElement mediaElement)
                                    {
                                        mediaElement.Stop();
                                        mediaElement.Source = null; // Gỡ luồng Uri để HĐH tự động giải phóng RAM ngay lập tức
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { /* Chặn lỗi âm thầm khi dọn bộ nhớ */ }
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
            DoubleAnimation slideAnimation = new DoubleAnimation
            {
                To = -(ViewModel.CurrentSlideIndex * SlideWidth),
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            SlideshowTranslate.BeginAnimation(TranslateTransform.XProperty, slideAnimation);
            UpdatePaginationDots();
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
    }
}