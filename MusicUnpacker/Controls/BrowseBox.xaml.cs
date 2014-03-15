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

namespace MusicUnpacker.Controls
{
    /// <summary>
    /// Interaction logic for BrowseBox.xaml
    /// </summary>
    public partial class BrowseBox : UserControl
    {
        public static readonly DependencyProperty FilepathProperty =
       DependencyProperty.Register(
           "Filepath",
           typeof(string),
           typeof(BrowseBox),
           new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        public BrowseBox()
        {
            InitializeComponent();
        }

        public string Filepath
        {
            get { return (string)GetValue(FilepathProperty); }
            set { SetValue(FilepathProperty, value); }
        }

        /// <summary>
        /// Shamelessly stolen from http://stackoverflow.com/a/10315283/330830
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".zip";
            dlg.Filter = "ZIP Files (*.zip)|*.zip";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                Filepath = filename;
            }
        }
    }


}
