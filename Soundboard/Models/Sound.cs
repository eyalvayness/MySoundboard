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


        [JsonProperty("Key")] Key shortcutKey;
        [JsonProperty("ModifierKeys")] ModifierKeys shortcutModifierKeys;

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
            shortcutKey = Key.None;
            shortcutModifierKeys = ModifierKeys.None;
            CreateShortcut();
        }

        public void AddHotKey(Key k)
        {
            string s = k.ToString();
            if (s.Contains("Ctrl"))
                shortcutModifierKeys |= ModifierKeys.Control;
            else if (s.Contains("Alt"))
                shortcutModifierKeys |= ModifierKeys.Alt;
            else if (s.Contains("Shift"))
                shortcutModifierKeys |= ModifierKeys.Shift;
            else if (s.Contains("Win"))
                shortcutModifierKeys |= ModifierKeys.Windows;
            else
                shortcutKey = k;

            CreateShortcut();
        }

        void BackupShortcut()
        {
        }

        void CreateShortcut()
        {
            string modif = shortcutModifierKeys.ToString().Replace("Control", "Ctrl").Replace("Windows", "Win").Replace(", ", " + ");
            if (modif == "None")
                Shortcut = shortcutKey.ToString();
            else
                Shortcut = string.Join(" + ", modif, shortcutKey);
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
            hardwareDevice?.Stop();
            hardwareDevice?.Dispose();
            hardwareDevice = null;

            virtualDevice?.Stop();
            virtualDevice?.Dispose();
            virtualDevice = null;

            CurrentState = PlayingState.Stopped;
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
