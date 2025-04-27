using KZCheats.Core.Models;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace KZCheatsDev
{
    public partial class MainWindow : MetroWindow
    {
        private List<CheatEntryViewModel> CheatEntries { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();
            CheatsListBox.ItemsSource = CheatEntries;
        }

        private void AddCheatButton_Click(object sender, RoutedEventArgs e)
        {
            CheatEntries.Add(new CheatEntryViewModel());
            CheatsListBox.Items.Refresh();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
            {
                await this.ShowMessageAsync("Error", "Please fill all required fields!");
                return;
            }

            var kzcFile = new KzcFile
            {
                GameName = GameNameTextBox.Text,
                GameVersion = GameVersionTextBox.Text,
                ProcessName = ProcessNameTextBox.Text,
                Cheats = CheatEntries.Select(c => new CheatEntry
                {
                    Name = c.Name,
                    Address = c.Address,
                    PointerChain = string.IsNullOrWhiteSpace(c.PointerChainText) ? new List<string>() : c.PointerChainText.Split(',').Select(p => p.Trim()).ToList(),
                    Value = c.Value,
                    Type = c.Type,
                    Freeze = c.Freeze
                }).ToList()
            };

            var dialog = new SaveFileDialog
            {
                Filter = "KZC Files (*.kzc)|*.kzc",
                DefaultExt = ".kzc",
                FileName = $"{kzcFile.GameName}.kzc"
            };

            if (dialog.ShowDialog() == true)
            {
                var json = JsonSerializer.Serialize(kzcFile, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(dialog.FileName, json);
                await this.ShowMessageAsync("Success", "File saved successfully!");
            }
        }


        private bool ValidateForm()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(GameNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(GameVersionTextBox.Text) ||
                string.IsNullOrWhiteSpace(ProcessNameTextBox.Text))
            {
                isValid = false;
            }

            foreach (var cheat in CheatEntries)
            {
                cheat.HasNameError = string.IsNullOrWhiteSpace(cheat.Name);
                cheat.HasTypeError = string.IsNullOrWhiteSpace(cheat.Type);

                bool hasAddress = !string.IsNullOrWhiteSpace(cheat.Address);
                bool hasPointerChain = !string.IsNullOrWhiteSpace(cheat.PointerChainText);
                cheat.HasAddressOrPointerError = !(hasAddress || hasPointerChain);

                if (cheat.HasNameError || cheat.HasTypeError || cheat.HasAddressOrPointerError)
                {
                    isValid = false;
                }
            }

            CheatsListBox.Items.Refresh();
            return isValid;
        }

    }

    public class CheatEntryViewModel : CheatEntry
    {
        public string PointerChainText { get; set; }
        public bool HasNameError { get; set; }
        public bool HasTypeError { get; set; }
        public bool HasAddressOrPointerError { get; set; }
    }

}
