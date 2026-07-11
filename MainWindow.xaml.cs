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

namespace Game_Library
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DynamicContentViewer.Content = new AllGamesView();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (string.IsNullOrEmpty(textBox.Text))
            {
                SearchPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void AddGameButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng thêm Game thủ công đang được xây dựng!", "Thông báo");
        }

        private void SidebarMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DynamicContentViewer == null) return;

            switch (SidebarMenu.SelectedIndex)
            {
                case 0:
                    DynamicContentViewer.Content = new AllGamesView();
                    break;
                case 1:
                    DynamicContentViewer.Content = new FlashClassicsView();
                    break;
                case 2:
                    DynamicContentViewer.Content = new HTML5IndieView();
                    break;
                case 3:
                    DynamicContentViewer.Content = new FavoritesView();
                    break;
            } 
        }

        private void LiveServer_Toggle(object sender, RoutedEventArgs e)
        {
            if (ServerStatus == null) return;
            ServerStatus.Text = LiveServerButton.IsChecked == true ? "Server Status: Active" : "Server Status: Off";
        }
    }    
}