using Explorer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Explorer.ViewModels
{
    internal class MainViewModel : ViewModelBase
    {
        public ObservableCollection<DirInfo> TreeViewItems { get; set; }

        public ObservableCollection<DirInfo> AvailablePaths { get; set; }

        public DirInfo? ActiveDir { get; set; }

        public string? ErrorText { get; set; }

        public MainViewModel()
        {
            DriveInfo[] drivers = DriveInfo.GetDrives();
            TreeViewItems = new ObservableCollection<DirInfo>(drivers.Select(x => new DirInfo(x.Name, null)));
            AvailablePaths = new ObservableCollection<DirInfo>();
        }

        public RelayCommand WindowLoaded => new RelayCommand(async s =>
        {
            DirInfo firstDisk = TreeViewItems.First();
            firstDisk.IsSelected = firstDisk.IsExpanded = true;
            await SetNewActiveElement(firstDisk);
        });

        private async Task FillDirectory(DirInfo? dir)
        {
            if (dir == null ||
                !Directory.Exists(dir.Path))
            {
                return;
            }

            try
            {
                dir.ClearData();

                if (dir.ParentDir != null)
                {
                    dir.ParentDir.ShownName = "...";
                    dir.DirContent.Add(dir.ParentDir);
                }

                await Task.Factory.StartNew(() =>
                {
                    foreach (string dirPath in Directory.GetDirectories(dir.Path))
                    {
                        DirInfo di = new DirInfo(dirPath, dir);
                        dir.Dirs.Add(di);
                        dir.DirContent.Add(di);
                    }

                    foreach (string file in Directory.GetFiles(dir.Path))
                    {
                        FileData fileEx = new FileData(file, dir);
                        dir.Files.Add(fileEx);
                        dir.DirContent.Add(fileEx);
                    }
                });
            }
            catch(Exception ex)
            {
                SetError($"Не удалось открыть папку: {ex.Message}");
            }
        }

        public RelayCommand TreeViewSelected => new RelayCommand(async s =>
        {
            if (s is DirInfo dir)
            {
                dir.IsExpanded = true;
                await SetNewActiveElement(dir);
            }
        }, s => s != null);

        public RelayCommand TreeViewExpanded => new RelayCommand(async s =>
        {
            if (s is not DirInfo info || info.Dirs.Any(x => x != DirInfo.EmptyDir))
            {
                return;
            }

            await FillDirectory(info);
        });

        public RelayCommand ElementDoubleClick => new RelayCommand(async s =>
        {
            if (s is DirInfo dir)
            {
                await SetNewActiveElement(dir);
            }
            else if (s is FileData file)
            {
                try
                {
                    System.Diagnostics.Process.Start(file.Path);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка при открытии файла:\n\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        });

        private async Task SetNewActiveElement(DirInfo? dir)
        {
            SetError("");
            if (dir == null ||
                dir == ActiveDir)
            {
                return;
            }

            ActiveDir = dir;
            dir.IsSelected = true;
            dir.IsExpanded = true;
            await FillDirectory(ActiveDir);
        }

        private void SetError(string errorText)
        {
            // Timer??
            ErrorText = errorText;
        }
    }
}
