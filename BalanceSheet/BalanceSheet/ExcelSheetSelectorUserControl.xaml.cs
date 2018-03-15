using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace Nachiappan.BalanceSheet
{
    /// <summary>
    /// Interaction logic for ExcelSheetSelectorUserControl.xaml
    /// </summary>
    public partial class ExcelSheetSelectorUserControl : UserControl
    {
        public ExcelSheetSelectorUserControl()
        {
            InitializeComponent();
        }


        private void OnPickerButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = ".xlsx";
            dialog.Filter = "Excel (.xlsx)|*.xlsx";
            var done = dialog.ShowDialog();
            if (done == true)
            {
                FilePicker.Clear();
                FilePicker.AppendText(dialog.FileName);
                FilePicker.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            }
        }
    }
}
