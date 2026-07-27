using Game_Library.Models;
using Game_Library.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Game_Library.ViewModels
{
    public class GameCheckSelectModel : ViewModelBase
    {
        private bool _isSelected;
        public string Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Thumbnail { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }

    public class AddEditCollectionViewModel : ViewModelBase
    {
        private string _gameSearchKeyword = string.Empty;
        public string GameSearchKeyword
        {
            get => _gameSearchKeyword;
            set
            {
                if (SetProperty(ref _gameSearchKeyword, value))
                    ApplyGameFilter();
            }
        }
        public ObservableCollection<GameCheckSelectModel> AllAvailableGames { get; set; } = new ObservableCollection<GameCheckSelectModel>();
        public ObservableCollection<GameCheckSelectModel> FilteredAvailableGames { get; set; } = new ObservableCollection<GameCheckSelectModel>();
        private readonly CollectionModel _editingCollection;
        private readonly bool _isEditMode;

        private string _windowTitle = "CREATE NEW COLLECTION";
        private string _title = string.Empty;
        private string _thumbnailPath = string.Empty;

        public string WindowTitle { get => _windowTitle; set => SetProperty(ref _windowTitle, value); }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string ThumbnailPath { get => _thumbnailPath; set => SetProperty(ref _thumbnailPath, value); }


        public ICommand SelectThumbnailCommand { get; }
        public ICommand SaveCommand { get; }

        public AddEditCollectionViewModel(CollectionModel collectionToEdit = null)
        {
            _editingCollection = collectionToEdit;
            _isEditMode = collectionToEdit != null;

            SelectThumbnailCommand = new RelayCommand(_ => ExecuteSelectThumbnail());
            SaveCommand = new RelayCommand(async w => await ExecuteSaveAsync(w));

            LoadGames();

            if (_isEditMode)
            {
                WindowTitle = "EDIT COLLECTION";
                Title = _editingCollection.Title;
                ThumbnailPath = _editingCollection.Thumbnail;
            }
        }

        private void LoadGames()
        {
            var allGames = GameDataService.Instance.AllGames;
            AllAvailableGames.Clear();

            foreach (var g in allGames)
            {
                bool selected = _isEditMode && _editingCollection.GameIds != null && _editingCollection.GameIds.Contains(g.Id);
                AllAvailableGames.Add(new GameCheckSelectModel
                {
                    Id = g.Id,
                    Title = g.Title,
                    Type = g.Type,
                    Thumbnail = g.Thumbnail,
                    IsSelected = selected
                });
            }
            ApplyGameFilter();
        }

        private void ApplyGameFilter()
        {
            string kw = (GameSearchKeyword ?? "").Trim().ToLower();
            FilteredAvailableGames.Clear();

            foreach (var item in AllAvailableGames)
            {
                if (string.IsNullOrEmpty(kw) || (item.Title ?? "").ToLower().Contains(kw))
                {
                    FilteredAvailableGames.Add(item);
                }
            }
        }

        private void ExecuteSelectThumbnail()
        {
            var ofd = new OpenFileDialog { Filter = "Image Files (*.jpg;*.jpeg;*.png;*.webp)|*.jpg;*.jpeg;*.png;*.webp" };
            if (ofd.ShowDialog() == true)
            {
                ThumbnailPath = ofd.FileName;
            }
        }

        private async Task ExecuteSaveAsync(object windowParam)
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                MessageBox.Show("Vui lòng nhập tên Collection!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CollectionModel target = _isEditMode ? _editingCollection : new CollectionModel();
            target.Title = Title.Trim();
            target.GameIds = AllAvailableGames.Where(x => x.IsSelected).Select(x => x.Id).ToList();

            if (!string.IsNullOrEmpty(ThumbnailPath) && Path.IsPathRooted(ThumbnailPath))
            {
                string targetFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "collections_data", target.Id);
                Directory.CreateDirectory(targetFolder);

                string destPath = Path.Combine(targetFolder, "cover" + Path.GetExtension(ThumbnailPath));
                File.Copy(ThumbnailPath, destPath, true);
                target.Thumbnail = Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, destPath);
            }

            if (!_isEditMode)
            {
                CollectionDataService.Instance.Collections.Add(target);
            }

            await CollectionDataService.Instance.SaveCollectionsAsync();

            if (windowParam is Window win)
            {
                win.DialogResult = true;
                win.Close();
            }
        }
    }
}
