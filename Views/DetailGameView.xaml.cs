using Game_Library.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Game_Library.Views
{
    public partial class DetailGameView : UserControl
    {
        private List<string> _slideshowImages = new List<string>();
        private int _currentSlideIndex = 0;
        private const double SlideWidth = 425.0;
        private GameModels _currentGame;

        public DetailGameView(GameModels selectedGame)
        {
            InitializeComponent();
            _currentGame = selectedGame;
            this.DataContext = selectedGame;
            InitializeSlideshow();
            UpdateFavoriteUI();
            LoadRelatedGames();
        }

        private void InitializeSlideshow()
        {
            _slideshowImages.Add(!string.IsNullOrEmpty(_currentGame.Thumbnail) ? _currentGame.Thumbnail : "Resources/Thumbnail-Placeholder.jpg");
            if (_currentGame.ImageInGame != null)
            {
                foreach (var img in _currentGame.ImageInGame)
                    if (!string.IsNullOrEmpty(img)) _slideshowImages.Add(img);
            }
            icSlideshowImages.ItemsSource = _slideshowImages;
            UpdatePaginationDots();
        }

        private void UpdatePaginationDots()
        {
            DotsContainer.Children.Clear();
            for (int i = 0; i < _slideshowImages.Count; i++)
            {
                DotsContainer.Children.Add(new System.Windows.Shapes.Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Margin = new Thickness(4, 0, 4, 0),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(i == _currentSlideIndex ? "#046B59" : "#D4E6E2"))
                });
            }
        }

        private void AnimateSlide()
        {
            DoubleAnimation slideAnimation = new DoubleAnimation
            {
                To = -(_currentSlideIndex * SlideWidth),
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            SlideshowTranslate.BeginAnimation(TranslateTransform.XProperty, slideAnimation);
            UpdatePaginationDots();
        }

        private void PrevSlide_Click(object sender, RoutedEventArgs e)
        {
            if (_slideshowImages.Count <= 1) return;
            _currentSlideIndex = (_currentSlideIndex == 0) ? _slideshowImages.Count - 1 : _currentSlideIndex - 1;
            AnimateSlide();
        }

        private void NextSlide_Click(object sender, RoutedEventArgs e)
        {
            if (_slideshowImages.Count <= 1) return;
            _currentSlideIndex = (_currentSlideIndex == _slideshowImages.Count - 1) ? 0 : _currentSlideIndex + 1;
            AnimateSlide();
        }

        private void Favorite_Click(object sender, RoutedEventArgs e)
        {
            _currentGame.isFavorite = !_currentGame.isFavorite;
            UpdateFavoriteUI();

            // Cập nhật trạng thái vào file JSON đồng bộ
            string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "game.json");
            if (File.Exists(jsonPath))
            {
                try
                {
                    var list = JsonSerializer.Deserialize<List<GameModels>>(File.ReadAllText(jsonPath));
                    var game = list.Find(g => g.Title == _currentGame.Title);
                    if (game != null) { game.isFavorite = _currentGame.isFavorite; }
                    File.WriteAllText(jsonPath, JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }));
                }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
            }
        }

        private void UpdateFavoriteUI()
        {
            if (_currentGame.isFavorite)
            {
                FavText.Text = "Đã Yêu thích";
                FavButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4E6E2"));
            }
            else
            {
                FavText.Text = "Yêu thích";
                FavButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EDF2F0"));
            }
        }

        private void PlayNow_Click(object sender, RoutedEventArgs e)
        {
            // Lưu lịch sử chơi vào recent_game.json
            try
            {
                _currentGame.LastPlayedText = "Vừa chơi xong";
                string recentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "recent_game.json");
                File.WriteAllText(recentPath, JsonSerializer.Serialize(_currentGame, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }

            // Gọi điều hướng kích hoạt tiến trình cụ thể từ MainWindow
            if (Application.Current.MainWindow is MainWindow main)
            {
                main.ExecuteGameLauncher(_currentGame);
            }
        }

        private void LoadRelatedGames()
        {
            string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "game.json");
            if (!File.Exists(jsonPath)) return;
            try
            {
                var allGames = JsonSerializer.Deserialize<List<GameModels>>(File.ReadAllText(jsonPath));
                icRelatedGamesGrid.ItemsSource = allGames.FindAll(g => g.Title != _currentGame.Title);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        }
    }
}