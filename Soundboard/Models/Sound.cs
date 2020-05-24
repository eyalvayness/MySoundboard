using NAudio.Wave;
using Newtonsoft.Json;
using Soundboard.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Soundboard.Models
{
    public class Sound : BaseNotifier
    {
        string _name, _path, _shortcut;
        float _volume;
        PlayingState _currentState;

        [JsonIgnore] public PlayingState CurrentState { get => _currentState; set => SetProperty(ref _currentState, value); }
        [JsonIgnore] public AudioFileReader AudioFile => new AudioFileReader(Path);
        AudioFileReader virtualAudioFile, hardwareAudioFile;
        WaveOutEvent virtualDevice, hardwareDevice;


        Key tempShortcutKey, _shortcutKey;
        ModifierKeys tempShortcutModifierKeys, _modifierKeys;

        [JsonProperty("Key")] Key ShortcutKey 
        { 
            get => _shortcutKey; 
            set 
            {
                SetProperty(ref _shortcutKey, value);
                tempShortcutKey = value;
                CreateShortcutString();
            } 
        }
        [JsonProperty("ModifierKeys")] ModifierKeys ShortcutModifierKeys
        {
            get => _modifierKeys;
            set
            {
                SetProperty(ref _modifierKeys, value);
                tempShortcutModifierKeys = value;
                CreateShortcutString();
            }
        }

        [JsonIgnore] public uint VKey => (uint)KeyInterop.VirtualKeyFromKey(ShortcutKey);
        [JsonIgnore] public uint ModifsKey => (uint)ShortcutModifierKeys;

        [JsonIgnore] public int Id => Path.GetHashCode();
        public string Path { get => _path; set => SetProperty(ref _path, value); }
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        [JsonIgnore] public string Shortcut
        {
            get => _shortcut;
            set
            {
                SetProperty(ref _shortcut, value);
                //shortcutKeys = Shortcut.Split('+').Select(s => s.Trim()).Select(s => { Enum.TryParse(s, out Key key); return key; }).ToList();
            }
        }

        public float Volume
        {
            get => _volume;
            set
            {
                SetProperty(ref _volume, value);
                if (virtualAudioFile != null)
                    virtualAudioFile.Volume = Volume / 100f;
                if (hardwareAudioFile != null)
                    hardwareAudioFile.Volume = Volume / 100f;
            }
        }
        [JsonIgnore] public bool IsCurrentlyPlaying { get; set; }

        public Sound(string path, string name)
            : this(path, name, 100f) { }

        [JsonConstructor] public Sound(string path, string name, float volume)
        {
            CurrentState = PlayingState.Stopped;
            Path = path;
            Name = name;
            Volume = volume;
        }

        public void ClearHotKeys()
        {
            tempShortcutKey = Key.None;
            tempShortcutModifierKeys = ModifierKeys.None;
            SaveShortcutKeys();
        }

        public void AddHotKey(Key k)
        {
            string s = k.ToString();
            if (s.Contains("Ctrl"))
                tempShortcutModifierKeys |= ModifierKeys.Control;
            else if (s.Contains("Alt"))
                tempShortcutModifierKeys |= ModifierKeys.Alt;
            else if (s.Contains("Shift"))
                tempShortcutModifierKeys |= ModifierKeys.Shift;
            else if (s.Contains("Win"))
                tempShortcutModifierKeys |= ModifierKeys.Windows;
            else
                tempShortcutKey = k;

            CreateShortcutString();
        }

        public void SaveShortcutKeys()
        {
            ShortcutKey = tempShortcutKey;
            ShortcutModifierKeys = tempShortcutModifierKeys;
            CreateShortcutString();
        }

        void CreateShortcutString()
        {
            if (tempShortcutModifierKeys == ModifierKeys.None)
            {
                if (tempShortcutKey == Key.None)
                    Shortcut = "";
                else
                    Shortcut = tempShortcutKey.ToString();
                return;
            }
            string modif = tempShortcutModifierKeys.ToString().Replace("Control", "Ctrl").Replace("Windows", "Win").Replace(", ", " + ");
            Shortcut = string.Join(" + ", modif, tempShortcutKey);
            //shortcutKeys = Shortcut.Split('+').Select(s => s.Trim()).Select(s => { Enum.TryParse(s, out Key key); return key; }).ToList();
        }

        void InitDevices()
        {
            if (CurrentState == PlayingState.Playing)
            {
                virtualDevice = new WaveOutEvent() { DeviceNumber = MainWindow.VirtualDeviceNumber };
            }
            hardwareDevice = new WaveOutEvent() { DeviceNumber = MainWindow.HardwareDeviceNumber };
            hardwareDevice.PlaybackStopped += (s, a) => Stop();
        }

        void InitFiles()
        {
            if (CurrentState == PlayingState.Playing)
                virtualAudioFile = new AudioFileReader(Path) { Volume = Volume / 100f };
            hardwareAudioFile = new AudioFileReader(Path) { Volume = Volume / 100f };
        }

        public void Play()
        {
            Stop();
            CurrentState = PlayingState.Playing;
            InitFiles();
            InitDevices();

            virtualDevice.Init(virtualAudioFile);
            hardwareDevice.Init(hardwareAudioFile);

            virtualDevice.Play();
            hardwareDevice.Play();
        }

        public void Preview()
        {
            Stop();
            CurrentState = PlayingState.Preview;
            InitFiles();
            InitDevices();

            hardwareDevice.Init(hardwareAudioFile);

            hardwareDevice.Play();
        }

        public void Stop()
        {
            CurrentState = PlayingState.Stopped;
            
            hardwareDevice?.Stop();
            hardwareDevice?.Dispose();
            hardwareDevice = null;

            virtualDevice?.Stop();
            virtualDevice?.Dispose();
            virtualDevice = null;
        }

        public override string ToString() => Name;

        public enum PlayingState
        {
            Stopped,
            Preview,
            Playing
        }
    }
}
