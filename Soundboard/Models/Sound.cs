using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soundboard.Models
{
    public class Sound
    {
        public string Path { get; set; }
        public string Name { get; set; }

        public Sound(string path, string name)
        {
            Path = path;
            Name = name;
        }
    }
}
