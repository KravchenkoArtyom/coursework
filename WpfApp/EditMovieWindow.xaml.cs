using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wpf;
using WpfApp.Entities;

namespace WpfApp
{

    public partial class EditMovieWindow : Window
    {
        private Movie Movie { get; }

        private MovieContext _dbContext;

        public EditMovieWindow(Movie movie)
        {
            InitializeComponent();
            Movie = movie;
            DataContext = Movie;

            LoadEntities();

        }
        private void LoadEntities()
        {
            _dbContext = new MovieContext();

            GenresListBox.ItemsSource = _dbContext.Genres.ToList();
            CountriesListBox.ItemsSource = _dbContext.Countries.ToList();
            DirectorsListBox.ItemsSource = _dbContext.Directors.ToList();

            GenresListBox.SelectedItems.Clear();
            foreach (var genre in Movie.Genres)
            {
                var item = GenresListBox.Items.Cast<Genre>().FirstOrDefault(g => g.GenreName == genre.GenreName);
                if (item != null) GenresListBox.SelectedItems.Add(item);
            }

            CountriesListBox.SelectedItems.Clear();
            foreach (var country in Movie.Countries)
            {
                var item = CountriesListBox.Items.Cast<Country>().FirstOrDefault(c => c.CountryName == country.CountryName);
                if (item != null) CountriesListBox.SelectedItems.Add(item);
            }

            DirectorsListBox.SelectedItems.Clear();
            foreach (var director in Movie.Directors)
            {
                var item = DirectorsListBox.Items.Cast<Director>().FirstOrDefault(d => d.DirectorID == director.DirectorID);
                if (item != null) DirectorsListBox.SelectedItems.Add(item);
            }
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (_dbContext = new MovieContext())
                {
                    var movieToUpdate = _dbContext.Movies
                        .Include(m => m.Genres)
                        .Include(m => m.Countries)
                        .Include(m => m.Directors)
                        .FirstOrDefault(m => m.MovieID == Movie.MovieID);

                    if (movieToUpdate != null)
                    {

                        if (NameTextBox.Text.Length > 0)
                        {
                            movieToUpdate.Name = NameTextBox.Text;
                        }
                        else
                        {
                            MessageBox.Show("Введите название фильма");
                            return;
                        }
                        movieToUpdate.Synopsis = SynopsisTextBox.Text;
                        if (int.TryParse(YearTextBox.Text, out int year)) 
                        {
                            movieToUpdate.Year = year;
                        }
                        else
                        {
                            MessageBox.Show("Введите корректный год.");
                            return;
                        }

                        if(int.TryParse(DurationTextBox.Text,out int duration))
                        {
                            movieToUpdate.Duration = duration;
                        }
                        else
                        {
                            MessageBox.Show("Введите корректное значение продолжительности фильма.");
                            return;
                        }

                        string rateText = RateTextBox.Text.Replace(',', '.');
                        if (float.TryParse(rateText, NumberStyles.Any, CultureInfo.InvariantCulture, out float rate))
                        {
                            movieToUpdate.Rate = rate;
                        }
                        else
                        {
                            MessageBox.Show("Введите корректный рейтинг.");
                            return;
                        }

                        if (GenresListBox.SelectedItems.Count == 0)
                        {
                            MessageBox.Show("Выберите хотя бы один жанр.");
                            return;
                        }

                        // Проверяем, что выбрана хотя бы одна страна
                        if (CountriesListBox.SelectedItems.Count == 0)
                        {
                            MessageBox.Show("Выберите хотя бы одну страну.");
                            return;
                        }

                        // Проверяем, что выбран хотя бы один режиссер
                        if (DirectorsListBox.SelectedItems.Count == 0)
                        {
                            MessageBox.Show("Выберите хотя бы одного режиссера.");
                            return;
                        }

                        // Очищение текущих связей во избежание дублирования
                        movieToUpdate.Genres.Clear();
                        movieToUpdate.Countries.Clear();
                        movieToUpdate.Directors.Clear();
                        _dbContext.SaveChanges();

                        // Добавляем выбранные жанры
                        foreach (Genre selectedGenre in GenresListBox.SelectedItems)
                        {
                            Genre genre = _dbContext.Genres.Find(selectedGenre.GenreName);
                            if (genre != null)
                            {
                                movieToUpdate.Genres.Add(genre);
                            }
                           
                        }

                        // Добавляем выбранные страны
                        foreach (Country selectedCountry in CountriesListBox.SelectedItems)
                        {
                            Country country = _dbContext.Countries.Find(selectedCountry.CountryName);
                            if (country != null)
                            {
                                movieToUpdate.Countries.Add(country);
                            }
                            
                        }

                        // Добавляем выбранных режиссеров
                        foreach (Director selectedDirector in DirectorsListBox.SelectedItems)
                        {
                            Director director = _dbContext.Directors.Find(selectedDirector.DirectorID);
                            if (director != null)
                            {
                                movieToUpdate.Directors.Add(director);
                            }
                            
                        }

                        // Обновляем фильм с новыми связями
                        _dbContext.Entry(movieToUpdate).State = EntityState.Modified;
                        _dbContext.SaveChanges();
                        MessageBox.Show("Фильм успешно обновлен!");
                        DialogResult = true;
                    }
                }
            }


            
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении изменений: " + ex.Message);
            }
        }

    }

}
