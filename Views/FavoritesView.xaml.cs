using Game_Library.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;

namespace Game_Library
{
    /// <summary>
    /// Interaction logic for FavoritesView.xaml
    /// </summary>
    public partial class FavoritesView : System.Windows.Controls.UserControl
    {
        private readonly string allGamesJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "game.json");

        public FavoritesView()
        {
            InitializeComponent();
            LoadFavoriteGames();
        }

        private void LoadFavoriteGames()
        {
            try
            {
                if (File.Exists(allGamesJsonPath))
                {
                    string jsonContent = File.ReadAllText(allGamesJsonPath);
                    var allGames = JsonSerializer.Deserialize<List<GameModels>>(jsonContent);

                    if (allGames != null)
                    {
                        var favoriteGames = allGames.FindAll(g => g.isFavorite);
                        FavoritesGameLayout.ItemsSource = favoriteGames;
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
