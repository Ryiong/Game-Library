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
    public class FutureGamesViewModel : ViewModelBase
    {
        public ObservableCollection<FutureGameModel> FutureGames { get; set; } = new ObservableCollection<FutureGameModel>();

        public ICommand OpenAddFutureGameCommand { get; }
        public ICommand ToggleActivationCommand { get; }
        public ICommand ImportToLibraryCommand { get; }
        public ICommand DeleteFutureGameCommand { get; }

        public FutureGamesViewModel()
        {
            OpenAddFutureGameCommand = new RelayCommand(_ => ExecuteOpenAdd());
            ToggleActivationCommand = new RelayCommand(async p => await ExecuteToggleAsync(p as FutureGameModel));
            ImportToLibraryCommand = new RelayCommand(p => ExecuteImportToLibrary(p as FutureGameModel));
            DeleteFutureGameCommand = new RelayCommand(async p => await ExecuteDeleteFutureGameAsync(p as FutureGameModel));

            LoadData();
        }

        private void LoadData()
        {
            FutureGames.Clear();
            foreach (var item in FutureGameDataService.Instance.FutureGames)
            {
                FutureGames.Add(item);
            }
        }

        private void ExecuteOpenAdd()
        {
            AddFutureGameWindow window = new AddFutureGameWindow();
            window.Owner = Application.Current.MainWindow;
            if (window.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private async Task ExecuteToggleAsync(FutureGameModel game)
        {
            if (game == null) return;
            await FutureGameDataService.Instance.SaveFutureGamesAsync();
        }

        private async void ExecuteImportToLibrary(FutureGameModel futureGame)
        {
            if (futureGame == null) return;

            var result = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa game tương lai '{futureGame.Title}'?\nHành động này sẽ xóa dữ liệu và không thể hoàn tác!",
                "Xác nhận xóa Future Game",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                GameModels prefilledGame = new GameModels
                {
                    Title = futureGame.Title,
                    Type = futureGame.Type,
                    Description = futureGame.Description,
                    isNSFW = false,
                    ReleaseDate = futureGame.ReleaseDate,
                    Thumbnail = futureGame.Thumbnail,
                    RelatedGameIds = futureGame.RelatedGameIds,
                    AddedDate = DateTime.Now.ToString("yyyy-MM-dd")
                };

                AddGameWindow addWindow = new AddGameWindow(prefilledGame);
                addWindow.Owner = Application.Current.MainWindow;

                if (addWindow.ShowDialog() == true)
                {
                    DeleteFutureGame(futureGame);

                    await FutureGameDataService.Instance.SaveFutureGamesAsync();
                    GameDataService.Instance.Log($"[FUTURE GAME] Đã chuyển game '{futureGame.Title}' trực tiếp vào thư viện chính.");
                }
            }
        }
        private async Task ExecuteDeleteFutureGameAsync(FutureGameModel futureGame)
        {
            if (futureGame == null) return;

            var result = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa game tương lai '{futureGame.Title}'?\nHành động này sẽ xóa dữ liệu và không thể hoàn tác!",
                "Xác nhận xóa Future Game",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DeleteFutureGame(futureGame);

                await FutureGameDataService.Instance.SaveFutureGamesAsync();
                GameDataService.Instance.Log($"[FUTURE GAME] Đã xóa game tương lai '{futureGame.Title}' khỏi danh sách.");
            }
        }

        public void DeleteFutureGame(FutureGameModel futureGame)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            try
            {
                if (!string.IsNullOrEmpty(futureGame.Thumbnail))
                {
                    string fullThumbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, futureGame.Thumbnail);
                    string futureGameFolder = Path.GetDirectoryName(fullThumbPath);

                    if (Directory.Exists(futureGameFolder) && futureGameFolder.Contains("future_games_data"))
                    {
                        Directory.Delete(futureGameFolder, recursive: true);
                    }
                }
            }
            catch (Exception ex)
            {
                GameDataService.Instance.Log($"[CLEANUP WARNING] Không thể xóa file tạm của Future Game: {ex.Message}");
            }

            FutureGameDataService.Instance.FutureGames.Remove(futureGame);
            FutureGames.Remove(futureGame);
        }
    }
}
