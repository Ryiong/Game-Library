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
using MessageBox = System.Windows.MessageBox;

namespace Game_Library
{
    /// <summary>
    /// Interaction logic for PasswordDialog.xaml
    /// </summary>
    public partial class PasswordDialog : Window
    {
        private const string CorrectPassword = "Rynsfw";
        public PasswordDialog()
        {
            InitializeComponent();
            txtPassword.Focus();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (txtPassword.Password == CorrectPassword)
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Mật khẩu không chính xác!", "Lỗi xác thực", MessageBoxButton.OK, MessageBoxImage.Error);
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.Close();

        private void CloseButton_Click(object sender, RoutedEventArgs e) => this.Close();
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void MaximizeButton_Click(object sender, RoutedEventArgs e) => this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }
}
