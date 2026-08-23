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
using System.Windows.Media.Imaging;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Game_Library.ViewModels
{
    public class AddGameViewModel : ViewModelBase
    {
        private readonly string centralStoragePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "games_data");
        private string _title = string.Empty;
        private string _selectedType;
        private string _selectedThumbnailPath;
        private object _thumbnailSource;
        private string _previewFolderName = "-- Chưa chọn nguồn chạy game --";
        private string _saveButtonContent = "Save";
        public ObservableCollection<TreeViewItem> FolderTreeItems { get; set; } = new ObservableCollection<TreeViewItem>();
        public string SelectedGameFolderPath { get; set; }
        public string SelectedSingleFilePath { get; set; }

        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string SelectedThumbnailPath { get => _selectedThumbnailPath; set => SetProperty(ref _selectedThumbnailPath, value); }
        public object ThumbnailSource { get => _thumbnailSource; set => SetProperty(ref _thumbnailSource, value); }
        public string PreviewFolderName { get => _previewFolderName; set => SetProperty(ref _previewFolderName, value); }
        public string SaveButtonContent { get => _saveButtonContent; set => SetProperty(ref _saveButtonContent, value); }

        public string SelectedType
        {
            get => _selectedType;
            set => SetProperty(ref _selectedType, value);
        }

        public ICommand SelectThumbnailCommand { get; }
        public ICommand RemoveThumbnailCommand { get; }
        public ICommand SelectFolderCommand { get; }
        public ICommand SelectSingleFileCommand { get; }
        public ICommand SaveCommand { get; }


        public AddGameViewModel()
        {
            SelectThumbnailCommand = new RelayCommand(_ => ExecuteSelectThumbnail());
            RemoveThumbnailCommand = new RelayCommand(_ => ExecuteRemoveThumbnail());
            SelectFolderCommand = new RelayCommand(_ => ExecuteSelectFolder());
            SelectSingleFileCommand = new RelayCommand(_ => ExecuteSelectSingleFile());
            SaveCommand = new RelayCommand(async w => await ExecuteSaveAsync(w));

        }

        private System.Windows.Media.Imaging.BitmapImage LoadImageUnloaded(string path)
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

        private void ExecuteSelectThumbnail()
        {
            var ofd = new OpenFileDialog { Filter = "Image Files (*.jpg;*.jpeg;*.png;*.webp)|*.jpg;*.jpeg;*.png;*.webp" };
            if (ofd.ShowDialog() == true)
            {
                SelectedThumbnailPath = ofd.FileName;
                ThumbnailSource = LoadImageUnloaded(ofd.FileName);
                OnPropertyChanged(nameof(ThumbnailSource));
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
            var ofd = new OpenFileDialog { Filter = "Game Files (*.swf;*.html;*.htm)|*.swf;*.html;*.htm" };
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

            string gameId = Guid.NewGuid().ToString();
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

                    string finalThumbnailRelativePath = "";
                    if (!string.IsNullOrEmpty(SelectedThumbnailPath))
                    {
                        string targetThumbPath = Path.Combine(targetGameFolder, "thumbnail" + Path.GetExtension(SelectedThumbnailPath));
                        if (SelectedThumbnailPath != targetThumbPath)
                        {
                            try
                            {
                                ImageOptimizer.OptimizeAndSave(SelectedThumbnailPath, targetThumbPath);
                            }
                            catch
                            {
                                File.Copy(SelectedThumbnailPath, targetThumbPath, true);
                            }
                        }
                        finalThumbnailRelativePath = Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, targetThumbPath);
                    }

                    GameModels targetGame = new GameModels();
                    targetGame.Id = gameId;
                    targetGame.Title = Title.Trim();
                    targetGame.Type = SelectedType;
                    targetGame.Thumbnail = finalThumbnailRelativePath;
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

                    return await GameDataService.Instance.SaveGameAsync(targetGame);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Bug: {ex.Message}", "Thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            });

            if (isSuccess)
            {
                GameDataService.Instance.Log($"[AUTO RELOAD] Đã cập nhật thành công game: {Title}");
                MessageBox.Show("Lưu trữ thông tin cấu hình trò chơi thành công!", "Hoàn tất", MessageBoxButton.OK, MessageBoxImage.Information);
                if (windowParam is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            else
            {
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

        private void ExecuteRemoveThumbnail()
        {
            SelectedThumbnailPath = null;
            ThumbnailSource = null;
            GameDataService.Instance.Log("Đã xóa ảnh bìa (Thumbnail).");
        }

        public void LoadFromDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath)) return;

            SelectedGameFolderPath = directoryPath;
            SelectedSingleFilePath = null;

            Title = Path.GetFileName(directoryPath);
            PreviewFolderName = Title;
            BuildFolderTree(directoryPath);

            var hasSwf = Directory.GetFiles(directoryPath, "*.swf", SearchOption.AllDirectories).Any();
            var hasHtml = Directory.GetFiles(directoryPath, "*.html", SearchOption.AllDirectories).Any() ||
                          Directory.GetFiles(directoryPath, "*.htm", SearchOption.AllDirectories).Any();

            if (hasSwf)
            {
                SelectedType = "FLASH";
            }
            else if (hasHtml)
            {
                SelectedType = "HTML5";
            }
        }
    }
}
