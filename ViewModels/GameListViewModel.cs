using Game_Library.Models;
using Game_Library.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows.Input;
using System.Windows.Threading;
using Application = System.Windows.Application;

namespace Game_Library.ViewModels
{
    public abstract class GameListViewModel : ViewModelBase, IDisposable
    {
        private string _searchKeyword = string.Empty;
        public ObservableCollection<GameModels> FilteredGames { get; } = new ObservableCollection<GameModels>();

        public ICommand OpenDetailCommand { get; }

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

        protected GameListViewModel()
        {
            OpenDetailCommand = new RelayCommand(p => ExecuteOpenDetail(p));

            GameDataService.Instance.OnLogChanged += Instance_OnLogChanged;

            ApplyFilter();
        }

        private void Instance_OnLogChanged(string logMessage)
        {
            if (logMessage.Contains("[AUTO RELOAD]") || logMessage.Contains("Đã tải lại") || logMessage.Contains("[EDIT/SAVE]"))
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ApplyFilter();
                });
            }
        }


        protected abstract IEnumerable<GameModels> GetSourceGames();

        public async void ApplyFilter()
        {
            var source = GetSourceGames()?.ToList() ?? new List<GameModels>();
            string rawKeyword = SearchKeyword ?? string.Empty;

            var filteredResult = await Task.Run(() =>
            {
                string normalizedKeyword = RemoveDiacritics(rawKeyword.Trim().ToLower());
                string[] tokens = normalizedKeyword.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                return source.Where(game =>
                {
                    

                    if (tokens.Length == 0) return true;

                    string titleNorm = RemoveDiacritics(game.Title ?? "").ToLower();
                    string typeNorm = RemoveDiacritics(game.Type ?? "").ToLower();

                    string fullSearchableText = $"{titleNorm} {typeNorm}";

                    return tokens.All(token => fullSearchableText.Contains(token));
                }).ToList();
            });

            if (Application.Current != null)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    FilteredGames.Clear();
                    foreach (var game in filteredResult)
                    {
                        FilteredGames.Add(game);
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

        private void ExecuteOpenDetail(object parameter)
        {
            if (parameter is GameModels selectedGame)
            {
                MainWindow.Instance.NavigateToPlay(selectedGame);
            }
        }

        public void Dispose()
        {
           
            GameDataService.Instance.OnLogChanged -= Instance_OnLogChanged;
        }
    }
}
