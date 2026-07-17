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

namespace Game_Library.Views
{
    public partial class AllGamesView : System.Windows.Controls.UserControl
    {
        private readonly string allGamesJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "game.json");

        public AllGamesView()
        {
            InitializeComponent();
            LoadDataFromJsons();
        }

        private void LoadDataFromJsons()
        {
            try
            {
                if (File.Exists(allGamesJsonPath))
                {
                    var allGames = JsonSerializer.Deserialize<List<GameModels>>(File.ReadAllText(allGamesJsonPath));
                    if (allGames != null)
                    {
                        if (MainWindow.IsNsfwEnabled)
                        {
                            AllGamesLayout.ItemsSource = allGames;
                        }
                        else
                        {
                            var safeGame = allGames.Where(g => (!g.isNSFW)).ToList();
                            AllGamesLayout.ItemsSource = safeGame;
                        }
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