using System;
using System.Diagnostics;
using System.IO;
using System.IO.Enumeration;
using System.Runtime.CompilerServices;

namespace FileMetadataChanger
{
    enum MetadataFieldUpdateType {
        CreationTime,
        LastWriteTime,
    }

    class Program
    {
        /// <summary>
        /// Updates metadata fields for a file
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="date"></param>
        /// <param name="metadataFieldUpdateType"></param>
        static void UpdateMetadata(string fileName, DateTime date, List<MetadataFieldUpdateType> metadataFieldUpdateType)
        {
            if (!File.Exists(fileName))
            {
                Console.WriteLine($"File not found {fileName}.\n");
                return;
            };

            FileInfo fileInfo = new FileInfo(fileName);
            string feedback = $"Updating file: {fileInfo.Name}\n";
            foreach(var metadatafieldupdatetype in metadataFieldUpdateType)
            {
                if (metadatafieldupdatetype == MetadataFieldUpdateType.CreationTime)
                {
                    feedback += $" Creation Time: {fileInfo.CreationTime}.";
                    fileInfo.CreationTime = date;
                    feedback += $" --> {fileInfo.CreationTime}.\n";
                }
                if (metadatafieldupdatetype == MetadataFieldUpdateType.LastWriteTime)
                {
                    feedback += $" Write Time:    {fileInfo.LastWriteTime}.";
                    fileInfo.LastWriteTime = date;
                    feedback += $" --> {fileInfo.CreationTime}.\n";
                }
            }
            Console.WriteLine(feedback);
        }

        static void Main(string[] args)
        {
            var parameters = new Dictionary<string, string>();
            foreach (var arg in args) {
                if (arg.StartsWith("--")) {
                    var parts = arg.Substring(2).Split('=');
                    if (parts.Length == 2) {
                        parameters[parts[0]] = parts[1];
                    }
                }
            }

            // Get which type of update to perform
            string? metaUpdateTypeArg;
            var metaUpdateTypes = new List<MetadataFieldUpdateType>(); 
            if (parameters.TryGetValue("mode", out metaUpdateTypeArg))
            {
                var parts = metaUpdateTypeArg.Split(',');
                foreach(var part in parts)
                {
                    MetadataFieldUpdateType type;
                    if (Enum.TryParse(part, true, out type))
                    {
                        metaUpdateTypes.Add(type);
                    }
                }
            }
            if (metaUpdateTypes.Count() == 0)
            {
                Console.WriteLine("Missing required parameter:   --mode=\"CreationTime,LastWriteTime\"   use either or both.");
                Environment.Exit(2);
            }

            // Get the desired date, otherwise use current time
            string? dateArg;
            DateTime date = DateTime.Now;
            if (parameters.TryGetValue("date", out dateArg))
            {
                DateTime.TryParse(dateArg, out date);
            }

            // Get file to update
            string? filePath = "";
            if (parameters.TryGetValue("file", out filePath))
            {
                UpdateMetadata(filePath, date, metaUpdateTypes);
                Environment.Exit(0);
            }

            // Get directory to update
            string? folderPath = "";
            if (parameters.TryGetValue("dir", out folderPath) && Directory.Exists(folderPath))
            {
                foreach (var file in Directory.GetFiles(folderPath))
                {
                    UpdateMetadata(file, date, metaUpdateTypes);
                }
                Environment.Exit(0);
            }

            Console.WriteLine("Could not find file or directory. Please provide either --file=\"/folder/filename\" or --dir=\"/folder\" to update metadata");
            Environment.Exit(3);
        }
    }
}
