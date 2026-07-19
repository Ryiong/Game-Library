using Game_Library.Models;
using Game_Library.Services;
using Game_Library.ViewModels;
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
    public partial class FlashClassicsGames : System.Windows.Controls.UserControl
    {
        public FlashClassicsGames()
        {
            InitializeComponent();
            this.DataContext = new FlashClassicsViewModel();
        }

    }
}