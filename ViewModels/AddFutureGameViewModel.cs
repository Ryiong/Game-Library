using Game_Library.Models;
using Game_Library.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Game_Library.ViewModels
{
    public class AddFutureGameViewModel : ViewModelBase
    {
        private string _windowTitle = "ADD FUTURE GAME";
        private string _title = string.Empty;
        private string _selectedType = "HTML5";
        private string _description = string.Empty;
        private DateTime? _releaseDate = DateTime.Now;
        private string _selectedThumbnailPath;
        private BitmapImage _thumbnailSource;
        private string _saveButtonContent = "Save";

        public string WindowTitle { get => _windowTitle; set => SetProperty(ref _windowTitle, value); }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string SelectedType { get => _selectedType; set => SetProperty(ref _selectedType, value); }
        public string Description { get => _description; set => SetProperty(ref _description, value);  }
        public DateTime? ReleaseDate { get => _releaseDate; set => SetProperty(ref _releaseDate, value); }
        public string SelectedThumbnailPath { get => _selectedThumbnailPath; set => SetProperty(ref _selectedThumbnailPath, value); }
        public BitmapImage ThumbnailSource { get => _thumbnailSource; set => SetProperty(ref _thumbnailSource, value); }
        public string SaveButtonContent { get => _saveButtonContent; set => SetProperty(ref _saveButtonContent, value); }

        public ObservableCollection<GameCheckItem> RelatedGames { get; set; } = new ObservableCollection<GameCheckItem>();
        public ICommand SelectThumbnailCommand { get; }
        public ICommand RemoveThumbnailCommand { get; }
        public ICommand SaveCommand { get; }

        public AddFutureGameViewModel()
        {
            SelectThumbnailCommand = new RelayCommand(_ => ExecuteSelectThumbnail());
            RemoveThumbnailCommand = new RelayCommand(_ => ExecuteRemoveThumbnail());
            SaveCommand = new RelayCommand(async param => await ExecuteSaveAsync(param as Window));

            LoadRelatedGamesData();
        }

        private void LoadRelatedGamesData()
        {
            RelatedGames.Clear();
            foreach (var g in GameDataService.Instance.AllGames)
            {
                RelatedGames.Add(new GameCheckItem { Id = g.Id, Title = g.Title, IsSelected = false });
            }
        }

        private void ExecuteSelectThumbnail()
        {
            var ofd = new OpenFileDialog { Filter = "Image Files (*.jpg;*.jpeg;*.png;*.webp)|*.jpg;*.jpeg;*.png;*.webp" };
            if (ofd.ShowDialog() == true)
            {
                SelectedThumbnailPath = ofd.FileName;
                ThumbnailSource = LoadImageUnloaded(ofd.FileName);
            }
        }

        private void ExecuteRemoveThumbnail()
        {
            SelectedThumbnailPath = null;
            ThumbnailSource = null;
        }

        private BitmapImage LoadImageUnloaded(string path)
        {
            if (!File.Exists(path)) return null;
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
        }

        private async Task ExecuteSaveAsync(Window window)
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                MessageBox.Show("Vui lòng nhập tên trò chơi tương lai!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveButtonContent = "Processing...";
            string futureId = Guid.NewGuid().ToString();
            string relativeThumbPath = "";

            if (!string.IsNullOrEmpty(SelectedThumbnailPath) && File.Exists(SelectedThumbnailPath))
            {
                string futureFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "future_games_data", futureId);
                if (!Directory.Exists(futureFolder)) Directory.CreateDirectory(futureFolder);

                string ext = Path.GetExtension(SelectedThumbnailPath);
                string targetThumbPath = Path.Combine(futureFolder, $"thumbnail{ext}");

                File.Copy(SelectedThumbnailPath, targetThumbPath, true);
                
                relativeThumbPath = Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, targetThumbPath);
            }

            var newFutureGame = new FutureGameModel
            {
                Id = Guid.NewGuid().ToString(),
                Title = Title.Trim(),
                Type = SelectedType,
                Description = Description,
                ReleaseDate = ReleaseDate?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd"),
                Thumbnail = relativeThumbPath,
                RelatedGameIds = RelatedGames.Where(x => x.IsSelected).Select(x => x.Id).ToList(),
                IsActivated = false
            };

            FutureGameDataService.Instance.FutureGames.Add(newFutureGame);
            bool saved = await FutureGameDataService.Instance.SaveFutureGamesAsync();

            if (saved && window != null)
            {
                window.DialogResult = true;
                window.Close();
            }
            else
            {
                SaveButtonContent = "Save";
            }
        }
    }
}
