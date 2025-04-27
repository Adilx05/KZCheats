using MahApps.Metro.Controls.Dialogs;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace KZCheats.Views
{
    public partial class EditCheatDialog : BaseMetroDialog
    {
        public string NewValue { get; private set; }
        public bool NewFreeze { get; private set; }

        private TaskCompletionSource<bool> _closeDialogTcs = new();

        public string CheatType { get; }

        public EditCheatDialog(string initialValue, bool initialFreeze, string cheatType)
        {
            InitializeComponent();
            ValueTextBox.Text = initialValue;
            FreezeCheckBox.IsChecked = initialFreeze;
            CheatType = cheatType;
        }


        public Task WaitForCloseAsync()
        {
            return _closeDialogTcs.Task;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ValueTextBox.Text))
            {
                MessageBox.Show("Value cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CheatType == "int" && !int.TryParse(ValueTextBox.Text, out _))
            {
                MessageBox.Show("Please enter a valid integer value.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if ((CheatType == "float" || CheatType == "double") && !double.TryParse(ValueTextBox.Text, out _))
            {
                MessageBox.Show("Please enter a valid number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NewValue = ValueTextBox.Text;
            NewFreeze = FreezeCheckBox.IsChecked ?? false;
            _closeDialogTcs.TrySetResult(true);
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _closeDialogTcs.TrySetResult(false);
        }
    }
}
