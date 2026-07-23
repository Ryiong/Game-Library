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
using System.Windows.Shapes;

namespace Game_Library
{
    /// <summary>
    /// Interaction logic for AddFutureGameWindow.xaml
    /// </summary>
    public partial class AddFutureGameWindow : Window
    {
        public AddFutureGameViewModel ViewModel { get; }
        public AddFutureGameWindow()
        {
            InitializeComponent();
            ViewModel = new AddFutureGameViewModel();
            this.DataContext = ViewModel;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Cancel_Click(sender, e);
            this.DialogResult = false;
            this.Close();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void MaximizeButton_Click(object sender, RoutedEventArgs e) => this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }
}
