using KZCheats.Models;
using MahApps.Metro.Controls;
using System.Collections.ObjectModel;
using System.Windows;
using KZCheats.Core.Services;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using System.Windows.Threading;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Input;
using KZCheats.Core.Models;



namespace KZCheats.Views
{
    public partial class CheatWindow : MetroWindow
    {
        private ObservableCollection<CheatDisplayEntry> _cheatEntries = new();
        private Process _targetProcess;
        private KzcFileEntry _selectedEntry;
        private DispatcherTimer _freezeTimer;
        private DispatcherTimer _readTimer;





        public CheatWindow(KzcFileEntry selectedEntry)
        {
            InitializeComponent();
            _selectedEntry = selectedEntry;
            Console.WriteLine("Program açıldı");

            GameInfoText.Text = $"{selectedEntry.GameName} (v{selectedEntry.GameVersion})";

            _freezeTimer = new DispatcherTimer();
            _freezeTimer.Interval = TimeSpan.FromMilliseconds(100);
            _freezeTimer.Tick += FreezeTimer_Tick;
            _freezeTimer.Start();

            _readTimer = new DispatcherTimer();
            _readTimer.Interval = TimeSpan.FromSeconds(1);
            _readTimer.Tick += ReadTimer_Tick;
            _readTimer.Start();



            try
            {
                _targetProcess = Process.GetProcessesByName(
                    Path.GetFileNameWithoutExtension(selectedEntry.KzcContent.ProcessName))
                    .FirstOrDefault();

                if (_targetProcess == null)
                {
                    MessageBox.Show("Game process not found. Please make sure the game is running.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error finding game process: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }

            foreach (var cheat in selectedEntry.KzcContent.Cheats)
            {
                _cheatEntries.Add(new CheatDisplayEntry
                {
                    Name = cheat.Name,
                    Value = cheat.Value?.ToString() ?? "",
                });
            }

            CheatsDataGrid.ItemsSource = _cheatEntries;
        }

        private void CheatsDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var editedCheat = e.Row.Item as CheatDisplayEntry;
                if (editedCheat == null)
                    return;

                var cheat = _selectedEntry.KzcContent.Cheats.FirstOrDefault(c => c.Name == editedCheat.Name);
                if (cheat == null || string.IsNullOrWhiteSpace(cheat.Address))
                    return;

                try
                {
                    IntPtr address = MemoryHelper.ParseAddress(cheat.Address);
                    byte[] bytesToWrite = PrepareBytes(editedCheat.Value, cheat.Type);
                    MemoryHelper.WriteMemory(_targetProcess, address, bytesToWrite);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to update memory value: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void FreezeTimer_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("FreezeTimer Tick çalıştı...");

            foreach (var cheatDisplay in _cheatEntries)
            {
                if (!cheatDisplay.Freeze)
                    continue;

                var cheat = _selectedEntry.KzcContent.Cheats.FirstOrDefault(c => c.Name == cheatDisplay.Name);
                if (cheat == null)
                    continue;

                IntPtr finalAddress;

                if (!string.IsNullOrWhiteSpace(cheat.Address))
                {
                    finalAddress = MemoryHelper.ParseAddress(cheat.Address);
                }
                else if (cheat.PointerChain != null && cheat.PointerChain.Count > 0)
                {
                    IntPtr baseAddress = MemoryHelper.ParseAddress(cheat.PointerChain[0]);
                    List<int> offsets = cheat.PointerChain.Skip(1).Select(x => Convert.ToInt32(x, 16)).ToList();
                    finalAddress = MemoryHelper.FollowPointerChain(_targetProcess, baseAddress, offsets);
                    System.Diagnostics.Debug.WriteLine($"Writing to address: {finalAddress.ToString("X")}");

                }
                else
                {
                    continue;
                }

                byte[] bytesToWrite = PrepareBytes(cheatDisplay.Value, cheat.Type);
                MemoryHelper.WriteMemory(_targetProcess, finalAddress, bytesToWrite);
            }
        }

        private void ReadTimer_Tick(object sender, EventArgs e)
        {
            if (_targetProcess == null || _targetProcess.HasExited)
                return;

            var editingItem = CheatsDataGrid.CurrentItem;

            foreach (var cheatDisplay in _cheatEntries)
            {
                if (cheatDisplay == editingItem)
                    continue;

                var cheat = _selectedEntry.KzcContent.Cheats.FirstOrDefault(c => c.Name == cheatDisplay.Name);
                if (cheat == null)
                    continue;

                try
                {
                    IntPtr finalAddress;

                    if (!string.IsNullOrWhiteSpace(cheat.Address))
                    {
                        finalAddress = MemoryHelper.ParseAddress(cheat.Address);
                    }
                    else if (cheat.PointerChain != null && cheat.PointerChain.Count > 0)
                    {
                        IntPtr baseAddress = MemoryHelper.ParseAddress(cheat.PointerChain[0]);
                        List<int> offsets = cheat.PointerChain.Skip(1).Select(x => MemoryHelper.ParseOffset(x)).ToList();
                        finalAddress = MemoryHelper.FollowPointerChain(_targetProcess, baseAddress, offsets);
                    }
                    else
                    {
                        continue;
                    }

                    int size = cheat.Type switch
                    {
                        "int" => 4,
                        "float" => 4,
                        "double" => 8,
                        _ => 4
                    };

                    byte[] valueBytes = MemoryHelper.ReadMemory(_targetProcess, finalAddress, size);

                    string newValueStr = cheat.Type switch
                    {
                        "int" => BitConverter.ToInt32(valueBytes, 0).ToString(),
                        "float" => BitConverter.ToSingle(valueBytes, 0).ToString(),
                        "double" => BitConverter.ToDouble(valueBytes, 0).ToString(),
                        _ => ""
                    };

                    cheatDisplay.Value = newValueStr;
                }
                catch
                {
                    //TODO: Handle the case where the memory read fails
                }
            }

            if (CheatsDataGrid.CommitEdit(DataGridEditingUnit.Row, true))
            {
                CheatsDataGrid.Items.Refresh();
            }
        }


        private void ActivateAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var cheatDisplay in _cheatEntries)
            {
                var cheat = _selectedEntry.KzcContent.Cheats.FirstOrDefault(c => c.Name == cheatDisplay.Name);
                if (cheat == null)
                    continue;

                IntPtr finalAddress;

                if (!string.IsNullOrWhiteSpace(cheat.Address))
                {
                    finalAddress = MemoryHelper.ParseAddress(cheat.Address);
                }
                else if (cheat.PointerChain != null && cheat.PointerChain.Count > 0)
                {
                    IntPtr baseAddress = MemoryHelper.ParseAddress(cheat.PointerChain[0]);
                    List<int> offsets = cheat.PointerChain.Skip(1).Select(x => Convert.ToInt32(x, 16)).ToList();
                    finalAddress = MemoryHelper.FollowPointerChain(_targetProcess, baseAddress, offsets);
                }
                else
                {
                    continue;
                }

                byte[] bytesToWrite = PrepareBytes(cheatDisplay.Value, cheat.Type);
                MemoryHelper.WriteMemory(_targetProcess, finalAddress, bytesToWrite);
            }

            CheatsDataGrid.Items.Refresh();
        }

        private async void CheatsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (CheatsDataGrid.SelectedItem is CheatDisplayEntry selectedCheatDisplay)
            {
                var cheat = _selectedEntry.KzcContent.Cheats.FirstOrDefault(c => c.Name == selectedCheatDisplay.Name);
                if (cheat != null)
                {
                    var dialog = new EditCheatDialog(selectedCheatDisplay.Value, selectedCheatDisplay.Freeze, cheat.Type);
                    await this.ShowMetroDialogAsync(dialog);

                    await dialog.WaitForCloseAsync();

                    if (!string.IsNullOrWhiteSpace(dialog.NewValue))
                    {
                        selectedCheatDisplay.Value = dialog.NewValue;
                        selectedCheatDisplay.Freeze = dialog.NewFreeze;
                        CheatsDataGrid.Items.Refresh();

                        cheat.Value = dialog.NewValue;

                        try
                        {
                            IntPtr finalAddress;

                            if (!string.IsNullOrWhiteSpace(cheat.Address))
                            {
                                finalAddress = MemoryHelper.ParseAddress(cheat.Address);
                            }
                            else if (cheat.PointerChain != null && cheat.PointerChain.Count > 0)
                            {
                                IntPtr baseAddress = MemoryHelper.ParseAddress(cheat.PointerChain[0]);
                                List<int> offsets = cheat.PointerChain.Skip(1).Select(x => MemoryHelper.ParseOffset(x)).ToList();
                                finalAddress = MemoryHelper.FollowPointerChain(_targetProcess, baseAddress, offsets);
                            }
                            else
                            {
                                return;
                            }

                            byte[] bytesToWrite = PrepareBytes(selectedCheatDisplay.Value, cheat.Type);
                            MemoryHelper.WriteMemory(_targetProcess, finalAddress, bytesToWrite);
                        }
                        catch
                        {
                        }
                    }

                    await this.HideMetroDialogAsync(dialog);
                }

            }
        }



