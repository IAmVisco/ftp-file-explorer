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
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace FTPFileExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string prevAddress = "ftp://";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConnectBtnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                FtpClient.Client client = new FtpClient.Client(addressBox.Text, loginBox.Text, passBox.Password);

                Regex regex = new Regex(@"^([d-])([rwxt-]{3}){3}\s+\d{1,}\s+.*?(\d{1,})\s+(\w+\s+\d{1,2}\s+(?:\d{4})?)(\d{1,2}:\d{2})?\s+(.+?)\s?$",
                    RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

                List<FileDirectoryInfo> list = client.ListDirectoryDetails()
                    .Select(s =>
                    {
                        Match match = regex.Match(s);
                        if (match.Length > 5)
                        {
                            string type = match.Groups[1].Value == "d" ? "dir" : "file";
                            string img = type == "dir" ? "img/folder.png" : "img/file.png";
                            string size = "";
                            if (type == "file")
                                size = (Int64.Parse(match.Groups[3].Value.Trim()) / 1024).ToString() + "kBytes";

                            return new FileDirectoryInfo(size, type, match.Groups[6].Value, match.Groups[4].Value, img, addressBox.Text);
                        }
                        else
                            return new FileDirectoryInfo();
                    }).ToList();
                list.Add(new FileDirectoryInfo("", "up", "...", "", "up.png", addressBox.Text));
                list.Reverse();

                lbx_files.DataContext = list;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString() + ": \n" + ex.Message);
            }
        }

        private void FolderClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                FileDirectoryInfo fdi = (FileDirectoryInfo)(sender as StackPanel).DataContext;
                if (fdi.Type == "dir")
                {
                    prevAddress = fdi.address;
                    addressBox.Text = fdi.address + fdi.Name + "/";
                    ConnectBtnClick(null, null);
                }
                // else if (click on up) { return }
            }
        }
    }

    public class FileDirectoryInfo
    {
        string fileSize;
        string type;
        string name;
        string date;
        string img;
        public string address;

        public string FileSize
        {
            get { return fileSize; }
            set { fileSize = value; }
        }

        public string Type
        {
            get { return type; }
            set { type = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Date
        {
            get { return date; }
            set { date = value; }
        }

        public string Img
        {
            get { return img; }
            set { img = value; }
        }

        public FileDirectoryInfo() { }

        public FileDirectoryInfo(string fileSize, string type, string name, string date, string img, string address)
        {
            FileSize = fileSize;
            Type = type;
            Name = name;
            Date = date;
            Img = img;
            this.address = address;
        }

    }
}
