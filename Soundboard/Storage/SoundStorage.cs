using Newtonsoft.Json.Linq;
using Soundboard.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soundboard.Storage
{
    public class SoundStorage
    {
        private static readonly string SoundsPath = @"Storage\SavedSounds.json";
        public SuperObservableCollection<Sound> Sounds { get; }

        public SoundStorage()
        {
            Sounds = new SuperObservableCollection<Sound>(SoundChange);
            CheckExists();
            LoadSounds();
        }

        void CheckExists()
        {
            if (!File.Exists(SoundsPath))
                File.WriteAllText(SoundsPath, "[]");
        }

        public bool LoadSounds()
        {
            string content = File.ReadAllText(SoundsPath);
            JArray arr = JArray.Parse(content);

            var sounds = arr.Select(t => t.ToObject<Sound>());

            Sounds.Clear();
            foreach (var sound in sounds)
                Sounds.Add(sound);
            return true;
        }

        public bool AddSound(Sound sound)
        {
            Sounds.Add(sound);
            return true;
        }

        public bool DeleteAll()
        {
            Sounds.Clear();
            SaveSounds();
            return true;
        }

        public bool AddSound(string filepath) => AddSound(new Sound(filepath, Path.GetFileNameWithoutExtension(filepath)));

        private void SoundChange(object sender, PropertyChangedEventArgs e) => SaveSounds();

        public bool SaveSounds()
        {
            JArray arr = JArray.FromObject(Sounds);
            File.Delete(SoundsPath);
            File.WriteAllText(SoundsPath, arr.ToString());
            return true;
        }
    }
}
