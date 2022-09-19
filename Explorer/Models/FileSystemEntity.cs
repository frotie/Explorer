using Explorer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Models
{
    internal abstract class FileSystemEntity : ViewModelBase
    {
        public string Path { get; set; }

        public string Name { get; set; }

        public string ShownName { get; set; }

        public DirInfo? ParentDir { get; set; }

        public string? AdditionalData { get; set; }

        public FileSystemEntity(string path, DirInfo? parentDir)
        {
            Path = path;
            Name = System.IO.Path.GetFileName(Path);
            ShownName = Name;
            ParentDir = parentDir;

            if (string.IsNullOrEmpty(Name))
            {
                Name = Path;
            }
        }
    }
}
