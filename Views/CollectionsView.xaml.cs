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

namespace Game_Library.Views
{
    /// <summary>
    /// Interaction logic for CollectionsView.xaml
    /// </summary>
    public partial class CollectionsView : System.Windows.Controls.UserControl
    {
        public CollectionsView()
        {
            InitializeComponent();
            this.DataContext = new CollectionsViewModel();
        }
    }
}
