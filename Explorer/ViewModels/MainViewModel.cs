using Explorer.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

        private FileSystemEntity? selectedEntity;

        public FileSystemEntity? SelectedEntity
        {
            get => selectedEntity;
            set
            {
                if (value == selectedEntity)
                {
                    return;
                }

                selectedEntity = value;
                StatusText = selectedEntity?.AdditionalData;
                OnPropertyChanged();
            }
        }

        public string? StatusText { get; set; }

        public Dictionary<FileData, DateTime> LastOpenedFiles { get; set; }

        private DirInfo? lastParent;

        public MainViewModel()
        {
            DriveInfo[] drivers = DriveInfo.GetDrives();
            TreeViewItems = new ObservableCollection<DirInfo>(drivers.Select(x => new MyDriveInfo(x)));
            AvailablePaths = new ObservableCollection<DirInfo>();
            LastOpenedFiles = new Dictionary<FileData, DateTime>();
        }

        public RelayCommand WindowLoaded => new RelayCommand(s =>
        {
            DirInfo firstDisk = TreeViewItems.First();
            firstDisk.IsSelected = firstDisk.IsExpanded = true;
            SetNewActiveElement(firstDisk);
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

                List<DirInfo> dirs = new List<DirInfo>();
                List<FileData> files = new List<FileData>();

                await Task.Factory.StartNew(() =>
                {
                    foreach (string dirPath in Directory.GetDirectories(dir.Path))
                    {
                        DirInfo di = new DirInfo(dirPath, dir);
                        dirs.Add(di);
                    }

                    foreach (string file in Directory.GetFiles(dir.Path))
                    {
                        FileData fileEx = new FileData(file, dir);
                        files.Add(fileEx);
                    }
                });

                foreach (DirInfo di in dirs)
                {
                    dir.Dirs.Add(di);
                    dir.DirContent.Add(di);
                }

                foreach (FileData fd in files)
                {
                    dir.Files.Add(fd);
                    dir.DirContent.Add(fd);
                }
            }
            catch (Exception ex)
            {
                StatusText = $"Не удалось открыть папку: {ex.Message}";
            }
        }

        public RelayCommand TreeViewSelected => new RelayCommand(s =>
        {
            if (s is DirInfo dir)
            {
                dir.IsExpanded = true;
                SetNewActiveElement(dir);
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

        public RelayCommand ElementDoubleClick => new RelayCommand(s =>
        {
            if (s is DirInfo dir)
            {
                SetNewActiveElement(dir);
            }
            else if (s is FileData file)
            {
                try
                {
                    AddInHistory(file);
                    Process process = new Process
                    {
                        StartInfo = new ProcessStartInfo(file.Path)
                        {
                            UseShellExecute = true,
                        }
                    };

                    process.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка при открытии файла:\n\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        });

        public RelayCommand SaveHistory => new RelayCommand(async s =>
        {
            SaveFileDialog sfd = new SaveFileDialog()
            {
                Filter = "Текстовый файл|*.txt"
            };

            if (sfd.ShowDialog() == true)
            {
                string fileName = sfd.FileName;
                string result = string.Empty;
                IEnumerable<KeyValuePair<FileData, DateTime>> files = LastTenSecOpenedFiles;
                foreach (KeyValuePair<FileData, DateTime> fileData in files)
                {
                    result += fileData.Key.Path + "\t" + fileData.Value.ToLongTimeString() + "\n";
                }

                try
                {
                    File.WriteAllText(fileName, result);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось сохранить файл с историей:\n\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                await FillDirectory(ActiveDir);
                UpdateParent(ActiveDir);
            }
        });

        private void SetNewActiveElement(DirInfo? dir)
        {
            if (dir == null ||
                ActiveDir == dir)
            {
                return;
            }

            ActiveDir = dir;
            dir.IsSelected = true;
            dir.IsExpanded = true;
            StatusText = dir.AdditionalData;

            UpdateParent(dir);
        }

        private void UpdateParent(DirInfo? dir)
        {
            if (dir == null)
            {
                return;
            }

            if (lastParent != null)
            {
                lastParent.ShownName = lastParent.Name;
            }

            if (dir.ParentDir != null)
            {
                dir.ParentDir.ShownName = "...";
                if (!dir.DirContent.Contains(dir.ParentDir))
                {
                    dir.DirContent.Insert(0, dir.ParentDir);
                }

                lastParent = dir.ParentDir;
            }
        }

        private IEnumerable<KeyValuePair<FileData, DateTime>> LastTenSecOpenedFiles => LastOpenedFiles
                .Where(x => DateTime.Now - x.Value < TimeSpan.FromSeconds(10));

        private void AddInHistory(FileData file)
        {
            if (LastOpenedFiles.ContainsKey(file))
            {
                LastOpenedFiles[file] = DateTime.Now;
            }
            else
            {
                LastOpenedFiles.Add(file, DateTime.Now);
            }

            IEnumerable<FileData> dirsToRemove = LastOpenedFiles.Where(x => !LastTenSecOpenedFiles.Contains(x)).Select(x => x.Key);
            foreach (FileData fileToRemove in dirsToRemove)
            {
                LastOpenedFiles.Remove(fileToRemove);
            }
        }
    }
}
