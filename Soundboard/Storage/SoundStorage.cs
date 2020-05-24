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
            Sounds.CollectionChanged += Sounds_CollectionChanged;
            CheckExists();
            LoadSounds();
        }
        void CheckExists()
        {
            if (!Directory.Exists("Storage"))
            {
                Directory.CreateDirectory("Storage");
            }
            if (!File.Exists(SoundsPath))
            {
                File.WriteAllText(SoundsPath, "[]");
            }
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

        public bool RemoveSound(Sound sound)
        {
            Sounds.Remove(sound);
            return true;
        }

        public bool DeleteAll()
        {
            Sounds.Clear();
            SaveSounds();
            return true;
        }

        public bool StopAll()
        {
            foreach (var s in Sounds)
            {
                s.Stop();
            }
            return true;
        }

        public bool AddSound(string filepath) => AddSound(new Sound(filepath, Path.GetFileNameWithoutExtension(filepath)));

        private void SoundChange(object sender, PropertyChangedEventArgs e) => SaveSounds();
        private void Sounds_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => SaveSounds();


        public bool SaveSounds()
        {
            JArray arr = JArray.FromObject(Sounds);
            File.Delete(SoundsPath);
            File.WriteAllText(SoundsPath, arr.ToString());
            return true;
        }
    }
}
