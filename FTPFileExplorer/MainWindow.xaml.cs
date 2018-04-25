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
using Microsoft.Win32;

namespace FTPFileExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string prevAddress = "ftp://";
        FtpClient.Client client = null;
        bool isLoading = false;

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
            statusBox.Text = "";
        }

        private async void ConnectBtnClick(object sender, RoutedEventArgs e)
        {
            string uri = addressBox.Text.Trim();
            string login = loginBox.Text.Trim();
            string pass = passBox.Password.Trim();
            string[] r = { };
            EntryControl entry = null;

            if (!(uri.StartsWith("ftp://")))
            {
                addressBox.Text = "ftp://" + addressBox.Text;
                uri = addressBox.Text;
            }
            if (!(uri.EndsWith("/")))
            {
                addressBox.Text = addressBox.Text + "/";
                uri = addressBox.Text;
            }

            try
            {
                Cursor = Cursors.AppStarting;
                ripple.Visibility = Visibility.Visible;
                statusBox.Text = "Connecting...";
                await Task.Run(() =>
                {
                    try
                    {
                        client = new FtpClient.Client(uri, login, pass);
                        r = client.ListDirectoryDetails();
                    }
                    catch (Exception ex) 
                    {
                        MessageBox.Show(ex.Message);
                    }                   
                });
                Cursor = Cursors.Arrow;
                ripple.Visibility = Visibility.Hidden;

                Regex regex = new Regex(@"^([d-])([rwxt-]{3}){3}\s+\d{1,}\s+.*?(\d{1,})\s+(\w+\s+\d{1,2}\s+(?:\d{4})?)(\d{1,2}:\d{2})?\s+(.+?)\s?$",
                    RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                List<EntryControl> list = r.Select(s =>
                {
                    Match match = regex.Match(s);
                    if (match.Length > 5)
                    {
                        string type = match.Groups[1].Value == "d" ? "dir" : "file";
                        string img = "";
                        string ext = Path.GetExtension(match.Groups[6].Value).Trim('.').ToLower();
                        #region Extension check
                        if (type == "dir")
                        {
                            img = "img/folder.png";
                        }
                        else if (picExt.Contains(ext))
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
                    entry = new EntryControl("", "up", "..", "", "img/up.png", uri);
                    entry.MouseDoubleClick += FolderClick;
                    list.Add(entry);
                }
                list.Reverse();
                
                filesList.Items.Clear();
                foreach (EntryControl entryControl in list)
                {
                    filesList.Items.Add(entryControl);
                    if (entryControl.Type == "dir")
                        entryControl.ContextMenu =  this.FindResource("cmFolder") as ContextMenu;
                    else
                        entryControl.ContextMenu = this.FindResource("cmFile") as ContextMenu;
                }
                statusBox.Text = "";
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
                GoBack();
            }
            else
            {
                DownloadFile(entry);
            }
        }

        private void GoBack()
        {
            addressBox.Text = addressBox.Text.Substring(0, addressBox.Text.Remove(addressBox.Text.Length - 1).LastIndexOf('/') + 1);
            ConnectBtnClick(null, null);
        }
   
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
                ConnectBtnClick(null, null);
            if (e.Key == Key.BrowserBack)
                GoBack();
        }

        private void pBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            percentage.Text = Math.Truncate((pBar.Value / pBar.Maximum) * 100).ToString() + "% " + pBar.Value.ToString() + "/" + pBar.Maximum.ToString();
        }

        private void FileDLClick(object sender, RoutedEventArgs e)
        {
            DownloadFile(filesList.SelectedItem as EntryControl);
        }

        private async void DownloadFile(EntryControl entry)
        {
            if (isLoading)
            {
                MessageBox.Show("Another download in progress,\nplease wait for it to finish.");
                return;
            }
            SaveFileDialog sfd = new SaveFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads",
                RestoreDirectory = true,
                Filter = "All files(*.*)|*.*",
                FileName = entry.FileName.Text
            };

            if (sfd.ShowDialog() == true)
            {
                pBar.Visibility = Visibility.Visible;
                percentage.Visibility = Visibility.Visible;
                string status = "";
                string filename = entry.FileName.Text;
                isLoading = true;

                await Task.Run(() =>
                {
                    status = client.DownloadFile(filename, sfd.FileName, pBar);
                });

                isLoading = false;

                statusBox.Text = status.Substring(4);
                pBar.Visibility = Visibility.Hidden;
                percentage.Visibility = Visibility.Hidden;
            }
        }

        private async void UploadFile()
        {
            try
            {
                if (isLoading)
                {
                    MessageBox.Show("Another download in progress,\nplease wait for it to finish.");
                    return;
                }
                OpenFileDialog ofd = new OpenFileDialog
                {
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    RestoreDirectory = true,
                    Filter = "All files(*.*)|*.*",
                };

                if (ofd.ShowDialog() == true)
                {
                    pBar.Visibility = Visibility.Visible;
                    percentage.Visibility = Visibility.Visible;
                    string status = "";
                    string url = addressBox.Text;
                    isLoading = true;

                    await Task.Run(() =>
                    {
                        status = client.UploadFile(ofd.FileName, ofd.SafeFileName, pBar);
                    });

                    isLoading = false;

                    statusBox.Text = status.Substring(4);
                    pBar.Visibility = Visibility.Hidden;
                    percentage.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FileULClick(object sender, RoutedEventArgs e)
        {
            UploadFile();
        }

        private void filesList_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var selection = ((ListBox)sender).InputHitTest(Mouse.GetPosition(filesList));

            if (selection is ScrollViewer)
            {
                var cm = this.FindResource("cmUp") as ContextMenu;
                cm.IsOpen = true;
            }

        }
    }
}