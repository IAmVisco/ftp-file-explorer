using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FTPFileExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string prevAddress = "ftp://";
        List<string> picExt = new List<string>()
        {
            "img/picture.png",
            "jpg",
            "jpeg",
            "gif",
            "png",
            "bmp"
        };
        List<string> archiveExt = new List<string>()
        {
            "img/archive.png",
            "zip",
            "rar",
            "7z",
        };

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void ConnectBtnClick(object sender, RoutedEventArgs e)
        {
            string uri = addressBox.Text;
            string login = loginBox.Text;
            string pass = passBox.Password;
            string[] r = { };
            FtpClient.Client client = null;
            Cursor = Cursors.AppStarting;
            await Task.Run(() =>
            {            
                client = new FtpClient.Client(uri, login, pass);
                r = client.ListDirectoryDetails();
            });
            Cursor = Cursors.Arrow;
            try
            {          
                EntryControl entry;
                Regex regex = new Regex(@"^([d-])([rwxt-]{3}){3}\s+\d{1,}\s+.*?(\d{1,})\s+(\w+\s+\d{1,2}\s+(?:\d{4})?)(\d{1,2}:\d{2})?\s+(.+?)\s?$",
                    RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
              
                List<EntryControl> list = r.Select(s =>
                {
                    Match match = regex.Match(s);
                    if (match.Length > 5)
                    {
                        string type = match.Groups[1].Value == "d" ? "dir" : "file";
                        string img = type == "dir" ? "img/folder.png" : "img/file.png";
                        string ext = (Path.GetExtension(match.Groups[6].Value).Trim('.'));
                        if (picExt.Contains(ext))
                        {
                            img = picExt[0];
                        }
                        else if (archiveExt.Contains(ext))
                        {
                            img = archiveExt[0];
                        }
                        string size = "";
                        if (type == "file")
                            size = string.Format("{0:n0}", (Int64.Parse(match.Groups[3].Value.Trim()) / 1024)) + " KB";
                        entry = new EntryControl(size, type, match.Groups[6].Value, match.Groups[4].Value, img, addressBox.Text);
                        entry.MouseDoubleClick += FolderClick;
                        return entry;
                    }
                    else
                        return new EntryControl();
                }).ToList();
                entry = new EntryControl("", "up", "Up", "", "img/up.png", addressBox.Text);
                entry.MouseDoubleClick += FolderClick;
                list.Add(entry);
                list.Reverse();
                filesList.Items.Clear();
                foreach (EntryControl entryControl in list)
                    filesList.Items.Add(entryControl);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString() + ": \n" + ex.Message);
            }

        }

        private void FolderClick(object sender, MouseButtonEventArgs e)
        {
            EntryControl entry = (sender as EntryControl);

            if (entry.Type == "dir")
            {
                prevAddress = entry.address;
                addressBox.Text = entry.address + entry.FileName.Text + "/";
                ConnectBtnClick(null, null);
            }
            else if (entry.Type == "up")
            {
                if (entry.address.LastIndexOf('/') + 1 == entry.address.Length)
                    addressBox.Text = entry.address.Substring(0, entry.address.Remove(entry.address.Length - 1).LastIndexOf('/') + 1);
                else
                    addressBox.Text = entry.address.Substring(0, entry.address.LastIndexOf('/') + 1); // useless?
                ConnectBtnClick(null, null);
            }            
        }
    }
}
