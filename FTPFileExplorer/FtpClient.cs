using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace FTPFileExplorer
{
    class FtpClient
    {
        public class Client
        {
            private string username;
            private string password = "";
            private string url;
            private int bufSize = 1024;

            public bool Passive = true;
            public bool Binary = true;
            public bool EnableSSL = false;
            public bool Hash = false;

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
                var r = (FtpWebRequest)WebRequest.Create(url);

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


        }
    }
}
