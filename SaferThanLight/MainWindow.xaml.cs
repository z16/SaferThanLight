using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SaferThanLight {
    public partial class MainWindow {
        public ViewModel ViewModel
            => (ViewModel) DataContext;

        public MainWindow()
            => InitializeComponent();

        private async void SaveButton_Click(Object sender, RoutedEventArgs e)
            => await ViewModel.Save();

        private async void OverwriteButton_Click(Object sender, RoutedEventArgs e)
            => await ViewModel.Overwrite((SaveEntry) FileGrid.SelectedItem);

        private void LoadButton_Click(Object sender, RoutedEventArgs e)
            => ViewModel.Load((SaveEntry) FileGrid.SelectedItem);

        private void DeleteButton_Click(Object sender, RoutedEventArgs e)
            => ViewModel.Delete(FileGrid.SelectedItems.Cast<SaveEntry>());

        private void FileGrid_SelectedCellsChanged(Object sender, SelectedCellsChangedEventArgs e)
            => ViewModel.SelectionCount = ((DataGrid) sender).SelectedItems.Count;

        private async void Window_Loaded(Object sender, RoutedEventArgs e)
            => await ViewModel.Initialize();

        private void Grid_MouseDown(Object sender, System.Windows.Input.MouseButtonEventArgs e)
            => FileGrid.UnselectAll();
    }
}