        private byte[] PrepareBytes(object value, string type)
        {
            if (value is System.Text.Json.JsonElement jsonElement)
            {
                switch (type.ToLower())
                {
                    case "int":
                        return BitConverter.GetBytes(jsonElement.GetInt32());
                    case "float":
                        return BitConverter.GetBytes(jsonElement.GetSingle());
                    case "double":
                        return BitConverter.GetBytes(jsonElement.GetDouble());
                    case "byte":
                        return new byte[] { jsonElement.GetByte() };
                    case "string":
                        return System.Text.Encoding.UTF8.GetBytes(jsonElement.GetString());
                    default:
                        throw new Exception("Unsupported type.");
                }
            }
            else
            {
                switch (type.ToLower())
                {
                    case "int":
                        return BitConverter.GetBytes(Convert.ToInt32(value));
                    case "float":
                        return BitConverter.GetBytes(Convert.ToSingle(value));
                    case "double":
                        return BitConverter.GetBytes(Convert.ToDouble(value));
                    case "byte":
                        return new byte[] { Convert.ToByte(value) };
                    case "string":
                        return System.Text.Encoding.UTF8.GetBytes(Convert.ToString(value));
                    default:
                        throw new Exception("Unsupported type.");
                }
            }
        }




        private void DeactivateAll_Click(object sender, RoutedEventArgs e)
        {
            CheatsDataGrid.Items.Refresh();
        }
    }

}
