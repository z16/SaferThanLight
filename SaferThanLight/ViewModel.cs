using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using z16;
using z16.Wpf;

namespace SaferThanLight {
    public class ViewModel : INotifyPropertyChanged {
        public ObservableContentCollection<SaveEntry> SaveFiles { get; } = new ObservableContentCollection<SaveEntry>();

        public Int32? SelectedIndex {
            get => SelectedIndexField;
            set {
                SelectedIndexField = value;
                NotifyPropertyChanged();
            }
        }

        public Int32 SelectionCount {
            get => SelectionCountField;
            set {
                var previous = SelectionCountField;
                SelectionCountField = value;
                NotifyPropertyChanged();

                if ((previous == 0) != (value == 0)) {
                    NotifyPropertyChanged(nameof(AnySelected));
                }

                if ((previous == 1) != (value == 1)) {
                    NotifyPropertyChanged(nameof(SingleSelected));
                }
            }
        }

        public Boolean AnySelected => SelectionCount != 0;
        public Boolean SingleSelected => SelectionCount == 1;
        public Boolean SaveFileExists {
            get => SaveFileExistsField;
            set {
                SaveFileExistsField = value;
                NotifyPropertyChanged();
            }
        }

        public async Task Initialize() {
            SaveFiles.AddRange(await GetMetaEntries());

            SaveFiles.CollectionChanged += OnChange;
            SaveFiles.CollectionContentChanged += OnChange;

            NotifyPropertyChanged(nameof(SaveFiles));

            Fsw.Created += (sender, e) => SaveFileExists = true;
            Fsw.Deleted += (sender, e) => SaveFileExists = false;
            Fsw.Renamed += (sender, e) => SaveFileExists = e.Name == "continue.sav";
            Fsw.EnableRaisingEvents = true;

            async void OnChange<T>(Object sender, T e) {
                await using var stream = File.Open(Data.MetaFile, FileMode.Create, FileAccess.Write);
                await JsonSerializer.SerializeAsync(stream, SaveFiles);
            }
        }

        public async Task Save(SaveEntry? old = null) {
            if (!File.Exists(Data.SaveFile)) {
                return;
            }

            var entry = await SaveEntry.Create();
            if (old != null) {
                entry.Description = old.Description;
            }

            if (File.Exists(entry.Filepath)) {
                return;
            }

            File.Copy(Data.SaveFile, entry.Filepath);
            SaveFiles.Add(entry);
            SelectedIndex = SaveFiles.IndexOf(entry);
        }

        public async Task Overwrite(SaveEntry entry) {
            await Save(entry);

            Delete(entry);
        }

        public void Load(SaveEntry entry) {
            var backupName = Data.SaveFile + ".bak";
            if (File.Exists(backupName)) {
                File.Delete(backupName);
            }

            if (File.Exists(Data.SaveFile)) {
                File.Move(Data.SaveFile, backupName);
            }

            File.Copy(Path.Combine(Data.SaveDirectory, entry.Filename), Data.SaveFile);
        }

        public void Delete(SaveEntry entry)
            => Delete(entry.Yield());

        public void Delete(IEnumerable<SaveEntry> entries) {
            foreach (var entry in entries) {
                try {
                    File.Delete(entry.Filepath);
                } catch (FileNotFoundException) { }
            }

            SaveFiles.RemoveRange(entries.Where(SaveFiles.Contains));
        }

        private async Task<IEnumerable<SaveEntry>> GetMetaEntries() {
            if (!File.Exists(Data.MetaFile)) {
                return Enumerable.Empty<SaveEntry>();
            }

            await using var stream = File.OpenRead(Data.MetaFile);
            return await JsonSerializer.DeserializeAsync<IEnumerable<SaveEntry>>(stream);
        }

        private Int32? SelectedIndexField;
        private Int32 SelectionCountField = 0;
        private Boolean SaveFileExistsField = File.Exists(Data.SaveFile);

        private FileSystemWatcher Fsw = new FileSystemWatcher(Data.SaveDirectory, "continue.sav");

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] String? propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
