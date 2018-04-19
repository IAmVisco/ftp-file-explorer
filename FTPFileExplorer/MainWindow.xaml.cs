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
        #region Extension lists
        List<string> picExt = new List<string>()
        {
            "img/picture.png", "jpg", "jpeg", "gif", "png", "bmp"
        };
        List<string> archiveExt = new List<string>()
        {
            "img/archive.png", "zip", "rar", "7z", "tar", "gz", "jar"
        };
        List<string> docsExt = new List<string>()
        {
            "img/docs.png", "doc", "docx"
        };
        List<string> sheetsExt = new List<string>()
        {
            "img/sheets.png", "xls", "xlsx"
        };
        List<string> musicExt = new List<string>()
        {
            "img/music.png", "mp3", "flac", "ogg", "wav", "ac3", "wma", "m4a"
        };
        List<string> videoExt = new List<string>()
        {
            "img/video.png", "avi", "wmw", "mkv", "3gp", "flv", "mpeg", "mp4", "mov", "vob"
        };
        #endregion

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
            EntryControl entry = null;

            try
            {
                Cursor = Cursors.AppStarting;
                await Task.Run(() =>
                {
                    //try
                    {
                        client = new FtpClient.Client(uri, login, pass);
                        r = client.ListDirectoryDetails();
                    }
                    //catch (Exception ex) { } needed in debug mode
                });
                Cursor = Cursors.Arrow;

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
                        #region Extension check
                        if (picExt.Contains(ext))
                        {
                            img = picExt[0];
                        }
                        else if (archiveExt.Contains(ext))
                        {
                            img = archiveExt[0];
                        }
                        else if (docsExt.Contains(ext))
                        {
                            img = docsExt[0];
                        }
                        else if (sheetsExt.Contains(ext))
                        {
                            img = sheetsExt[0];
                        }
                        else if (musicExt.Contains(ext))
                        {
                            img = musicExt[0];
                        }
                        else if (videoExt.Contains(ext))
                        {
                            img = videoExt[0];
                        }
                        else if (ext == "txt")
                        {
                            img = "img/txt.png";
                        }
                        else if (ext == "pdf")
                        {
                            img = "img/pdf.png";
                        }
                        else
                        {
                            img = "img/file.png";
                        }
                        #endregion
                        string size = "";
                        if (type == "file")
                            size = string.Format("{0:n0}", (Int64.Parse(match.Groups[3].Value.Trim()) / 1024)) + " KB";
                        entry = new EntryControl(size, type, match.Groups[6].Value, match.Groups[4].Value, img, uri);
                        entry.MouseDoubleClick += FolderClick;
                        return entry;
                    }
                    else
                        return new EntryControl();
                }).ToList();
                if (!(uri.Count(c => c == '/') <= 3))
                {
                    entry = new EntryControl("", "up", "Up", "", "img/up.png", uri);
                    entry.MouseDoubleClick += FolderClick;
                    list.Add(entry);
                }
                list.Reverse();

                filesList.Items.Clear();
                foreach (EntryControl entryControl in list)
                    filesList.Items.Add(entryControl);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString() + ": \n" + ex.Message);
            }
            finally
            {
                Cursor = Cursors.Arrow;
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
