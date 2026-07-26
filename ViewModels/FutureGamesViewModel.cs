using Game_Library.Models;
using Game_Library.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Game_Library.ViewModels
{
    public class FutureGamesViewModel : ViewModelBase
    {
        private string _searchKeyword = string.Empty;
        private List<FutureGameModel> _allFutureGames = new List<FutureGameModel>();
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

            _ = LoadDataAsync();
        }

        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                if (SetProperty(ref _searchKeyword, value))
                {
                    ApplyFilter();
                }
            }
        }

        public async Task LoadDataAsync()
        {
            _allFutureGames = await Task.Run(() => FutureGameDataService.Instance.FutureGames.ToList()); 

            ApplyFilter();
        }

        public async void ApplyFilter()
        {
            string rawKeyword = SearchKeyword ?? string.Empty;
            var filteredResult = await Task.Run(() =>
            {
                string normalizedKeyword = RemoveDiacritics(rawKeyword.Trim().ToLower());
                string[] tokens = normalizedKeyword.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                return _allFutureGames.Where(game =>
                {
                    if (tokens.Length == 0) return true;

                    string titleNorm = RemoveDiacritics(game.Title ?? "").ToLower();
                    string typeNorm = RemoveDiacritics(game.Type ?? "").ToLower();
                    string descNorm = RemoveDiacritics(game.Description ?? "").ToLower();

                    string fullText = $"{titleNorm} {typeNorm} {descNorm}";

                    return tokens.All(token => fullText.Contains(token));
                }).ToList();
            });

            if (Application.Current != null)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    FutureGames.Clear();
                    foreach (var item in filteredResult)
                    {
                        FutureGames.Add(item);
                    }
                }, DispatcherPriority.Background);
            }
        }

        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            string normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC).Replace('đ', 'd').Replace('Đ', 'D');
        }

        private void ExecuteOpenAdd()
        {
            AddFutureGameWindow window = new AddFutureGameWindow();
            window.Owner = Application.Current.MainWindow;
            if (window.ShowDialog() == true)
            {
                LoadDataAsync();
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
                $"Bạn có chắc chắn muốn thêm game '{futureGame.Title}' vào thư viện?\nHành động này sẽ xóa dữ liệu và không thể hoàn tác!",
                "Xác nhận thêm vào thư viện!",
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

                AddGameViewModel addVm = new AddGameViewModel(prefilledGame, isImportFromFuture: true);
                AddGameWindow addWindow = new AddGameWindow();
                addWindow.DataContext = addVm;
                addWindow.Owner = Application.Current.MainWindow;

                if (addWindow.ShowDialog() == true)
                {
                    DeleteFutureGame(futureGame);

                    await FutureGameDataService.Instance.SaveFutureGamesAsync();
                    if (Application.Current.MainWindow is MainWindow mainWindow)
                    {
                        mainWindow.NavigateToList();

                        if (mainWindow.ViewModel is MainViewModel mainVm)
                        {
                            mainVm.SidebarSelectedIndex = 0;
                        }
                    }
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
