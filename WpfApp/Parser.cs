using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using System.Globalization;
using Newtonsoft.Json.Linq;
using WpfApp.Entities;
using Wpf;
using System.Data.Entity.Migrations;

namespace WpfApp
{
    class Parser
    {

        public async Task<string> Parse(string url)
        {
            //конфигурация
            IConfiguration config = Configuration.Default.WithDefaultLoader();
            //контекст
            IBrowsingContext context = BrowsingContext.New(config);
            //получаем документ
            IDocument doc = await context.OpenAsync(url);

            Movie movie = new Movie();

            IElement name = doc.QuerySelector("h1.headline-1.js-widont.prettify");           
            movie.Name = name.TextContent.Trim();

            IElement synopsis = doc.QuerySelector("h4.tagline");
            movie.Synopsis = synopsis == null ? "No synopsis" : synopsis.TextContent.Trim();

            IElement year = doc.QuerySelector("p small.number a");
            movie.Year = Convert.ToInt32(year.TextContent.Trim());

            var scriptElement = doc.QuerySelector("script[type='application/ld+json']");
            // Преобразуем текст скрипта в объект JSON
            var json = JObject.Parse(scriptElement.TextContent);
            // Извлекаем средний рейтинг
            float averageRating = json["aggregateRating"]["ratingValue"].Value<float>();
            movie.Rate = averageRating;


            string genresUrl = url + "genres/";
            IDocument genresDoc = await context.OpenAsync(genresUrl);
            var genreBlock = genresDoc.QuerySelector("div.text-sluglist.capitalize");
            var genres = genreBlock.QuerySelectorAll("a.text-slug");
            foreach (var el in genres)
                movie.Genres.Add(new Genre { GenreName = el.TextContent.Trim() }); 

  


            IElement durationElement = doc.QuerySelector("p.text-link.text-footer");
            string durationText = durationElement.TextContent.Trim();
            Match match = Regex.Match(durationText, @"\d+");
            if (match.Success)
            {
                int duration = int.Parse(match.Value);
                movie.Duration = duration;
            }


            string directorUrl = url + "crew/";
            IDocument directorsDoc = await context.OpenAsync(directorUrl);
            var directorBlock = directorsDoc.QuerySelector("div#tab-crew h3:contains('Director') + div.text-sluglist");
            var directors = directorBlock.QuerySelectorAll("a.text-slug");
            foreach (var el in directors)
                movie.Directors.Add(new Director { DirectorName = el.TextContent.Trim() });

            string detailsUrl = url + "details/";
            IDocument detailsDoc = await context.OpenAsync(detailsUrl);
            var countryBlock = detailsDoc.QuerySelector("div#tab-details h3:contains('Countr') + div.text-sluglist");
            var countries = countryBlock.QuerySelectorAll("a.text-slug");
            foreach (var el in countries)
                movie.Countries.Add(new Country { CountryName = el.TextContent.Trim() });
            


            using (var db = new MovieContext())
            {
                Guid movieId = Guid.NewGuid();
                Movie mov = new Movie
                {
                    MovieID = movieId,
                    Name = movie.Name,
                    Synopsis = movie.Synopsis,
                    Duration = movie.Duration,
                    Rate = movie.Rate,
                    Year = movie.Year,
                    Genres = new List<Genre>(),
                    Countries = new List<Country>(),
                    Directors = new List<Director>()
                };

                // Проверка и добавление Genres
                foreach (var genre in movie.Genres)
                {
                    var dbGenre = db.Genres.FirstOrDefault(g => g.GenreName == genre.GenreName);
                    if (dbGenre == null)
                    {
                        dbGenre = new Genre { GenreName = genre.GenreName };
                        db.Genres.Add(dbGenre); // Добавляем новый жанр, если такого еще нет
                    }
                    mov.Genres.Add(dbGenre); // Добавляем существующий жанр к фильму
                }

                // Проверка и добавление Countries
                foreach (var country in movie.Countries)
                {
                    var dbCountry = db.Countries.FirstOrDefault(c => c.CountryName == country.CountryName);
                    if (dbCountry == null)
                    {
                        dbCountry = new Country { CountryName = country.CountryName };
                        db.Countries.Add(dbCountry); // Добавляем новую страну, если такой еще нет
                    }
                    mov.Countries.Add(dbCountry); // Добавляем существующую страну к фильму
                }

                // Проверка и добавление Directors
                foreach (var director in movie.Directors)
                {
                    var dbDirector = db.Directors.FirstOrDefault(d => d.DirectorName == director.DirectorName);
                    if (dbDirector == null)
                    {
                        dbDirector = new Director { DirectorID = Guid.NewGuid(), DirectorName = director.DirectorName };
                        db.Directors.Add(dbDirector); // Добавляем нового режиссера, если такого еще нет
                    }
                    mov.Directors.Add(dbDirector); // Добавляем существующего режиссера к фильму
                }

                db.Movies.Add(mov); // Добавляем фильм
                
                db.SaveChanges(); // Сохраняем изменения в базе данных
                
            }


            return movie.GetInfo();
        }
        



        public async Task<List<string>>  GetLinks(string url)
        {
            Console.WriteLine("Начинаем загрузку главной страницы");
            IConfiguration config = Configuration.Default.WithDefaultLoader();
            IBrowsingContext context = BrowsingContext.New(config);
            IDocument doc = await context.OpenAsync(url);
            Console.WriteLine("Начинаем считывание главной страницы");

            IEnumerable<IElement> divElements = doc.QuerySelectorAll("li.poster-container div");
            List<string> output = new List<string>();


            foreach (IElement div in divElements.ToList())
            {
                string filmLink = div.GetAttribute("data-film-slug");
                
                if (!string.IsNullOrEmpty(filmLink))
                {
                    output.Add("https://letterboxd.com/film/" + filmLink + "/");
                }
                
            }
            Console.WriteLine("Считывание главной страницы завершено \n");
            return output;
        }

    }
}
