using Game_Library.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace Game_Library.Views
{
    public partial class AllGamesView : UserControl
    {
        private readonly string allGamesJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "game.json");
        private readonly string recentGameJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "recent_game.json");

        public AllGamesView()
        {
            InitializeComponent();
            InitializeJsonFiles();
            LoadDataFromJsons();
        }

        private void InitializeJsonFiles()
        {
            if (!File.Exists(allGamesJsonPath))
            {
                CreateSampleJson();
            }
        }

        private void CreateSampleJson()
        {
            try
            {
                var allGamesSample = new List<GameModels>
                {
                    new GameModels { Title = "Vector Siege", Type = "FLASH", Thumbnail = "Resources/IMG_1067_500.jpg", AddedDate = "Added Mar 12, 2026", ReleaseDate = "Mar 12, 2026", isFavorite = true, Description="Game bắn súng Flash không gian Vector cổ điển cực kỳ hấp dẫn.", MainFile="vector_siege.swf", FolderName="vector_siege" },
                    new GameModels { Title = "HTML5 Indie Adventure", Type = "HTML5", Thumbnail = "Resources/IMG_1079_500.jpg", AddedDate = "Added Mar 12, 2026", ReleaseDate = "Nov 20, 2025", isFavorite = false, Description="Một tựa game phiêu lưu mã nguồn mở HTML5.", MainFile="index.html", FolderName="indie_adventure" }
                };
                File.WriteAllText(allGamesJsonPath, JsonSerializer.Serialize(allGamesSample, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        }

        private void LoadDataFromJsons()
        {
            try
            {
                if (File.Exists(allGamesJsonPath))
                {
                    var allGames = JsonSerializer.Deserialize<List<GameModels>>(File.ReadAllText(allGamesJsonPath));
                    AllGamesLayout.ItemsSource = allGames;
                }
                if (File.Exists(recentGameJsonPath))
                {
                    var recentGame = JsonSerializer.Deserialize<GameModels>(File.ReadAllText(recentGameJsonPath));
                    if (recentGame != null) { RecentSection.DataContext = recentGame; RecentSection.Visibility = Visibility.Visible; }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message); }
        }

        private void GameCard_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is GameModels selectedGame && Application.Current.MainWindow is MainWindow main)
            {
                main.NavigateToDetail(selectedGame);
            }
        }

        private void PlayAgainBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is GameModels selectedGame && Application.Current.MainWindow is MainWindow main)
            {
                main.NavigateToDetail(selectedGame); // Điều hướng trực tiếp sang trang Detail của game để khởi chạy mạch lạc
            }
        }
    }
}