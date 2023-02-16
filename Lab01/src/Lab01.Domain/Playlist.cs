using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab01.Domain
{
    public class Car {

    }
    public class CarBuilder {

        private Car car = new Car();

        private CarBuilder(){

        }
        public static CarBuilder Create() {
            return new CarBuilder();
        }

        public CarBuilder AddEngine() {
            return this;
        }
        public CarBuilder AddWheel(){
            // add a wheel
            return this;
        }

        public Car Build() {
            return car;
        }
    }


    public class SimpleCalculator {
        public int Add(int x, int y) {
            return x + y;
        }


    }
    public class Song
    {
        public string Title { get; set; }
        public string Artist { get; set; }
    }

    public class ArtistTitleComparer: Comparer<Song>
    {
        public override int Compare(Song x, Song y) {
            if (x.Artist.CompareTo(y.Artist) != 0) return x.Artist.CompareTo(y.Artist);

            return 0;
        }
    }
    public class Playlist
    {
        public Playlist()
        {
            Songs = new List<Song>();
        }

        public List<Song> Songs { get; set; }

        public bool IsActive { get; set; }

        public string Title { get; set; }

        public void Clear() {
            Songs.Clear();
        }

        public void AddSong(Song songToAdd)
        {
            if (songToAdd.Artist == "ABBA")
                throw new InvalidOperationException("Abba songs are forbidden.");

            songToAdd.Title = $"2021 {songToAdd.Title}";
            Songs.Add(songToAdd);

            Songs.Sort(new ArtistTitleComparer());
        }
    }
}
