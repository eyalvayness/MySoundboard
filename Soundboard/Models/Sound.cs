using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Soundboard.Models
{
    public class Sound : BaseNotifier
    {
        string _name, _path;
        
        public string Path { get => _path; set => SetProperty(ref _path, value); }
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        [JsonIgnore] public AudioFileReader AudioFile { get => new AudioFileReader(Path); }

        public Sound(string path, string name)
        {
            Path = path;
            Name = name;
        }

        public override string ToString() => Name;
    }
}
