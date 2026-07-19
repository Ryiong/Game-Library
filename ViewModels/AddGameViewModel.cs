using Game_Library.Extensions;
using Game_Library.Models;
using Game_Library.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Game_Library.ViewModels
{
    public class AddGameViewModel : ViewModelBase
    {
        private readonly string centralStoragePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "games_data");
        private readonly bool _isEditMode;
        private readonly GameModels _editingGame;

        private string _windowTitle = "ADD NEW GAME";
        private string _title = string.Empty;
        private string _description = string.Empty;
        private string _selectedType;
        private bool _isNsfw;
        private DateTime? _addedDate = DateTime.Now;
        private DateTime? _releaseDate = DateTime.Now;

        private string _selectedThumbnailPath;
        private object _thumbnailSource;
        private string _previewFolderName = "-- Chưa chọn nguồn chạy game --";
        private string _saveButtonContent = "Save";

        public ObservableCollection<string> SelectedInGameImages { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<GameCheckItem> RelatedGames { get; set; } = new ObservableCollection<GameCheckItem>();
        public ObservableCollection<TreeViewItem> FolderTreeItems { get; set; } = new ObservableCollection<TreeViewItem>();

        public string SelectedGameFolderPath { get; set; }
        public string SelectedSingleFilePath { get; set; }

        #region Properties Binding
        public string WindowTitle { get => _windowTitle; set => SetProperty(ref _windowTitle, value); }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string Description { get => _description; set => SetProperty(ref _description, value); }
        public bool IsNsfw { get => _isNsfw; set => SetProperty(ref _isNsfw, value); }
        public DateTime? AddedDate { get => _addedDate; set => SetProperty(ref _addedDate, value); }
        public DateTime? ReleaseDate { get => _releaseDate; set => SetProperty(ref _releaseDate, value); }
        public string SelectedThumbnailPath { get => _selectedThumbnailPath; set => SetProperty(ref _selectedThumbnailPath, value); }
        public object ThumbnailSource { get => _thumbnailSource; set => SetProperty(ref _thumbnailSource, value); }
        public string PreviewFolderName { get => _previewFolderName; set => SetProperty(ref _previewFolderName, value); }
        public string SaveButtonContent { get => _saveButtonContent; set => SetProperty(ref _saveButtonContent, value); }

        public string SelectedType
        {
            get => _selectedType;
            set => SetProperty(ref _selectedType, value);
        }
        #endregion

        #region Commands
        public ICommand SelectThumbnailCommand { get; }
        public ICommand SelectInGameImagesCommand { get; }
        public ICommand SelectFolderCommand { get; }
        public ICommand SelectSingleFileCommand { get; }
        public ICommand SaveCommand { get; }
        #endregion

        public AddGameViewModel(GameModels gameToEdit = null)
        {
            _editingGame = gameToEdit;
            _isEditMode = gameToEdit != null;

            // Khởi tạo Commands
            SelectThumbnailCommand = new RelayCommand(_ => ExecuteSelectThumbnail());
            SelectInGameImagesCommand = new RelayCommand(_ => ExecuteSelectInGameImages());
            SelectFolderCommand = new RelayCommand(_ => ExecuteSelectFolder());
            SelectSingleFileCommand = new RelayCommand(_ => ExecuteSelectSingleFile());
            SaveCommand = new RelayCommand(async w => await ExecuteSaveAsync(w));

            LoadRelatedGamesData();

            if (_isEditMode)
            {
                WindowTitle = "EDIT TRÒ CHƠI";
                PopulateFieldsForEditing();
            }
        }

        private void LoadRelatedGamesData()
        {
            var sourceGames = GameDataService.Instance.AllGames;
            var filtered = _isEditMode ? sourceGames.Where(g => g.Id != _editingGame.Id) : sourceGames;

            var items = filtered.Select(g => new GameCheckItem
            {
                Id = g.Id,
                Title = g.Title,
                IsSelected = _isEditMode && _editingGame.RelatedGameIds != null && _editingGame.RelatedGameIds.Contains(g.Id)
            }).ToList();

            RelatedGames = new ObservableCollection<GameCheckItem>(items);
        }

        private void PopulateFieldsForEditing()
        {
            Title = _editingGame.Title;
            Description = _editingGame.Description;
            IsNsfw = _editingGame.isNSFW;
            SelectedType = _editingGame.Type;

            if (DateTime.TryParse(_editingGame.AddedDate, out DateTime aDate)) AddedDate = aDate;
            if (DateTime.TryParse(_editingGame.ReleaseDate, out DateTime rDate)) ReleaseDate = rDate;

            if (!string.IsNullOrEmpty(_editingGame.Thumbnail))
            {
                string thumbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _editingGame.Thumbnail);
                if (File.Exists(thumbPath))
                {
                    SelectedThumbnailPath = thumbPath;
                    ThumbnailSource = LoadImageUnloaded(thumbPath);
                }
            }

            if (_editingGame.ImageInGame != null)
            {
                foreach (var img in _editingGame.ImageInGame)
                {
                    string fullImgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, img);
                    if (File.Exists(fullImgPath)) SelectedInGameImages.Add(fullImgPath);
                }
            }

            string gameFolder = Path.Combine(centralStoragePath, _editingGame.Id);
            if (Directory.Exists(gameFolder))
            {
                SelectedGameFolderPath = gameFolder;
                PreviewFolderName = Path.GetFileName(gameFolder);
                BuildFolderTree(gameFolder);
            }
        }

        private System.Windows.Media.Imaging.BitmapImage LoadImageUnloaded(string path)
        {
            var bitmap = new System.Windows.Media.Imaging.BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path);
            bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        #region Execution Logic
        private void ExecuteSelectThumbnail()
        {
            var ofd = new OpenFileDialog { Filter = "Image Files (*.jpg;*.jpeg;*.png;*.webp)|*.jpg;*.jpeg;*.png;*.webp" };
            if (ofd.ShowDialog() == true)
            {
                SelectedThumbnailPath = ofd.FileName;
                ThumbnailSource = LoadImageUnloaded(ofd.FileName);
            }
        }

        private void ExecuteSelectInGameImages()
        {
            if (SelectedInGameImages.Count >= 6)
            {
                MessageBox.Show("Chỉ được phép chọn tối đa 6 hình ảnh in-game!", "Giới hạn tệp tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var ofd = new OpenFileDialog { Filter = "Image Files (*.jpg;*.jpeg;*.png;*.webp;*.gif)|*.jpg;*.jpeg;*.png;*.webp;*.gif", Multiselect = true };
            if (ofd.ShowDialog() == true)
            {
                foreach (string file in ofd.FileNames)
                {
                    if (SelectedInGameImages.Count < 6 && !SelectedInGameImages.Contains(file))
                    {
                        SelectedInGameImages.Add(file);
                    }
                }
            }
        }

        private void ExecuteSelectFolder()
        {
            SelectedSingleFilePath = null;
            var dialog = new OpenFolderDialog { Title = "Chọn thư mục chứa source game" };
            if (dialog.ShowDialog() == true)
            {
                SelectedGameFolderPath = dialog.FolderName;
                PreviewFolderName = Path.GetFileName(SelectedGameFolderPath);
                BuildFolderTree(SelectedGameFolderPath);
            }
        }

        private void ExecuteSelectSingleFile()
        {
            SelectedGameFolderPath = null;
            var ofd = new OpenFileDialog { Filter = "Game Files (*.swf;*.html;*.htm;*.exe)|*.swf;*.html;*.htm;*.exe" };
            if (ofd.ShowDialog() == true)
            {
                SelectedSingleFilePath = ofd.FileName;
                PreviewFolderName = Path.GetFileName(SelectedSingleFilePath);
                FolderTreeItems.Clear();
                FolderTreeItems.Add(new TreeViewItem { Header = Path.GetFileName(SelectedSingleFilePath), IsSelected = true });
            }
        }

        private async Task ExecuteSaveAsync(object windowParam)
        {
            // VALIDATION ENGINE
            if (string.IsNullOrWhiteSpace(Title))
            {
                MessageBox.Show("Vui lòng nhập tên trò chơi!", "Thẩm định dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrEmpty(SelectedGameFolderPath) && string.IsNullOrEmpty(SelectedSingleFilePath))
            {
                MessageBox.Show("Vui lòng cấu hình tệp tin nguồn hoặc thư mục trò chơi!", "Thẩm định dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrEmpty(SelectedType))
            {
                MessageBox.Show("Vui lòng chọn Phân loại trò chơi!", "Thẩm định dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveButtonContent = "Processing...";

            string gameId = _isEditMode ? _editingGame.Id : Guid.NewGuid().ToString();
            string targetGameFolder = Path.Combine(centralStoragePath, gameId);

            bool isSuccess = await Task.Run(async () =>
            {
                try
                {
                    if (!Directory.Exists(targetGameFolder)) Directory.CreateDirectory(targetGameFolder);

                    string mainFileRelativePath = "";

                    if (!string.IsNullOrEmpty(SelectedSingleFilePath))
                    {
                        string fileName = Path.GetFileName(SelectedSingleFilePath);
                        string targetFilePath = Path.Combine(targetGameFolder, fileName);
                        File.Copy(SelectedSingleFilePath, targetFilePath, true);
                        mainFileRelativePath = fileName;
                    }
                    else if (!string.IsNullOrEmpty(SelectedGameFolderPath) && SelectedGameFolderPath != targetGameFolder)
                    {
                        CopyDirectory(SelectedGameFolderPath, targetGameFolder);
                    }

                    string finalThumbnailRelativePath = _editingGame?.Thumbnail ?? "";
                    if (!string.IsNullOrEmpty(SelectedThumbnailPath) && SelectedThumbnailPath != Path.Combine(AppDomain.CurrentDomain.BaseDirectory, finalThumbnailRelativePath))
                    {
                        string targetThumbPath = Path.Combine(targetGameFolder, "thumbnail" + Path.GetExtension(SelectedThumbnailPath));
                        ImageOptimizer.OptimizeAndSave(SelectedThumbnailPath, targetThumbPath);
                        finalThumbnailRelativePath = Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, targetThumbPath);
                    }

                    List<string> finalInGameRelativePaths = new List<string>();
                    int imgIndex = 1;
                    foreach (var srcImgPath in SelectedInGameImages)
                    {
                        if (srcImgPath.Contains(targetGameFolder))
                        {
                            finalInGameRelativePaths.Add(Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, srcImgPath));
                        }
                        else
                        {
                            string targetImgPath = Path.Combine(targetGameFolder, $"ingame_{imgIndex++}{Path.GetExtension(srcImgPath)}");
                            ImageOptimizer.OptimizeAndSave(srcImgPath, targetImgPath);
                            finalInGameRelativePaths.Add(Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, targetImgPath));
                        }
                    }

                    GameModels targetGame = _isEditMode ? _editingGame : new GameModels();
                    targetGame.Id = gameId;
                    targetGame.Title = Title.Trim();
                    targetGame.Type = SelectedType;
                    targetGame.isNSFW = IsNsfw;
                    targetGame.Description = Description;
                    targetGame.AddedDate = AddedDate?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd");
                    targetGame.ReleaseDate = ReleaseDate?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd");
                    targetGame.Thumbnail = finalThumbnailRelativePath;
                    targetGame.ImageInGame = finalInGameRelativePaths;
                    targetGame.RelatedGameIds = RelatedGames.Where(x => x.IsSelected).Select(x => x.Id).ToList();
                    targetGame.FolderName = !string.IsNullOrEmpty(SelectedSingleFilePath) ? gameId : Path.GetFileName(SelectedGameFolderPath);

                    if (!string.IsNullOrEmpty(mainFileRelativePath))
                    {
                        targetGame.MainFile = mainFileRelativePath;
                    }
                    else if (string.IsNullOrEmpty(targetGame.MainFile))
                    {
                        var files = Directory.GetFiles(targetGameFolder, "*.*", SearchOption.AllDirectories)
                            .Where(f => f.EndsWith(".exe") || f.EndsWith(".html") || f.EndsWith(".swf") || f.EndsWith(".index.html")).ToList();
                        if (files.Count > 0) targetGame.MainFile = Path.GetRelativePath(targetGameFolder, files[0]);
                    }

                    return await GameDataService.Instance.SaveGameAsync(targetGame, _isEditMode);
                }
                catch (Exception)
                {
                    return false;
                }
            });

            if (isSuccess)
            {
                MessageBox.Show("Lưu trữ thông tin cấu hình trò chơi thành công!", "Hoàn tất", MessageBoxButton.OK, MessageBoxImage.Information);
                if (windowParam is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            else
            {
                MessageBox.Show("Hệ thống nén ảnh hoặc ghi tệp tin gặp lỗi bất thường.", "Thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
                SaveButtonContent = "Save";
            }
        }

        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);
            foreach (string file in Directory.GetFiles(sourceDir))
                File.Copy(file, Path.Combine(destinationDir, Path.GetFileName(file)), true);
            foreach (string subDir in Directory.GetDirectories(sourceDir))
                CopyDirectory(subDir, Path.Combine(destinationDir, Path.GetFileName(subDir)));
        }

        private void BuildFolderTree(string rootPath)
        {
            FolderTreeItems.Clear();
            var rootDir = new DirectoryInfo(rootPath);
            FolderTreeItems.Add(CreateTreeItem(rootDir));
        }

        private TreeViewItem CreateTreeItem(DirectoryInfo directoryInfo)
        {
            var item = new TreeViewItem { Header = directoryInfo.Name, IsExpanded = true };
            try
            {
                foreach (var directory in directoryInfo.GetDirectories()) item.Items.Add(CreateTreeItem(directory));
                foreach (var file in directoryInfo.GetFiles()) item.Items.Add(new TreeViewItem { Header = file.Name });
            }
            catch { }
            return item;
        }
        #endregion
    }
}
