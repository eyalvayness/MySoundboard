using Microsoft.Win32;
using NAudio.Wave;
using Newtonsoft.Json.Linq;
using Soundboard.Models;
using Soundboard.Properties;
using Soundboard.Storage;
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
        private static readonly string defaultExt = "mp3";

        readonly SoundStorage Storage;

        WaveOutEvent outputDevice;

        public MainWindow()
        {
            InitializeComponent();
            //allSounds = new ObservableCollection<Sound>(Storage.SoundStorage.LoadSounds());
            Storage = new SoundStorage();

            SoundsDisplayer.DataContext = Storage.Sounds;
            outputDevice = new WaveOutEvent();
        }
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => 

        private void AddNewClick(object sender, RoutedEventArgs e)
        {
            var d = new OpenFileDialog
            {
                Filter = $"{defaultExt} files (*.{defaultExt})|*.{defaultExt}",
                Title = "Pick the sound to add to the sound board",
                Multiselect = false
            };

            if (d.ShowDialog() == true)
            {
                //Sound s = new Sound(d.FileName, System.IO.Path.GetFileNameWithoutExtension(d.FileName));
                Storage.AddSound(d.FileName);
                Storage.SaveSounds();
            }
        }

        private void DeleteAllClick(object sender, RoutedEventArgs e) => Storage.DeleteAll();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Sound s = ((sender as Button).DataContext as Sound);
            var file = s.AudioFile;
            outputDevice.Stop();

            outputDevice.Init(file);
            outputDevice.Play();
        }


        //private void DG_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) => Storage.SaveSounds();
    }
}
