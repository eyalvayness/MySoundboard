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
        readonly List<string> devicesNames;
        readonly SoundStorage Storage;

        WaveOutEvent virtualOutputDevice;
        WaveOutEvent hardwareOutputDevice;

        public MainWindow()
        {
            InitializeComponent();
            Storage = new SoundStorage();
            virtualOutputDevice = new WaveOutEvent();
            hardwareOutputDevice = new WaveOutEvent();

            devicesNames = new List<string>();
            for (int i = 0; i < WaveOut.DeviceCount; i++)
                if (WaveOut.GetCapabilities(i).SupportsPlaybackRateControl)
                    devicesNames.Add(WaveOut.GetCapabilities(i).ProductName);
            VirtualOutputDevices.ItemsSource = devicesNames;
            VirtualOutputDevices.SelectedIndex = 0;

            HardwareOutputDevices.ItemsSource = devicesNames;
            HardwareOutputDevices.SelectedIndex = 0;

            SoundsDisplayer.DataContext = Storage.Sounds;
            VolumeSlider.Value = Settings.Default.Volume;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Settings.Default.Save();
        }

        private void ChangeVirtualOutputDevice(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            if (e.AddedItems.Count != 1)
                return;
            //Settings.Default.OutputDeviceName = OutputDevices.SelectedItem.ToString();
            if (virtualOutputDevice != null)
            {
                virtualOutputDevice.Dispose();
                virtualOutputDevice = null;
            }
            virtualOutputDevice = new WaveOutEvent() { DeviceNumber = cb.SelectedIndex };
        }

        private void ChangeHardwareOutputDevice(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            if (e.AddedItems.Count != 1)
                return;
            //Settings.Default.OutputDeviceName = OutputDevices.SelectedItem.ToString();
            if (hardwareOutputDevice != null)
            {
                hardwareOutputDevice.Dispose();
                hardwareOutputDevice = null;
            }
            hardwareOutputDevice = new WaveOutEvent() { DeviceNumber = cb.SelectedIndex };
        }

        private void ChangeVolume(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            virtualOutputDevice.Volume = (float)e.NewValue / 100f;
            hardwareOutputDevice.Volume = (float)e.NewValue / 100f;
            Settings.Default.Volume = (float)e.NewValue;
        }

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
            virtualOutputDevice.Stop();
            hardwareOutputDevice.Stop();

            virtualOutputDevice.Init(s.AudioFile);
            hardwareOutputDevice.Init(s.AudioFile);
            virtualOutputDevice.Play();
            hardwareOutputDevice.Play();
        }
        //private void DG_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) => Storage.SaveSounds();
    }
}
