using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp.Entities;

namespace Wpf
{
    public class Movie
    {
        public Movie()
        {
            Genres = new List<Genre>();
            Countries = new List<Country>();
            Directors = new List<Director>();
        }

        public Guid MovieID { get; set; }

        [MinLength(1)]
        
        public string Name { get; set; }
        public string Synopsis { get; set; }
        public int Year { get; set; }
        public int Duration { get; set; }
        public float Rate { get; set; }
        public virtual ICollection<Genre> Genres { get; set; }
        public virtual ICollection<Country> Countries { get; set; }
        public virtual ICollection<Director> Directors { get; set; }

        public string GetInfo()
        {
            string genres = string.Join(", ", Genres.Select(g => g.GenreName));
            string countries = string.Join(", ", Countries.Select(c => c.CountryName));
            string directors = string.Join(", ", Directors.Select(d => d.DirectorName));
            return $"Название фильма: {Name}\nСинопсис: {Synopsis}\nЖанр: {genres}\nПродолжительность: {Duration} мин" +
                   $"\nСтрана: {countries}\nРежиссер:{directors}\nГод выпуска: {Year}\nРейтинг Letterbox: {Rate}\n";
        }


    }
}
