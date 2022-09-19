using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Models
{
    internal class MyDriveInfo : DirInfo
    {
        public MyDriveInfo(DriveInfo drive)
            : base(drive.Name, null)
        {
            AdditionalData = $"Объем диска: {FormatFileSize(drive.TotalSize)} Свободное пространство: {FormatFileSize(drive.AvailableFreeSpace)}";
        }

        public static string FormatFileSize(long bytes)
        {
            var unit = 1024;
            if (bytes < unit) { return $"{bytes} B"; }

            var exp = (int)(Math.Log(bytes) / Math.Log(unit));
            return $"{bytes / Math.Pow(unit, exp):F2} {("KMGTPE")[exp - 1]}B";
        }
    }
}
