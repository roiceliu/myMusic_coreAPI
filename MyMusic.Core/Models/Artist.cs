using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MyMusic.Core.Models{
    public class Artist{
        public Artist(){
            Musics = new Collection<Music>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        //property where a single artist can have multiple music
        public ICollection<Music> Musics { get; set; }
    }
}