using Game_Library.Models;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using Image = System.Windows.Controls.Image;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;


namespace Game_Library
{
    /// <summary>
    /// Interaction logic for AddGameWindow.xaml
    /// </summary>
    public partial class AddGameWindow : Window
    {
        private readonly string centralStoragePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "games_data");
        private readonly string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "game.json");

        private string _selectedThumbnailPath = null;
        private List<string> _selectedInGameImages = new List<string>();

        private string _selectedGameFolderPath = null;
        private string _selectedSingleFilePath = null;

        private List<GameModels> _allGames = new List<GameModels>();

        private readonly bool _isEditMode = false;
        private readonly GameModels _editingGame = null;

        public AddGameWindow(GameModels gameToEdit = null)
        {
            InitializeComponent();
            LoadRelatedGamesComboBox();
            if (gameToEdit != null)
            {
                _isEditMode = true;
                _editingGame = gameToEdit;
                txtWindowTitle.Text = "EDIT GAME";
                PopulateFieldsForEditing();
            }
            else
            {
                _isEditMode = false;
                dpAddedDate.SelectedDate = DateTime.Now;
                dpReleaseDate.SelectedDate = DateTime.Now;
            }

        }

        #region Initialization & UI Binding
        private void LoadRelatedGamesComboBox()
        {
            try
            {
                if (File.Exists(jsonPath))
                {
                    _allGames = JsonSerializer.Deserialize<List<GameModels>>(File.ReadAllText(jsonPath)) ?? new List<GameModels>();
                    var comboDataSource = _isEditMode
                        ? _allGames.Where(g => g.Id != _editingGame.Id).ToList()
                        : _allGames;

                    cbRelatedGame.ItemsSource = comboDataSource;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách game liên quan: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopulateFieldsForEditing()
        {
            txtTitle.Text = _editingGame.Title;
            txtDescription.Text = _editingGame.Description;
            if (DateTime.TryParse(_editingGame.AddedDate, out DateTime addedDate))
            {
                dpAddedDate.SelectedDate = addedDate;
            } else
            {
                dpAddedDate.SelectedDate = null;
            }

            foreach (ComboBoxItem item in cbType.Items)
            {
                if (item.Content.ToString().Equals(_editingGame.Type, StringComparison.OrdinalIgnoreCase))
                {
                    cbType.SelectedItem = item;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(_editingGame.SeriesId))
            {
                cbRelatedGame.SelectedValue = _editingGame.SeriesId;
            }

            if (!string.IsNullOrEmpty(_editingGame.Thumbnail))
            {
                _selectedThumbnailPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _editingGame.Thumbnail);
                if (File.Exists(_selectedThumbnailPath))
                {
                    imgPreviewThumbnail.Source = new BitmapImage(new Uri(_selectedThumbnailPath));
                }
            }

            if (_editingGame.ImageInGame != null)
            {
                foreach (var img in _editingGame.ImageInGame)
                {
                    string fullImgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, img);
                    if (File.Exists(fullImgPath))
                    {
                        _selectedInGameImages.Add(fullImgPath);
                    }
                }
                UpdateInGameImagesUI();
            }

            tgNsfw.IsChecked = _editingGame.isNSFW;

            string gameFolder = Path.Combine(centralStoragePath, _editingGame.Id);
            if (Directory.Exists(gameFolder))
            {
                _selectedGameFolderPath = gameFolder;
                txtPreviewFolderName.Text = Path.GetFileName(gameFolder);
                BuildFolderTree(gameFolder);
            }
        }
        #endregion

        #region Event Handlers (Select Image / Folder / Single File)
        private void SelectThumbnail_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Image Files (*.jpg;*.jpeg;*.png;*.webp)|*.jpg;*.jpeg;*.png;*.webp",
                Title = "Chọn ảnh bìa Game"
            };

            if (ofd.ShowDialog() == true)
            {
                _selectedThumbnailPath = ofd.FileName;
                imgPreviewThumbnail.Source = new BitmapImage(new Uri(_selectedThumbnailPath));
            }
        }

        private void SelectImageInGame_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedInGameImages.Count >= 6)
            {
                MessageBox.Show("Bạn chỉ được chọn tối đa 6 hình ảnh in-game!", "Giới hạn", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Image Files (*.jpg;*.jpeg;*.png;*.webp;*.gif)|*.jpg;*.jpeg;*.png;*.webp;*.gif",
                Multiselect = true,
                Title = "Chọn ảnh chụp trong Game (Tối đa 6)"
            };

            if (ofd.ShowDialog() == true)
            {
                foreach (string file in ofd.FileNames)
                {
                    if (_selectedInGameImages.Count < 6)
                    {
                        if (!_selectedInGameImages.Contains(file))
                        {
                            _selectedInGameImages.Add(file);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                UpdateInGameImagesUI();
            }
        }

        private void UpdateInGameImagesUI()
        {
            wpPreviewIngame.Children.Clear();
            foreach (var imgPath in _selectedInGameImages)
            {
                try
                {
                    Border border = new Border
                    {
                        Width = 60,
                        Height = 45,
                        Margin = new Thickness(0, 0, 8, 8),
                        CornerRadius = new CornerRadius(4),
                        ClipToBounds = true
                    };

                    if (Path.GetExtension(imgPath).Equals(".gif", StringComparison.OrdinalIgnoreCase))
                    {
                        MediaElement gifPlayer = new MediaElement
                        {
                            Source = new Uri(imgPath),
                            LoadedBehavior = MediaState.Play,
                            UnloadedBehavior = MediaState.Manual,
                            IsMuted = true,
                            Stretch = System.Windows.Media.Stretch.Fill
                        };

                        gifPlayer.MediaEnded += (s, e) => { gifPlayer.Position = TimeSpan.Zero; gifPlayer.Play(); };
                        border.Child = gifPlayer;
                    }
                    else
                    {
                        Image img = new Image
                        {
                            Source = new BitmapImage(new Uri(imgPath)),
                            Stretch = System.Windows.Media.Stretch.UniformToFill
                        };
                        border.Child = img;
                    }
                    
                    wpPreviewIngame.Children.Add(border);
                    
                }
                catch { /* Bỏ qua ảnh lỗi */ }
            }
        }

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            _selectedSingleFilePath = null;
            var dialog = new OpenFolderDialog
            {
                Title = "Chọn thư mục chứa source game"
            };

            if (dialog.ShowDialog() == true)
            {
                _selectedGameFolderPath = dialog.FolderName;
                txtPreviewFolderName.Text = Path.GetFileName(_selectedGameFolderPath);
                BuildFolderTree(_selectedGameFolderPath);
            }
        }

        private void SelectSingleFile_Click(object sender, RoutedEventArgs e)
        {
            _selectedGameFolderPath = null;

            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Game Files (*.swf;*.html;*.htm;*.exe)|*.swf;*.html;*.htm;*.exe",
                Title = "Chọn tệp game"
            };

            if (ofd.ShowDialog() == true)
            {
                _selectedSingleFilePath = ofd.FileName;
                txtPreviewFolderName.Text = Path.GetFileName(_selectedSingleFilePath);

                trvFolderStructure.Items.Clear();
                trvFolderStructure.Items.Add(new TreeViewItem { Header = Path.GetFileName(_selectedSingleFilePath), IsSelected = true });
            }
        }

        #endregion

        #region TreeView Folder Structure Builder
        private void BuildFolderTree(string rootPath)
        {
            trvFolderStructure.Items.Clear();
            DirectoryInfo rootDir = new DirectoryInfo(rootPath);
            TreeViewItem rootItem = CreateTreeItem(rootDir);
            trvFolderStructure.Items.Add(rootItem);
        }

        private TreeViewItem CreateTreeItem(DirectoryInfo directoryInfo)
        {
            TreeViewItem item = new TreeViewItem
            {
                Header = directoryInfo.Name,
                IsExpanded = true
            };

            try
            {
                foreach (var directory in directoryInfo.GetDirectories())
                {
                    item.Items.Add(CreateTreeItem(directory));
                }
                foreach (var file in directoryInfo.GetFiles())
                {
                    item.Items.Add(new TreeViewItem { Header = file.Name });
                }
            }
            catch { /* Bỏ qua các thư mục bị chặn quyền truy cập */ }
            return item;
        }
        #endregion

        #region Save Logic (Add / Edit Action)
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Vui lòng nhập Tên Game!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(_selectedGameFolderPath) && string.IsNullOrEmpty(_selectedSingleFilePath))
            {
                MessageBox.Show("Vui lòng chọn Thư mục hoặc File game!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string gameId = _isEditMode ? _editingGame.Id : Guid.NewGuid().ToString();
                string targetGameFolder = Path.Combine(centralStoragePath, gameId);

                if (!Directory.Exists(targetGameFolder))
                {
                    Directory.CreateDirectory(targetGameFolder);
                }

                string mainFileRelativePath = "";

                if (!string.IsNullOrEmpty(_selectedSingleFilePath))
                {
                    string fileName = Path.GetFileName(_selectedSingleFilePath);
                    string targetFilePath = Path.Combine(targetGameFolder, fileName);

                    File.Copy(_selectedSingleFilePath, targetFilePath, true);

                    mainFileRelativePath = fileName;
                }
                else if (!string.IsNullOrEmpty(_selectedGameFolderPath))
                {
                    if (_selectedGameFolderPath != targetGameFolder)
                    {
                        CopyDirectory(_selectedGameFolderPath, targetGameFolder);
                    }
                }

                string finalThumbnailRelativePath = _editingGame?.Thumbnail ?? "";
                if (!string.IsNullOrEmpty(_selectedThumbnailPath) && _selectedThumbnailPath != Path.Combine(AppDomain.CurrentDomain.BaseDirectory, finalThumbnailRelativePath))
                {
                    string thumbExt = Path.GetExtension(_selectedThumbnailPath);
                    string thumbName = "thumbnail" + thumbExt;
                    string targetThumbPath = Path.Combine(targetGameFolder, thumbName);

                    File.Copy(_selectedThumbnailPath, targetThumbPath, true);
                    finalThumbnailRelativePath = Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, targetThumbPath);
                }

                List<string> finalInGameRelativePaths = new List<string>();
                int imgIndex = 1;
                foreach (var srcImgPath in _selectedInGameImages)
                {
                    if (srcImgPath.Contains(targetGameFolder))
                    {
                        finalInGameRelativePaths.Add(Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, srcImgPath));
                    }
                    else
                    {
                        string imgExt = Path.GetExtension(srcImgPath);
                        string imgName = $"ingame_{imgIndex++}{imgExt}";
                        string targetImgPath = Path.Combine(targetGameFolder, imgName);

                        File.Copy(srcImgPath, targetImgPath, true);
                        finalInGameRelativePaths.Add(Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, targetImgPath));
                    }
                }

                GameModels targetGame = _isEditMode ? _editingGame : new GameModels();
                targetGame.Id = gameId;
                targetGame.Title = txtTitle.Text.Trim();
                targetGame.Type = (cbType.SelectedItem as ComboBoxItem)?.Content.ToString();
                targetGame.isNSFW = tgNsfw.IsChecked == true;
                targetGame.Description = txtDescription.Text;
                targetGame.AddedDate = dpAddedDate.SelectedDate?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd");
                targetGame.ReleaseDate = dpReleaseDate.SelectedDate?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd");
                targetGame.Thumbnail = finalThumbnailRelativePath;
                targetGame.ImageInGame = finalInGameRelativePaths;
                targetGame.SeriesId = cbRelatedGame.SelectedValue?.ToString();

                targetGame.FolderName = !string.IsNullOrEmpty(_selectedSingleFilePath)
                    ? gameId
                    : Path.GetFileName(_selectedGameFolderPath);

                if (!string.IsNullOrEmpty(mainFileRelativePath))
                {
                    targetGame.MainFile = mainFileRelativePath;
                }
                else if (string.IsNullOrEmpty(targetGame.MainFile))
                {
                    var files = Directory.GetFiles(targetGameFolder, "*.*", SearchOption.AllDirectories)
                        .Where(f => f.EndsWith(".exe") || f.EndsWith(".html") || f.EndsWith(".swf") || f.EndsWith(".index.html")).ToList();
                    if (files.Count > 0)
                    {
                        targetGame.MainFile = Path.GetRelativePath(targetGameFolder, files[0]);
                    }
                }

                if (!_isEditMode)
                {
                    targetGame.isFavorite = false;
                    _allGames.Add(targetGame);
                }
                else
                {
                    int idx = _allGames.FindIndex(g => g.Id == gameId);
                    if (idx != -1) _allGames[idx] = targetGame;
                }

                File.WriteAllText(jsonPath, JsonSerializer.Serialize(_allGames, new JsonSerializerOptions { WriteIndented = true }));

                MessageBox.Show(_isEditMode ? "Cập nhật thông tin Game thành công!" : "Thêm mới Game thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi trong quá trình lưu trữ: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                File.Copy(file, Path.Combine(destinationDir, Path.GetFileName(file)), true);
            }
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                CopyDirectory(subDir, Path.Combine(destinationDir, Path.GetFileName(subDir)));
            }
        }
        #endregion

        private void CloseButton_Click(object sender, RoutedEventArgs e) => this.Close();
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void MaximizeButton_Click(object sender, RoutedEventArgs e) => this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

        
    }
}
