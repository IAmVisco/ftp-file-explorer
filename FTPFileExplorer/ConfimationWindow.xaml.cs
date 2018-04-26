using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FTPFileExplorer
{
    /// <summary>
    /// Interaction logic for ConfimationWindow.xaml
    /// </summary>
    public partial class ConfimationWindow : Window
    {
        public ConfimationWindow(string fileName)
        {
            InitializeComponent();
            msgBox.Text = "Do you really want to delete " + fileName + "?\nThis can't be undone.";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            nBtn.Focus();
        }

        private void BtnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
