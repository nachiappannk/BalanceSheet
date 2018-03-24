using System;
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

namespace Nachiappan.BalanceSheet
{
    /// <summary>
    /// Interaction logic for OptionsWorkFlowStepUserControl.xaml
    /// </summary>
    public partial class AlteringAccountsRelationWorkFlowStepUserControl : UserControl
    {
        public AlteringAccountsRelationWorkFlowStepUserControl()
        {
            InitializeComponent();
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid.UnselectAllCells();
        }
    }
}
