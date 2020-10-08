using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using z16;
using z16.Wpf;

namespace SaferThanLight {
    public partial class MainWindow : INotifyPropertyChanged {
        public MainWindow() {
            InitializeComponent();
        }

        public ObservableContentCollection<SaveEntry> SaveFiles { get; } = new ObservableContentCollection<SaveEntry>();

        private async Task Save(SaveEntry? old = null) {
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
            FileGrid.SelectedIndex = SaveFiles.IndexOf(entry);
        }

        private void Load(SaveEntry entry) {
            var backupName = Data.SaveFile + ".bak";
            if (File.Exists(backupName)) {
                File.Delete(backupName);
            }

            if (File.Exists(Data.SaveFile)) {
                File.Move(Data.SaveFile, backupName);
            }

            File.Copy(Path.Combine(Data.SaveDirectory, entry.Filename), Data.SaveFile);
        }

        private void Delete(SaveEntry entry)
            => Delete(entry.Yield());

        private void Delete(IEnumerable<SaveEntry> entries) {
            foreach (var entry in entries) {
                try {
                    File.Delete(entry.Filepath);
                } catch (FileNotFoundException) { }
            }

            SaveFiles.RemoveRange(entries.Where(SaveFiles.Contains));
        }

        private async void SaveButton_Click(Object sender, RoutedEventArgs e) {
            await Save();
        }

        private async void OverwriteButton_Click(Object sender, RoutedEventArgs e) {
            if (FileGrid.SelectedItems.Count != 1) {
                return;
            }

            var entry = (SaveEntry) FileGrid.SelectedItem;

            await Save(entry);

            Delete(entry);
        }

        private void LoadButton_Click(Object sender, RoutedEventArgs e) {
            if (FileGrid.SelectedItems.Count != 1) {
                return;
            }

            Load((SaveEntry) FileGrid.SelectedItem);
        }

        private void DeleteButton_Click(Object sender, RoutedEventArgs e)
            => Delete(FileGrid.SelectedItems.Cast<SaveEntry>());

        private async void Window_Loaded(Object sender, RoutedEventArgs e) {
            if (File.Exists(Data.MetaFile)) {
                await using var stream = File.OpenRead(Data.MetaFile);
                SaveFiles.AddRange(await JsonSerializer.DeserializeAsync<IEnumerable<SaveEntry>>(stream));
            }

            SaveFiles.CollectionChanged += OnChange;
            SaveFiles.CollectionContentChanged += OnChange;
            NotifyPropertyChanged("SaveFiles");

            async void OnChange<T>(Object sender, T e) {
                await using var stream = File.Open(Data.MetaFile, FileMode.Create, FileAccess.Write);
                await JsonSerializer.SerializeAsync(stream, SaveFiles);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] String? propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
