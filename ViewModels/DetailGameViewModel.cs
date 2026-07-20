using Game_Library.Models;
using Game_Library.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Game_Library.ViewModels
{
    public class DetailGameViewModel : ViewModelBase
    {
        private GameModels _currentGame;
        private ObservableCollection<string> _slideshowImages;
        private ObservableCollection<GameModels> _relatedGames;
        private int _currentSlideIndex = 0;
        private string _favoriteButtonText;
        private string _favoriteButtonBackground;
        private Visibility _noRelatedGamesVisibility = Visibility.Collapsed;
        private Visibility _relatedGamesGridVisibility = Visibility.Visible;

        public GameModels CurrentGame
        {
            get => _currentGame;
            set => SetProperty(ref _currentGame, value);
        }

        public ObservableCollection<string> SlideshowImages
        {
            get => _slideshowImages;
            set => SetProperty(ref _slideshowImages, value);
        }

        public ObservableCollection<GameModels> RelatedGames
        {
            get => _relatedGames;
            set => SetProperty(ref _relatedGames, value);
        }

        public int CurrentSlideIndex
        {
            get => _currentSlideIndex;
            set => SetProperty(ref _currentSlideIndex, value);
        }

        public string FavoriteButtonText
        {
            get => _favoriteButtonText;
            set => SetProperty(ref _favoriteButtonText, value);
        }

        public string FavoriteButtonBackground
        {
            get => _favoriteButtonBackground;
            set => SetProperty(ref _favoriteButtonBackground, value);
        }

        public Visibility NoRelatedGamesVisibility
        {
            get => _noRelatedGamesVisibility;
            set => SetProperty(ref _noRelatedGamesVisibility, value);
        }

        public Visibility RelatedGamesGridVisibility
        {
            get => _relatedGamesGridVisibility;
            set => SetProperty(ref _relatedGamesGridVisibility, value);
        }

        public ICommand ToggleFavoriteCommand { get; }
        public ICommand PlayNowCommand { get; }
        public ICommand EditGameCommand { get; }
        public ICommand DeleteGameCommand { get; }
        public ICommand OpenRelatedGameCommand { get; }

        public DetailGameViewModel(GameModels game)
        {
            CurrentGame = game;
            SlideshowImages = new ObservableCollection<string>();
            RelatedGames = new ObservableCollection<GameModels>();

            ToggleFavoriteCommand = new RelayCommand(_ => ExecuteToggleFavorite());
            PlayNowCommand = new RelayCommand(_ => ExecutePlayNow());
            EditGameCommand = new RelayCommand(_ => ExecuteEditGame());
            DeleteGameCommand = new RelayCommand(_ => _ = ExecuteDeleteGameAsync());
            OpenRelatedGameCommand = new RelayCommand(p => ExecuteOpenRelatedGame(p));

            LoadGameData();
        }

        public void LoadGameData()
        {
            if (CurrentGame == null) return;

            SlideshowImages.Clear();
            if (!string.IsNullOrEmpty(CurrentGame.Thumbnail))
                SlideshowImages.Add(CurrentGame.Thumbnail);
            else
                SlideshowImages.Add("Resources/Thumbnail-Placeholder.jpg");

            if (CurrentGame.ImageInGame != null)
            {
                foreach (var img in CurrentGame.ImageInGame)
                {
                    if (!string.IsNullOrEmpty(img) && !SlideshowImages.Contains(img))
                    {
                        SlideshowImages.Add(img);
                    }
                }
            }

            UpdateFavoriteUI();

            var sourceList = GameDataService.Instance.AllGames;
            var related = sourceList.Where(g =>
                g.Id != CurrentGame.Id && (
                (CurrentGame.RelatedGameIds != null && CurrentGame.RelatedGameIds.Contains(g.Id)) ||
                (g.RelatedGameIds != null && g.RelatedGameIds.Contains(CurrentGame.Id))
            )).ToList();

            RelatedGames = new ObservableCollection<GameModels>(related);

            RelatedGamesGridVisibility = related.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            NoRelatedGamesVisibility = related.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void UpdateFavoriteUI()
        {
            FavoriteButtonText = CurrentGame.isFavorite ? "Đã Yêu thích" : "Yêu thích";
            FavoriteButtonBackground = CurrentGame.isFavorite ? "#D4E6E2" : "#EDF2F0";
        }

        private void ExecuteToggleFavorite()
        {
            CurrentGame.isFavorite = !CurrentGame.isFavorite;
            UpdateFavoriteUI();
            
            _ = GameDataService.Instance.SaveGameAsync(CurrentGame, isEditMode: true);
        }

        private void ExecutePlayNow()
        {
            MainWindow.Instance.NavigateToPlay(CurrentGame);
        }

        private void ExecuteOpenRelatedGame(object parameter)
        {
            if (parameter is GameModels selectedGame)
            {
                MainWindow.Instance.NavigateToDetail(selectedGame);
            }
        }

        private void ExecuteEditGame()
        {
            AddGameWindow editWindow = new AddGameWindow(CurrentGame);
            editWindow.Owner = Application.Current.MainWindow;
            if (editWindow.ShowDialog() == true)
            {
                LoadGameData();
                MainWindow.Instance.NavigateToDetail(CurrentGame);
            }
        }

        private async Task ExecuteDeleteGameAsync()
        {
            var result = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa game '{CurrentGame.Title}' khỏi thư viện?\nHành động này sẽ xóa sạch dữ liệu nguồn trên ổ cứng và không thể hoàn tác!",
                "Xác nhận gỡ bỏ game",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                string centralStoragePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "games_data");

                bool isDeleted = await GameDataService.Instance.DeleteGameAsync(CurrentGame.Id, centralStoragePath);

                if (isDeleted)
                {
                    MessageBox.Show("Đã gỡ bỏ tệp tin và dọn dẹp liên kết game thành công!", "Xác nhận sạch dữ liệu", MessageBoxButton.OK, MessageBoxImage.Information);
                    MainWindow.Instance.NavigateToList();
                }
                else
                {
                    MessageBox.Show("Hệ thống không thể xóa thư mục gốc của game do tệp tin đang bị một tiến trình khác khóa.", "Lỗi xóa file", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
