using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Models
{
    internal class DirInfo : FileSystemEntity
    {
        public ObservableCollection<DirInfo> Dirs { get; set; }

        public ObservableCollection<FileData> Files { get; set; }

        public ObservableCollection<FileSystemEntity> DirContent { get; set; }

        public bool IsSelected { get; set; }

        public bool IsExpanded { get; set; }

        public static DirInfo EmptyDir = new DirInfo(string.Empty, null);

        public DirInfo(string path, DirInfo? parentDir)
            : base(path, parentDir)
        {
            Dirs = new ObservableCollection<DirInfo>() { EmptyDir };
            Files = new ObservableCollection<FileData>();
            DirContent = new ObservableCollection<FileSystemEntity>();

            if (!string.IsNullOrEmpty(path))
            {
                DirectoryInfo di = new DirectoryInfo(path);
                AdditionalData = $"Время создания: {di.CreationTime} Корневой каталог: {di.Root}";
            }
        }

        public void ClearData()
        {
            Dirs.Clear();
            Files.Clear();
            DirContent.Clear();
        }
    }
}
