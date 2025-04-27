using KZCheats.Core.Services;
using KZCheats.Models;
using KZCheats.Views;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KZCheats;

public partial class MainWindow : MetroWindow
{
    private ObservableCollection<KzcFileEntry> _fileEntries = new();

    public MainWindow()
    {
        InitializeComponent();

        ConfigService.CleanupMissingFiles();
        var recentFiles = ConfigService.GetRecentFiles();

        foreach (var filePath in recentFiles)
        {
            try
            {
                var kzc = KzcService.LoadKzcFile(filePath);

                var entry = new KzcFileEntry
                {
                    GameName = kzc.GameName,
                    GameVersion = kzc.GameVersion,
                    FilePath = filePath,
                    KzcContent = kzc
                };

                _fileEntries.Add(entry);
            }
            catch (Exception)
            {
                //TODO: Handle the case where the file is not valid or missing
            }
        }

        FilesDataGrid.ItemsSource = _fileEntries;
    }



    private void SelectNewFile_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "KZC Files (*.kzc)|*.kzc",
            Title = "Select a KZC File"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                var kzc = KzcService.LoadKzcFile(openFileDialog.FileName);

                var entry = new KzcFileEntry
                {
                    GameName = kzc.GameName,
                    GameVersion = kzc.GameVersion,
                    FilePath = openFileDialog.FileName,
                    KzcContent = kzc
                };

                _fileEntries.Add(entry);
                ConfigService.AddRecentFile(openFileDialog.FileName);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load KZC file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void FilesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (FilesDataGrid.SelectedItem is KzcFileEntry selectedEntry)
        {
            var cheatWindow = new CheatWindow(selectedEntry);
            cheatWindow.Show();
            this.Close();
        }
    }


    private void Continue_Click(object sender, RoutedEventArgs e)
    {
        var selectedEntry = FilesDataGrid.SelectedItem as KzcFileEntry;
        if (selectedEntry == null)
        {
            MessageBox.Show("Please select a KZC file first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var cheatWindow = new CheatWindow(selectedEntry);
        cheatWindow.Show();
        this.Close();
    }

}