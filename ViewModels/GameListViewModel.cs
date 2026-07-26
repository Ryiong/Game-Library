using Game_Library.Models;
using Game_Library.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

            MainWindow.Instance.PropertyChanged += OnMainWindowPropertyChanged;
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

        private void OnMainWindowPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainWindow.IsNsfwEnabled))
            {
                ApplyFilter();
            }
        }

        protected abstract IEnumerable<GameModels> GetSourceGames();

        public async void ApplyFilter()
        {
            var source = GetSourceGames()?.ToList() ?? new List<GameModels>();
            string keyword = SearchKeyword.Trim().ToLower();

            var filteredResult = await Task.Run(() =>
            {
                return source.Where(g =>
                    (string.IsNullOrEmpty(keyword) || (g.Title != null && g.Title.ToLower().Contains(keyword))) &&
                    (MainWindow.IsNsfwEnabled || !g.isNSFW)
                ).ToList();
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

        private void ExecuteOpenDetail(object parameter)
        {
            if (parameter is GameModels selectedGame)
            {
                MainWindow.Instance.NavigateToDetail(selectedGame);
            }
        }

        public void Dispose()
        {
            if (MainWindow.Instance != null)
            {
                MainWindow.Instance.PropertyChanged -= OnMainWindowPropertyChanged;
            }
            GameDataService.Instance.OnLogChanged -= Instance_OnLogChanged;
        }
    }
}
