using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Models
{
    internal class FileData : FileSystemEntity
    {
        public FileData(string path, DirInfo? parentDir)
            : base(path, parentDir)
        {
        }
    }
}
