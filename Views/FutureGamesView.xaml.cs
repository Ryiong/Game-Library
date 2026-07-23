using Game_Library.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UserControl = System.Windows.Controls.UserControl;

namespace Game_Library.Views
{
    /// <summary>
    /// Interaction logic for FutureGamesView.xaml
    /// </summary>
    public partial class FutureGamesView : UserControl
    {
        public FutureGamesViewModel viewModel {  get; set; }
        public FutureGamesView()
        {
            InitializeComponent();
            viewModel = new FutureGamesViewModel();
            this.DataContext = viewModel;
        }
    }
}
