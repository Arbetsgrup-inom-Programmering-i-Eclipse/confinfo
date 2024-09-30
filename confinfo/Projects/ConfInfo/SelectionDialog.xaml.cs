using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ConfInfo
{
    /// <summary>
    /// Interaction logic for SelectionDialog.xaml
    /// </summary>
    public partial class SelectionDialog : Window
    {
        public SelectionDialog(List<string> itemNames, string displayedInstruction = "Select")
        {
            InitializeComponent();

            Title = displayedInstruction;

            listDisplay.ItemsSource = itemNames;
            listDisplay.SelectedIndex = 0;
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            listDisplay.Focus();
        }
        
    }
}
