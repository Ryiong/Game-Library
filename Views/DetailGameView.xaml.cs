using Game_Library.Models;
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
            _slideshowImages.Clear();

            if (!string.IsNullOrEmpty(_currentGame.Thumbnail))
                _slideshowImages.Add(_currentGame.Thumbnail);
            else
                _slideshowImages.Add("Resources/Thumbnail-Placeholder.jpg");

            if (_currentGame.ImageInGame != null)
            {
                foreach (var img in _currentGame.ImageInGame)
                {
                    if (!string.IsNullOrEmpty(img))
                        _slideshowImages.Add(img);
                }
            }

            icSlideshowImages.ItemsSource = _slideshowImages;
            _currentSlideIndex = 0;
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
            if (_currentGame == null) return;

            if (Application.Current.MainWindow is MainWindow main)
            {
                main.DynamicContentViewer.Content = new GameView(_currentGame);
            }
        }

        private void LoadRelatedGames()
        {
            string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "game.json");
            if (!File.Exists(jsonPath)) return;

            try
            {
                var allGames = JsonSerializer.Deserialize<List<GameModels>>(File.ReadAllText(jsonPath));
                if (allGames == null) return;

                var relatedGames = allGames.FindAll(g =>
                    g.Id != _currentGame.Id && (
                        g.SeriesId == _currentGame.Id ||
                        (!string.IsNullOrEmpty(_currentGame.SeriesId) && g.Id == _currentGame.SeriesId) ||
                        (!string.IsNullOrEmpty(_currentGame.SeriesId) && g.SeriesId == _currentGame.SeriesId)
                    )
                );

                if (relatedGames != null && relatedGames.Count > 0)
                {
                    icRelatedGamesGrid.ItemsSource = relatedGames;
                    icRelatedGamesGrid.Visibility = Visibility.Visible;
                    txtNoRelatedGames.Visibility = Visibility.Collapsed;
                }
                else
                {
                    icRelatedGamesGrid.ItemsSource = null;
                    icRelatedGamesGrid.Visibility = Visibility.Collapsed;
                    txtNoRelatedGames.Visibility = Visibility.Visible;
                }

                icRelatedGamesGrid.ItemsSource = relatedGames;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi tải danh sách game liên quan: " + ex.Message);
            }
        }

        private void RelatedGameCard_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is GameModels selectedGame && Application.Current.MainWindow is MainWindow main)
            {
                main.NavigateToDetail(selectedGame);
    }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentGame == null) return;

            var editWindow = new AddGameWindow(_currentGame);
            editWindow.Owner = Application.Current.MainWindow;
            if (editWindow.ShowDialog() == true)
            {
                if (Application.Current.MainWindow is MainWindow main)
                {
                    main.NavigateToDetail(_currentGame);  
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentGame == null) return;

            var result = MessageBox.Show(
                $"Bạn có chắc muốn xóa game '{_currentGame.Title}'?\nHành động này không thể hoàn tác!",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "game.json");

                    if (File.Exists(jsonPath))
                    {
                        var games = JsonSerializer.Deserialize<List<GameModels>>(File.ReadAllText(jsonPath));
                        var gameToRemove = games.FirstOrDefault(g => g.Id == _currentGame.Id);

                        if (gameToRemove != null)
                        {
                            games.Remove(gameToRemove);
                            File.WriteAllText(jsonPath, JsonSerializer.Serialize(games, new JsonSerializerOptions { WriteIndented = true }));

                            string gameFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "games_data", _currentGame.Id);
                            if (Directory.Exists(gameFolder))
                            {
                                Directory.Delete(gameFolder, true);
                            }

                            MessageBox.Show("Đã xóa game thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                            if (Application.Current.MainWindow is MainWindow main)
                            {
                                main.NavigateToList();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa game: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}