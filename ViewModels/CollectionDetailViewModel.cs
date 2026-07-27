using Game_Library.Models;
using Game_Library.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Application = System.Windows.Application;

namespace Game_Library.ViewModels
{
    public class CollectionDetailViewModel : ViewModelBase
    {
        private CollectionModel _currentCollection;
        public CollectionModel CurrentCollection
        {
            get => _currentCollection;
            set => SetProperty(ref _currentCollection, value);
        }

        public ObservableCollection<GameModels> CollectionGames { get; set; } = new ObservableCollection<GameModels>();

        public ICommand EditCollectionCommand { get; }
        public ICommand OpenGameDetailCommand { get; }

        public CollectionDetailViewModel(CollectionModel collection)
        {
            CurrentCollection = collection;

            EditCollectionCommand = new RelayCommand(_ => ExecuteEditCollection());
            OpenGameDetailCommand = new RelayCommand(p => ExecuteOpenGameDetail(p as GameModels));

            LoadGames();
        }

        public void LoadGames()
        {
            CollectionGames.Clear();
            if (CurrentCollection?.GameIds != null)
            {
                var games = GameDataService.Instance.AllGames.Where(g => CurrentCollection.GameIds.Contains(g.Id));
                foreach (var g in games) CollectionGames.Add(g);
            }
        }

        private void ExecuteEditCollection()
        {
            AddEditCollectionWindow win = new AddEditCollectionWindow();
            win.DataContext = new AddEditCollectionViewModel(CurrentCollection);
            win.Owner = Application.Current.MainWindow;

            if (win.ShowDialog() == true)
            {
                OnPropertyChanged(nameof(CurrentCollection));
                LoadGames();
            }
        }

        private void ExecuteOpenGameDetail(GameModels selectedGame)
        {
            if (selectedGame != null)
            {
                MainWindow.Instance.NavigateToDetail(selectedGame);
            }
        }
    }
}
