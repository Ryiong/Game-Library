using Game_Library.Models;
using Game_Library.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Game_Library.ViewModels
{
    public class CollectionsViewModel : ViewModelBase
    {
        private string _searchKeyword = string.Empty;
        public ObservableCollection<CollectionModel> FilteredCollections { get; set; } = new ObservableCollection<CollectionModel>();

        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                if (SetProperty(ref _searchKeyword, value)) ApplyFilter();
            }
        }

        public ICommand CreateCollectionCommand { get; }
        public ICommand OpenDetailCommand { get; }
        public ICommand DeleteCollectionCommand { get; }

        public CollectionsViewModel()
        {
            CreateCollectionCommand = new RelayCommand(_ => ExecuteCreate());
            OpenDetailCommand = new RelayCommand(p => ExecuteOpenDetail(p as CollectionModel));
            DeleteCollectionCommand = new RelayCommand(async p => await ExecuteDeleteAsync(p as CollectionModel));

            if (MainWindow.Instance != null)
            {
                MainWindow.Instance.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(MainWindow.IsNsfwEnabled))
                    {
                        ApplyFilter();
                    }
                };
            }

            LoadData();
        }

        public void LoadData()
        {
            ApplyFilter();
        }

        public async void ApplyFilter()
        {
            var sourceCollections = CollectionDataService.Instance.Collections.ToList();
            var allGames = GameDataService.Instance.AllGames;
            string keyword = (SearchKeyword ?? "").Trim().ToLower();

            var result = await Task.Run(() =>
            {
                return sourceCollections.Where(col =>
                {
                    bool containsNsfwGame = false;
                    if (col.GameIds != null && col.GameIds.Count > 0)
                    {
                        var gamesInCol = allGames.Where(g => col.GameIds.Contains(g.Id));
                        containsNsfwGame = gamesInCol.Any(g => g.isNSFW);
                    }

                    if (!MainWindow.IsNsfwEnabled && containsNsfwGame)
                    {
                        return false;
                    }

                    if (string.IsNullOrEmpty(keyword)) return true;
                    return (col.Title ?? "").ToLower().Contains(keyword);
                }).ToList();
            });

            if (Application.Current != null)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    FilteredCollections.Clear();
                    foreach (var col in result) FilteredCollections.Add(col);
                }, DispatcherPriority.Background);
            }
        }

        private void ExecuteCreate()
        {
            AddEditCollectionWindow win = new AddEditCollectionWindow();
            win.DataContext = new AddEditCollectionViewModel();
            win.Owner = Application.Current.MainWindow;

            if (win.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void ExecuteOpenDetail(CollectionModel selectedCollection)
        {
            if (selectedCollection == null) return;
            if (MainWindow.Instance.ViewModel is MainViewModel mainVM)
            {
                mainVM.NavigateToCollectionDetail(selectedCollection);
            }
        }

        private async Task ExecuteDeleteAsync(CollectionModel collection)
        {
            if (collection == null) return;

            var res = MessageBox.Show($"Bạn có chắc chắn muốn xóa Collection '{collection.Title}'?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                CollectionDataService.Instance.Collections.Remove(collection);
                await CollectionDataService.Instance.SaveCollectionsAsync();
                LoadData();
            }
        }
    }
}
