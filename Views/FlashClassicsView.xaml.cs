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
    /// Interaction logic for FlashClassicsView.xaml
    /// </summary>
    public partial class FlashClassicsView : UserControl
    {
        private readonly string allGamesJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "game.json");
        public FlashClassicsView()
        {
            InitializeComponent();
            LoadFlashGames();
        }

        private void LoadFlashGames()
        {
            try
            {
                if (File.Exists(allGamesJsonPath))
                {
                    string jsonContent = File.ReadAllText(allGamesJsonPath);
                    var allGames = JsonSerializer.Deserialize<List<GameModels>>(jsonContent);

                    if (allGames != null)
                    {
                        var flashGames = allGames.FindAll(g => g.Type != null && g.Type.ToUpper() == "FLASH");
                        FlashClassicsGameLayout.ItemsSource = flashGames;
                    }
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
    }
}
