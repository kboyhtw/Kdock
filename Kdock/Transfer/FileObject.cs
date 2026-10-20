using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

namespace Kdock
{
    public class FileObject
    {
        public string ID { get; private set; }
        public string FileName { get; private set; }
        public string Directory { get; private set; }
        public long TotalLength { get; private set; }
        public string FullPath { get { return Path.Combine(Directory, FileName); } }

        public long ProcessedData { get; set; }
        public bool IsCompleted { get; set; }


        public FileObject(string id, string name, string directory, long length)
        {
            ID = id;
            FileName = name;
            Directory = directory;
            TotalLength = length;
        }

        public static FileObject FromInstruction(string instruction)
        {
            var parts = instruction.Split(':');
            return new FileObject(parts[1], parts[2], "YourDirectoryPath", long.Parse(parts[3]));
        }
    }
}
