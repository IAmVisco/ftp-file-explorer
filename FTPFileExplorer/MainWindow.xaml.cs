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
        string prevAddress = "ftp://", status = "";
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
            "img/docs.png", "doc", "docx", "txt"
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
                statusBox.Text = status;
                status = "";               
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
                Refresh();
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
            addressBox.Text = addressBox.Text.Substring(0, 
                addressBox.Text.Remove(addressBox.Text.Length - 1).LastIndexOf('/') + 1);
            Refresh();
        }

        private void Refresh()
        {
            ConnectBtnClick(null, null);
        }

        private void ShowException(string exMsg)
        {
            ExceptionWindow exWin = new ExceptionWindow(exMsg)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            exWin.ShowDialog();
        }

        private void FileDLClick(object sender, RoutedEventArgs e)
        {
            DownloadFile(filesList.SelectedItem as EntryControl);
        }

        private void FileULClick(object sender, RoutedEventArgs e)
        {
            UploadFile();
        }

        private async void DownloadFile(EntryControl entry)
        {
            statusBox.Text = "";
            if (isLoading)
            {
                ShowException("Another loading is in progress,\nplease wait for it to finish.");
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
                string filename = entry.FileName.Text;
                isLoading = true;

                await Task.Run(() =>
                {
                    try
                    {
                        status = client.DownloadFile(filename, sfd.FileName, pBar);
                    }
                    catch(Exception ex)
                    {
                        ShowException(ex.Message);
                    }
                });

                isLoading = false;

                statusBox.Text = status;
                pBar.Visibility = Visibility.Hidden;
                percentage.Visibility = Visibility.Hidden;
            }
        }

        private async void UploadFile()
        {
            statusBox.Text = "";
            if (isLoading)
            {
                ShowException("Another loading is in progress,\nplease wait for it to finish.");
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
                string url = addressBox.Text;
                isLoading = true;

                await Task.Run(() =>
                {
                    try
                    {
                        status = client.UploadFile(ofd.FileName, ofd.SafeFileName, pBar);
                    }
                    catch(Exception ex)
                    {
                        ShowException(ex.Message);
                    }
                });

                isLoading = false;

                statusBox.Text = status;
                pBar.Visibility = Visibility.Hidden;
                percentage.Visibility = Visibility.Hidden;
                Refresh();
            }
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

        private void Rename(object sender, RoutedEventArgs e)
        {
            TextEnterWindow nameWin = new TextEnterWindow()
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            nameWin.okBtn.Click += (s, _) =>
            {
                string newName = nameWin.nameBox.Text.Trim();
                if (newName != "")
                {
                    try
                    { 
                        status = client.Rename((filesList.SelectedItem as EntryControl).FileName.Text, newName);
                        Refresh();
                    }
                    catch(Exception ex)
                    {
                        ShowException(ex.Message);
                    }
                }
            };
            nameWin.ShowDialog();
        }

        private void DeleteFile(object sender, RoutedEventArgs e)
        {
            EntryControl entry = filesList.SelectedItem as EntryControl;
            ConfimationWindow yesNoWin = new ConfimationWindow(entry.FileName.Text)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            yesNoWin.yBtn.Click += (s, _) =>
            {
                try
                {
                    status = client.DeleteFile(entry.FileName.Text);
                    Refresh();
                }
                catch (Exception ex)
                {
                    ShowException(ex.Message);
                }
            };
            yesNoWin.ShowDialog();
        }

        private void CreateFolder(object sender, RoutedEventArgs e)
        {
            TextEnterWindow nameWin = new TextEnterWindow()
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            nameWin.okBtn.Click += (s, _) =>
            {
                string newName = nameWin.nameBox.Text.Trim();
                if (newName != "")
                {
                    try
                    {
                        status = client.CreateFolder(newName);
                        Refresh();
                    }
                    catch (Exception ex)
                    {
                        ShowException(ex.Message);
                    }
                }
            };
            nameWin.ShowDialog();
        }

        private void RemoveFolder(object sender, RoutedEventArgs e)
        {
            EntryControl entry = filesList.SelectedItem as EntryControl;
            ConfimationWindow yesNoWin = new ConfimationWindow(entry.FileName.Text)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            yesNoWin.yBtn.Click += (s, _) =>
            {
                try
                {
                    status = client.RemoveFolder(entry.FileName.Text);
                    Refresh();
                }
                catch (Exception ex)
                {
                    ShowException(ex.Message);
                }
            };
            yesNoWin.ShowDialog();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            bool ctrlPressed = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);

            if (e.Key == Key.F5 ||
               (e.Key == Key.Enter && (addressBox.IsFocused || loginBox.IsFocused || passBox.IsFocused)))
                Refresh();

            if (e.Key == Key.BrowserBack)
                GoBack();

            if (ctrlPressed && e.Key == Key.S)
            {
                if (LeftBar.Width == new GridLength(0))
                    LeftBar.Width = new GridLength(0.3, GridUnitType.Star);
                else
                    LeftBar.Width = new GridLength(0);
            }
        }

        private void pBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            percentage.Text = Math.Truncate((pBar.Value / pBar.Maximum) * 100).ToString() + "% " + pBar.Value.ToString() + "/" + pBar.Maximum.ToString();
        }
    }
}