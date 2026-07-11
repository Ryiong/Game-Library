using Game_Library.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace Game_Library
{
    /// <summary>
    /// Interaction logic for DetailGameView.xaml
    /// </summary>
    public partial class DetailGameView : UserControl
    {
        private List<string> _slideshowImages = new List<string>();
        private int _currentSlideIndex = 0;
        private const double SlideWidth = 420.0;
        private GameModels _currentGame;
        public DetailGameView(GameModels selectedGame)
        {
            InitializeComponent();
            _currentGame = selectedGame;
            this.DataContext = selectedGame;

            InitializeSlideshow();
            LoadRelatedGames(selectedGame);
        }

        private void InitializeSlideshow()
        {
            if (_currentGame == null) return;

            if (!string.IsNullOrEmpty(_currentGame.Thumbnail))
            {
                _slideshowImages.Add(_currentGame.Thumbnail);
            }
            else
            {
                _slideshowImages.Add("pack://application:,,,/Resources/Thumbnail-Placeholder.jpg");
            }

            if (_currentGame.ImageInGame != null)
            {
                foreach (var img in _currentGame.ImageInGame)
                {
                    if (_slideshowImages.Count >= 6) break;
                    if (!string.IsNullOrEmpty(img)) _slideshowImages.Add(img);
                }
            }

            icSlideshowImages.ItemsSource = _slideshowImages;

            UpdatePaginationDots();
        }

        /// <summary>
        /// Animation Slideshow
        /// </summary>
        private void AnimateSlide()
        {
            if (_slideshowImages.Count == 0) return;

            double targetLeft = -(_currentSlideIndex * SlideWidth);
            DoubleAnimation slideAnimation = new DoubleAnimation
            {
                To = targetLeft,
                Duration = TimeSpan.FromSeconds(0.4),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            SlideshowTranslate.BeginAnimation(TranslateTransform.XProperty, slideAnimation);

            UpdatePaginationDotsColor();
        }

        private void UpdatePaginationDots()
        {
            if (DotsContainer == null) return;
            DotsContainer.Children.Clear();

            for (int i = 0; i < _slideshowImages.Count; i++)
            {
                var dot = new System.Windows.Shapes.Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Margin = new Thickness(4, 0, 4, 0),
                };
                DotsContainer.Children.Add(dot);
            }
            UpdatePaginationDotsColor();
        }

        private void UpdatePaginationDotsColor()
        {
            for (int i = 0; i < DotsContainer.Children.Count; i++)
            {
                if (DotsContainer.Children[i] is System.Windows.Shapes.Ellipse dot)
                {
                    dot.Fill = (i == _currentSlideIndex)
                        ? new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#005B41"))
                        : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                }
            }
        }

        private void PrevSlide_Click(object sender, RoutedEventArgs e)
        {
            if (_slideshowImages.Count <= 1) return;
            _currentSlideIndex--;
            if (_currentSlideIndex < 0) _currentSlideIndex = _slideshowImages.Count - 1; // Vòng lặp lại ảnh cuối
            AnimateSlide();
        }

        private void NextSlide_Click(object sender, RoutedEventArgs e)
        {
            if (_slideshowImages.Count <= 1) return;
            _currentSlideIndex++;
            if (_currentSlideIndex >= _slideshowImages.Count) _currentSlideIndex = 0; // Vòng lặp lại ảnh đầu
            AnimateSlide();
        }

        private void LoadRelatedGames(GameModels currentGame)
        {
            string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "game.json");
            if (!File.Exists(jsonPath)) return;

            try
            {
                string jsonString = File.ReadAllText(jsonPath);
                List<GameModels> allGames = JsonSerializer.Deserialize<List<GameModels>>(jsonString);

                if (allGames != null)
                {
                    List<GameModels> relatedGames = allGames.FindAll(g => g.Title != currentGame.Title);

                    if (relatedGames.Count > 4)
                    {
                        relatedGames = relatedGames.GetRange(0, 4);
                    }

                    icRelatedGamesGrid.ItemsSource = relatedGames;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi nạp Related Games: " + ex.Message);
            }
        }
    }
}
