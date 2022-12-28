using FluentFTP;
using System;
using System.IO;

namespace cs_FileSystemWatcher
{
    internal class MyClassCS
    {
        private static IniFile ini = new IniFile("./setup.ini");  // 환경설정 파일
        private static string _watcherdir = ini.IniReadValue("setup", "wdir");  // 감시할 디렉토리
        //private static string _savedir = ini.IniReadValue("setup", "savedir");  // 저장할 디렉토리

        //private static MariaDB mydb = new MariaDB("localhost", "3306", "root", "1234qwer", "Hana");
        private static MariaDB mydb = new MariaDB("localhost", "3306", "root", "25800478*", "filecheck");

        private static string ftpServerIP = "192.168.0.101";
        private static string ftpUser = "visftp";
        private static string ftpPwd = "viscorp1!";

        private static void Main()
        {
            ConnDB();

            var watcher = new FileSystemWatcher();
            watcher.Path = _watcherdir;
            //var watcher = new FileSystemWatcher(@"C:\Users\fromj\Downloads");

            watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            watcher.Created += OnCreated;
            watcher.Filter = "*.*";
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            string sFilename = e.Name;
            string sFilepath = e.FullPath;

            Console.Write("{0} ---> ", sFilepath);

            int subdirpos = sFilename.LastIndexOf("\\");
            string sFilename2 = sFilename.Substring(subdirpos + 1, sFilename.Length - subdirpos - 1);
            FtpUpd(sFilepath, sFilename2);

            Console.WriteLine("OK");
            InsDB(sFilepath, sFilename2);
        }

        private static void ConnDB()
        {
            if (mydb.GetDBConnectTest() == true)
            {
                Console.WriteLine("db connect");
            }
            else
            {
                Console.WriteLine("db is not connected");
            }
        }

        private static void InsDB(string sFilepath, string sFilename)
        {
            string sWdate, sWtime;
            string sQry;

            // mariadb 에 정보 저장
            mydb.DBConnect();
            sWdate = DateTime.Now.ToString("yyyy-MM-dd");
            sWtime = DateTime.Now.ToString("HH:mm:ss.ffff");
            sQry = string.Format("insert into FILELIST (FILEPATH, FILENAME, WDATE, WTIME)" +
                " SELECT '{0}', '{1}', '{2}', '{3}' " +
            " FROM DUAL" +
            " WHERE NOT EXISTS (SELECT 1 FROM FILELIST WHERE " +
                " FILEPATH='{0}' AND FILENAME='{1}' AND WDATE='{2}');", sFilepath, sFilename, sWdate, sWtime);

            mydb.ExecuteQuery(sQry);
            mydb.DBDisconnect();
        }

        private static void FtpUpd(string updFile, string updFilename)
        {
            using (var ftp = new FtpClient(ftpServerIP, ftpUser, ftpPwd))
            {
                ftp.AutoConnect();
                ftp.Config.RetryAttempts = 3;
                ftp.UploadFile(updFile, "/FTP/" + updFilename, FtpRemoteExists.Overwrite, true, FtpVerify.Retry);
            }
        }
    }
}