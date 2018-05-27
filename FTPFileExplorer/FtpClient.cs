using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace FTPFileExplorer
{
    class FtpClient
    {
        public class Client
        {
            private string username;
            private string password = "";
            private string url;
            private int buffSize = 1024;

            public bool Passive = true;
            public bool Binary = true;
            public bool EnableSSL = false;

            public Client(string url, string username, string password)
            {
                this.url = url;
                this.username = username;
                this.password = password;
            }

            private string CombinePaths(string path1, string path2)
            {
                return Path.Combine(path1, path2).Replace("\\", "/");
            }

            private FtpWebRequest CreateRequest(string method)
            {
                return CreateRequest(url, method);
            }

            private FtpWebRequest CreateRequest(string url, string method)
            {
                FtpWebRequest r = (FtpWebRequest)WebRequest.Create(url);

                r.Credentials = new NetworkCredential(username, password);
                r.Method = method;
                r.UseBinary = Binary;
                r.EnableSsl = EnableSSL;
                r.UsePassive = Passive;

                return r;
            }

            private string GetStatusDescription(FtpWebRequest request)
            {
                using (var response = (FtpWebResponse)request.GetResponse())
                {
                    return response.StatusDescription;
                }
            }

            public string PrintDir()
            {
                var req = CreateRequest(WebRequestMethods.Ftp.PrintWorkingDirectory);

                return GetStatusDescription(req);
            }

            public string ChangeDir(string path)
            {
                url = CombinePaths(url, path);
                return PrintDir();
            }

            public long GetFileSize(string fileName)
            {
                FtpWebRequest request = CreateRequest(CombinePaths(url, fileName), WebRequestMethods.Ftp.GetFileSize);

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    return response.ContentLength;
                }
            }

            public string[] ListDirectory()
            {
                List<string> list = new List<string>();

                FtpWebRequest request = CreateRequest(WebRequestMethods.Ftp.ListDirectory);

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream, true))
                        {
                            while (!reader.EndOfStream)
                            {
                                list.Add(reader.ReadLine());
                            }
                        }
                    }
                }
                return list.ToArray();
            }

            public string[] ListDirectoryDetails()
            {
                List<string> list = new List<string>();

                FtpWebRequest request = CreateRequest(WebRequestMethods.Ftp.ListDirectoryDetails);

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream, true))
                        {
                            while (!reader.EndOfStream)
                            {
                                list.Add(reader.ReadLine());
                            }
                        }
                    }
                }
                return list.ToArray();
            }

            public string DownloadFile(string source, string dest, ProgressBar pBar)
            {
                FtpWebRequest request = CreateRequest(CombinePaths(url, source), WebRequestMethods.Ftp.DownloadFile);

                pBar.Dispatcher.Invoke(() =>
                {
                    if (GetFileSize(source) <= 0)
                        pBar.IsIndeterminate = true;
                    else
                    {
                        pBar.IsIndeterminate = false;
                        pBar.Maximum = GetFileSize(source);
                        pBar.Value = 0;
                    }
                });

                byte[] buffer = new byte[buffSize];

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (FileStream fs = new FileStream(dest, FileMode.OpenOrCreate))
                        {
                            int readCount = stream.Read(buffer, 0, buffSize);

                            while (readCount > 0)
                            {                                
                                fs.Write(buffer, 0, readCount);

                                pBar.Dispatcher.Invoke(() =>
                                {
                                    pBar.Value = pBar.Value + readCount;
                                });

                                readCount = stream.Read(buffer, 0, buffSize);
                            }
                        }
                    }
                    return response.StatusDescription;
                }
            }

            public string UploadFile(string source, string dest, ProgressBar pBar)
            {
                FtpWebRequest request = CreateRequest(CombinePaths(url, dest), WebRequestMethods.Ftp.UploadFile);

                using (var stream = request.GetRequestStream())
                {
                    using (FileStream fileStream = File.Open(source, FileMode.Open))
                    {
                        int num;

                        pBar.Dispatcher.Invoke(() =>
                        {
                            if (fileStream.Length <= 0)
                                pBar.IsIndeterminate = true;
                            else
                            {
                                pBar.IsIndeterminate = false;
                                pBar.Maximum = fileStream.Length;
                                pBar.Value = 0;
                            }
                        });
                        byte[] buffer = new byte[buffSize];

                        while ((num = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            stream.Write(buffer, 0, num);

                            pBar.Dispatcher.Invoke(() =>
                            {
                                pBar.Value = pBar.Value + num;
                            });
                        }
                    }
                }

                return GetStatusDescription(request);
            }

            public string Rename(string curName, string newName)
            {
                var request = CreateRequest(CombinePaths(url, curName), WebRequestMethods.Ftp.Rename);

                request.RenameTo = newName;

                return GetStatusDescription(request);
            }

            public string DeleteFile(string fileName)
            {
                var request = CreateRequest(CombinePaths(url, fileName), WebRequestMethods.Ftp.DeleteFile);

                return GetStatusDescription(request);
            }

            public string CreateFolder(string directoryName)
            {
                var request = CreateRequest(CombinePaths(url, directoryName), WebRequestMethods.Ftp.MakeDirectory);

                return GetStatusDescription(request);
            }

            public string RemoveFolder(string directoryName)
            {
                var request = CreateRequest(CombinePaths(url, directoryName), WebRequestMethods.Ftp.RemoveDirectory);

                return GetStatusDescription(request);
            }
        }
    }
}
