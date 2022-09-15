using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Models
{
    internal class DirInfo : FileSystemEntity
    {
        public List<DirInfo> Dirs { get; set; }

        public List<FileData> Files { get; set; }

        public List<FileSystemEntity> DirContent { get; set; }

        public bool IsSelected { get; set; }

        public bool IsExpanded { get; set; }

        public static DirInfo EmptyDir = new DirInfo(string.Empty, null);

        public DirInfo(string path, DirInfo? parentDir)
            : base(path, parentDir)
        {
            Dirs = new List<DirInfo>() { EmptyDir };
            Files = new List<FileData>();
            DirContent = new List<FileSystemEntity>();
        }

        public void ClearData()
        {
            Dirs.Clear();
            Files.Clear();
            DirContent.Clear();
        }
    }
}
