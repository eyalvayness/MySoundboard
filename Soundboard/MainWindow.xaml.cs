using Microsoft.Win32;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Newtonsoft.Json.Linq;
using Soundboard.Converters;
using Soundboard.Models;
using Soundboard.Properties;
using Soundboard.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
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
        [DllImport("user32.dll")] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        readonly SoundStorage Storage;

        private IntPtr _windowHandle;
        private HwndSource _source;

        public static int VirtualDeviceNumber { get; private set; }
        public static int HardwareDeviceNumber { get; private set; }
        
        public MainWindow()
        {
            InitializeComponent();

            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon
            {
                Icon = new System.Drawing.Icon(@"Assets\poop.ico"),
                Visible = true
            };
            ni.DoubleClick += (sender, a) =>
            {
                Show();
                WindowState = WindowState.Normal;
            };

            Storage = new SoundStorage();
            VirtualDeviceNumber = -1;
            HardwareDeviceNumber = -1;

            List<string> displayedDevicesNames = new List<string>();

            for (int i = 0; i < WaveOut.DeviceCount; i++)
                if (WaveOut.GetCapabilities(i).SupportsPlaybackRateControl)
                    displayedDevicesNames.Add(WaveOut.GetCapabilities(i).ProductName);


            VirtualOutputDevices.ItemsSource = displayedDevicesNames;
            VirtualOutputDevices.SelectedItem = Settings.Default.VirtualOutputDeviceName;

            HardwareOutputDevices.ItemsSource = displayedDevicesNames;
            HardwareOutputDevices.SelectedItem = Settings.Default.HardwareOutputDeviceName;

            SoundsDisplayer.DataContext = Storage.Sounds;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            var ss = Storage.Sounds.ToList();
            foreach (Sound s in ss)
            {
                if (s.VKey != 0)
                    RegisterHotKey(_windowHandle, s.Id, s.ModifsKey, s.VKey);
            }
            //bool a = RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_CONTROL, VK_CAPITAL); //CTRL + CAPS_LOCK
            //bool b = RegisterHotKey(_windowHandle, HOTKEY_ID + 1, MOD_SHIFT, VK_CAPITAL);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    int id = wParam.ToInt32();
                    
                    int vkey = ((int)lParam >> 16) & 0xFFFF;
                    int mkey = (int)lParam & 0xFF;
                    Key k = KeyInterop.KeyFromVirtualKey(vkey);
                    ModifierKeys mk = (ModifierKeys)mkey;

                    Sound sound = Storage.Sounds.Where(s => s.Id == id).FirstOrDefault();
                    sound?.Stop();
                    sound?.Play();
                    handled = true;
                    break;
            }
            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            Settings.Default.Save();
            _source.RemoveHook(HwndHook);

            Storage.Sounds.ToList().ForEach(s => UnregisterHotKey(_windowHandle, s.Id));
            
            //UnregisterHotKey(_windowHandle, HOTKEY_ID);
            base.OnClosed(e);
        }

        private void StopAllSoundsClick(object sender, RoutedEventArgs e) => Storage.StopAll();// allOutputsEvents.ForEach(ev => ev.Stop());

        private void ChangeVirtualOutputDevice(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            if (e.AddedItems.Count != 1)
                return;

            VirtualDeviceNumber = cb.SelectedIndex;
            Settings.Default.VirtualOutputDeviceName = cb.SelectedItem.ToString();
        }

        private void ChangeHardwareOutputDevice(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            if (e.AddedItems.Count != 1)
                return;

            HardwareDeviceNumber = cb.SelectedIndex;
            Settings.Default.HardwareOutputDeviceName = cb.SelectedItem.ToString();
        }

        private void AddNewClick(object sender, RoutedEventArgs e)
        {
            var d = new OpenFileDialog
            {
                Filter = $"Sound Files (*.mp3, *.wav)|*.mp3;*.wav",
                Title = "Pick the sound to add to your Sound Board",
                Multiselect = false
            };

            if (d.ShowDialog() == true)
            {
                //Sound s = new Sound(d.FileName, System.IO.Path.GetFileNameWithoutExtension(d.FileName));
                Storage.AddSound(d.FileName);
            }
        }

        private void DeleteAllClick(object sender, RoutedEventArgs e)
        {
            Storage.Sounds.ToList().ForEach(s => UnregisterHotKey(_windowHandle, s.Id));
            Storage.DeleteAll();
        }

        private void PreviewOrStopClick(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            Sound sound = b.DataContext as Sound;

            lock(sound)
            {
                switch (sound.CurrentState)
                {
                    case Sound.PlayingState.Playing:
                        break;
                    case Sound.PlayingState.Stopped:
                        sound.Preview();
                        break;
                    case Sound.PlayingState.Preview:
                        sound.Stop();
                        break;
                    default:
                        break;
                }
            }

        }

        private void RemoveClick(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b.Content.ToString() == Properties.Resources.Remove)
            {
                Sound s = b.DataContext as Sound;
                UnregisterHotKey(_windowHandle, s.Id);
                Storage.RemoveSound(s);
            }
        }

        private void SaveHotKeys(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            Sound s = b.DataContext as Sound;
            s.SaveShortcutKeys();

            UnregisterHotKey(_windowHandle, s.Id);
            RegisterHotKey(_windowHandle, s.Id, s.ModifsKey, s.VKey);
        }

        private void ResetShorcutClick(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            Sound s = b.DataContext as Sound;
            s.ClearHotKeys();
            s.SaveShortcutKeys();
            UnregisterHotKey(_windowHandle, s.Id);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.KeyDown += MainWindow_KeyDown;

            //keys = new List<Key>();
        }

        private void Tb_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            //keys.Remove(e.Key);
            //tb.Text = string.Join(" + ", keys.Select(k => k.ToString()));
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            Sound s = tb.DataContext as Sound;

            s.AddHotKey(e.Key);
            //if (!s.shortcutKeys.Contains(e.Key))
                //s.shortcutKeys.Add(e.Key); 
            //s.Shortcut = string.Join(" + ", s.shortcutKeys.Select(k => k.ToString()));
            //tb.Text = string.Join(" + ", s.shortcutKeys.Select(k => k.ToString()));
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.KeyDown -= MainWindow_KeyDown;

            Sound s = tb.DataContext as Sound;
            s.SaveShortcutKeys();
            UnregisterHotKey(_windowHandle, s.Id);
            RegisterHotKey(_windowHandle, s.Id, s.ModifsKey, s.VKey);
        }

        //private void DG_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) => Storage.SaveSounds();
    }
}
