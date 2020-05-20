using Microsoft.Win32;
using Soundboard.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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

namespace Soundboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<Sound> filenames;

        public MainWindow()
        {
            InitializeComponent();
            filenames = new ObservableCollection<Sound>();

            DG.DataContext = filenames;
        }

        private void AddNewClick(object sender, RoutedEventArgs e)
        {
            var d = new OpenFileDialog
            {
                Filter = "mp3 files (*.mp3)|*.mp3|All files (*.*)|*.*",
                Title = "Pick the sound to add to the sound board",
                Multiselect = false
            };

            if (d.ShowDialog() == true)
            {
                if (!d.FileName.EndsWith(".mp3"))
                    return;

                Sound s = new Sound(d.FileName, d.SafeFileName);
                filenames.Add(s);
            }
        }
    }
}
