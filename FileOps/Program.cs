using System;
using System.Security.Cryptography;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Linq;

namespace FileOps
{

    class Program
    {
        static void Main(string[] args)
        {
            var lastModifiedFileName = string.Empty;

            var clientFolderPath = ConfigurationManager.AppSettings.Get("ClientFolderPath");

            // Get Previouly saved modifiled hash value
            var outPutfileDataInfo = GetFileInfoWithHash();

            DirectoryInfo directoryInfo = new DirectoryInfo(clientFolderPath);

            List<Tuple<string, string, DateTime>> filesWithHash = GetFilesWithHash(directoryInfo.GetFiles());

            filesWithHash = filesWithHash.OrderByDescending(s => s.Item3).ToList();

            if (outPutfileDataInfo.Count() == 0)
            {
                var lastModifiedFileInfo = filesWithHash.FirstOrDefault();

                // Write Last Modified file into Output folder
                WriteToOutPutFile(new Dictionary<string, string>()
                {
                    { lastModifiedFileInfo.Item1, lastModifiedFileInfo.Item2 }
                });

                lastModifiedFileName = lastModifiedFileInfo.Item1;
            }
            else
            {
                if (filesWithHash.Any(s => s.Item2 == outPutfileDataInfo.Values.FirstOrDefault()))
                {
                    Console.WriteLine("Last modified file same as previous one");
                }
                else
                {
                    var lastModifiedFileInfo = filesWithHash.FirstOrDefault();

                    // Write Last Modified file into Output folder
                    WriteToOutPutFile(new Dictionary<string, string>()
                    {
                        { lastModifiedFileInfo.Item1, lastModifiedFileInfo.Item2 }
                    });

                    lastModifiedFileName = lastModifiedFileInfo.Item1;
                }
            }
        }

        private FileInfo[] GetFileInfos(string folderPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
            return directoryInfo.GetFiles();
        }

        private static byte[] GetHashMd5(string filename)
        {
            using (FileStream stream = File.OpenRead(filename))
            {
                var md5 = MD5.Create();
                return md5.ComputeHash(stream);
            }
        }

        public static string BytesToString(byte[] bytes)
        {
            string result = "";
            foreach (byte b in bytes) result += b.ToString("x2");
            return result;
        }

        private static List<Tuple<string, string, DateTime>> GetFilesWithHash(FileInfo[] fileInfos)
        {
            var resultDictionary = new List<Tuple<string, string, DateTime>>();

            foreach (var fileInfo in fileInfos)
            {
                resultDictionary.Add(new Tuple<string, string, DateTime>(fileInfo.Name, BytesToString(GetHashMd5(fileInfo.FullName)), File.GetLastWriteTime(fileInfo.FullName)));
            }

            return resultDictionary;
        }

        private static void WriteToOutPutFile(Dictionary<string, string> fileInfo)
        {
            var folderPath = ConfigurationManager.AppSettings.Get("OutPutFolderPath");

            var fileName = ConfigurationManager.AppSettings.Get("OutPutFileName");

            // Check first if the folder is exist
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var directoryInfo = new DirectoryInfo(folderPath);

            StringBuilder stringBuilder = new StringBuilder();

            foreach (var file in fileInfo)
            {
                stringBuilder.AppendLine($"{file.Key}, {file.Value}");
            }

            File.WriteAllText($@"{directoryInfo.FullName}\{fileName}", stringBuilder.ToString());
        }

        private static Dictionary<string, string> GetFileInfoWithHash()
        {
            var resultDictionary = new Dictionary<string, string>();

            var folderPath = ConfigurationManager.AppSettings.Get("OutPutFolderPath");

            var fileName = ConfigurationManager.AppSettings.Get("OutPutFileName");

            var directoryInfo = new DirectoryInfo(folderPath);

            FileInfo file = directoryInfo.GetFiles().FirstOrDefault();

            if (file != null)
            {
                string[] fileContents = File.ReadAllLines(file.FullName);

                foreach (var fileContent in fileContents)
                {
                    var fileDataInfo = fileContent.Split(",");
                    resultDictionary.Add(fileDataInfo[0].Trim(), fileDataInfo[1].Trim());
                }
            }

            return resultDictionary;
        }
    }
}
