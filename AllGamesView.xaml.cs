using Game_Library.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace Game_Library
{
    /// <summary>
    /// Interaction logic for AllGamesView.xaml
    /// </summary>
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
#if DEBUG 
            CreateSampleJson();
#else
            if (!File.Exists(allGamesJsonPath) || !File.Exists(recentGameJsonPath))
            {
                CreateSampleJsons();
            }
#endif
        }

        private void CreateSampleJson()
        {
            try 
            {
                var allGamesSample = new List<GameModels>
                {
                    new GameModels { idGame = 1, Title = "Vector Siege", Type = "FLASH", Thumbnail = "", AddedDate = "Added Mar 12, 2026", isFavorite = true },
                    new GameModels { idGame = 2, Title = "Lorem Ipsum Indie Game", Type = "HTML5", Thumbnail = "", AddedDate = "Added Mar 12, 2026", isFavorite = true },
                    new GameModels { idGame = 3, Title = "Cyberpunk Classic", Type = "HTML5", Thumbnail = "", AddedDate = "Added Jun 04, 2026", isFavorite = true },
                    new GameModels { idGame = 4, Title = "Super Mario Flash", Type = "FLASH", Thumbnail = "", AddedDate = "Added Jul 11, 2026", isFavorite = false },
                    new GameModels { idGame = 5, Title = "Super Mario Flash", Type = "FLASH", Thumbnail = "", AddedDate = "Added Jul 11, 2026", isFavorite = false },
                    new GameModels { idGame = 6, Title = "Display Test Text Very Long Title Game Name", Type = "FLASH", Thumbnail = "", AddedDate = "Added Jul 11, 2026", isFavorite = false }
                };
                string allGamesJson = JsonSerializer.Serialize(allGamesSample, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(allGamesJsonPath, allGamesJson);

                var recentGameSample = new GameModels
                {
                    idGame = 1,
                    Title = "Vector Siege (Recent)",
                    Type = "FLASH",
                    Thumbnail = "",
                    LastPlayText = "Last played 2 hours ago"
                };
                string recentJson = JsonSerializer.Serialize(recentGameSample, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(recentGameJsonPath, recentJson);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi khởi tạo JSON: " + ex.Message);
            }
        }

        private void PlayAgainBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is GameModels selectedGame)
            {
                MessageBox.Show($"Đang chạy trò chơi: {selectedGame.Title}");
            }
        }

        private void LoadDataFromJsons()
        {
            // 1. Đọc file Game chơi gần đây
            try
            {
                if (File.Exists(recentGameJsonPath))
                {
                    string recentJsonString = File.ReadAllText(recentGameJsonPath);
                    GameModels recentGame = JsonSerializer.Deserialize<GameModels>(recentJsonString);

                    if (recentGame != null)
                    {
                        RecentSection.DataContext = recentGame;
                        RecentSection.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        RecentSection.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đọc file recent_game.json: " + ex.Message);
            }

            // 2. Đọc file Tất cả game
            try
            {
                if (File.Exists(allGamesJsonPath))
                {
                    string allGamesJsonString = File.ReadAllText(allGamesJsonPath);
                    List<GameModels> allGames = JsonSerializer.Deserialize<List<GameModels>>(allGamesJsonString);

                    if (allGames != null)
                    {
                        AllGamesLayout.ItemsSource = allGames;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đọc file game.json: " + ex.Message);
            }
        }
    }
}
