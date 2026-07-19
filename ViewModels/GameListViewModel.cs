using Game_Library.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;

namespace Game_Library.ViewModels
{
    public abstract class GameListViewModel : ViewModelBase
    {
        private ObservableCollection<GameModels> _filteredGames;
        private string _searchKeyword = string.Empty;

        public ObservableCollection<GameModels> FilteredGames
        {
            get => _filteredGames;
            set => SetProperty(ref _filteredGames, value);
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

        public ICommand OpenDetailCommand { get; }

        protected GameListViewModel()
        {
            FilteredGames = new ObservableCollection<GameModels>();
            OpenDetailCommand = new RelayCommand(p => ExecuteOpenDetail(p));

            MainWindow.Instance.PropertyChanged += OnMainWindowPropertyChanged;

            ApplyFilter();
        }

        private void OnMainWindowPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainWindow.IsNsfwEnabled))
            {
                ApplyFilter();
            }
        }

        protected abstract IEnumerable<GameModels> GetSourceGames();

        public void ApplyFilter()
        {
            var source = GetSourceGames() ?? new List<GameModels>();

            string keyword = SearchKeyword.Trim().ToLower();

            var result = source.Where(g =>
                (string.IsNullOrEmpty(keyword) || g.Title.ToLower().Contains(keyword)) &&
                (MainWindow.IsNsfwEnabled || !g.isNSFW)
            ).ToList();

            FilteredGames = new ObservableCollection<GameModels>(result);
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
        }
    }
}
