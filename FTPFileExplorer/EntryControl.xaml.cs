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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FTPFileExplorer
{
    /// <summary>
    /// Interaction logic for EntryControl.xaml
    /// </summary>
    public partial class EntryControl : UserControl
    {
        string type;
        public string address;
        public string Type
        {
            get { return type; }
            set { type = value; }
        }

        public EntryControl()
        {
            InitializeComponent();
        }

        public EntryControl(string fileSize, string type, string filename, string date, string img, string address)
        {
            InitializeComponent();
            BitmapImage bm = new BitmapImage();
            bm.BeginInit();
            bm.UriSource = new Uri(img, UriKind.Relative);
            bm.EndInit();
            FileSize.Text = fileSize;
            Type = type;
            FileName.Text = filename;
            Date.Text = date;
            Img.Source = bm;
            this.address = address;
        }
    }
}
