using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf;
using WpfApp.ViewModel;

namespace WpfApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        private bool isParsing = false;
        public MainWindow()
        {
            InitializeComponent();

            MoviesListBox.PreviewMouseRightButtonDown += MoviesListBox_PreviewMouseRightButtonDown;
        }
        private void MoviesListBox_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // создание конктекстного меню
            ContextMenu contextMenu = new ContextMenu();

            // создание пунктов меню
            MenuItem deleteMenuItem = new MenuItem { Header = "Удалить" };
            deleteMenuItem.Click += DeleteMenuItem_Click;

            MenuItem editMenuItem = new MenuItem { Header = "Редактировать" };
            editMenuItem.Click += EditMenuItem_Click;

            // пункты меню в контекстное меню
            contextMenu.Items.Add(deleteMenuItem);
            contextMenu.Items.Add(editMenuItem);


            deleteMenuItem.IsEnabled = !isParsing;
            editMenuItem.IsEnabled = !isParsing;


            // Открываем контекстное меню
            contextMenu.PlacementTarget = MoviesListBox;
            contextMenu.Placement = PlacementMode.MousePoint;
            contextMenu.IsOpen = true;
        }
        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MoviesListBox.SelectedItem is MovieViewModel selectedViewModel)
            {
                Movie selectedMovie = selectedViewModel.movie;
                if (selectedMovie != null)
                {
                    using (var db = new MovieContext())
                    {
                        // Сначала проверяем, есть ли объект в базе данных
                        var movieInDb = db.Movies.Find(selectedMovie.MovieID);
                        if (movieInDb != null)
                        {
                            // Если объект найден, происходит удаление
                            db.Movies.Remove(movieInDb);
                            db.SaveChanges();

                            // Обновляем список фильмов
                            LoadMoviesButton_Click(null, null);
                        }
                        else
                        {
                            MessageBox.Show("Фильм не найден в базе данных.");
                        }
                    }
                }
            }
        }



        private void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MoviesListBox.SelectedItem is MovieViewModel selectedMovieViewModel)
            {
                var editWindow = new EditMovieWindow(selectedMovieViewModel.movie); // передача текущего выбранного объекта фильма
                var result = editWindow.ShowDialog(); // отображение окна для редактирования
                if (result == true)
                {
                    // Вы можете обновить отображаемое значение в ListBox или перезагрузить данные.
                    LoadMoviesButton_Click(null, null);
                }
            }
        }


        private void LoadMoviesButton_Click(object sender, RoutedEventArgs e)
        {
            MoviesListBox.Items.Clear();
            isParsing = false;
            using (var db = new MovieContext())
            {
                var movies = db.Movies
                     .Include("Genres")
                     .Include("Countries")
                     .Include("Directors")
                     .OrderByDescending(m => m.Rate)
                     .ToList();


                foreach (var movie in movies)
                {
                    string genres = string.Join(", ", movie.Genres.Select(g => g.GenreName));
                    string countries = string.Join(", ", movie.Countries.Select(c => c.CountryName));
                    string directors = string.Join(", ", movie.Directors.Select(d => d.DirectorName));


                    var movieViewModel = new MovieViewModel
                    {
                        movie = movie,
                        DisplayText = $"Название фильма: {movie.Name}\nСинопсис: {movie.Synopsis}\nЖанр: {genres}\nПродолжительность: {movie.Duration} мин" +
                        $"\nСтрана: {countries}\nРежиссер:{directors}\nГод выпуска: {movie.Year}\nРейтинг Letterbox: {movie.Rate}\n"
                    };

                    MoviesListBox.Items.Add(movieViewModel);
                }
            }
        }

        private async void ParseMoviesButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            ParseButton.IsEnabled = false;
            isParsing = true;
            GraphButton.IsEnabled = false;
            LoadMoviesButton.IsEnabled = false;
            MoviesListBox.Items.Clear();

            using (var db = new MovieContext())
            {
                db.Movies.RemoveRange(db.Movies);
                db.Genres.RemoveRange(db.Genres);
                db.Countries.RemoveRange(db.Countries);
                db.Directors.RemoveRange(db.Directors);
                db.SaveChanges();
            }

            var parser = new Parser();
            int pageNumber = 1;
            while (true)
            {
               
                string mainPage = $"https://letterboxd.com/dave/list/official-top-250-narrative-feature-films/page/{pageNumber}/";
                List<string> movies = await parser.GetLinks(mainPage);
                if (movies.Count == 0) // Если на странице нет фильмов, значит, мы достигли конца списка
                {
                    break;
                }

                foreach (string movieUrl in movies)
                {

                    var info = await parser.Parse(movieUrl);
                    var movieViewModel = new MovieViewModel
                    {
                       
                        DisplayText = info
                    };

                    MoviesListBox.Items.Add(movieViewModel);

                }
                pageNumber++; // Переходим на следующую страницу
            }
            isParsing = false;
            LoadMoviesButton.IsEnabled = true;
            ParseButton.IsEnabled = true;
            GraphButton.IsEnabled = true;

        }

        private void GraphButton_Click(object sender, RoutedEventArgs e)
        {
            var db = new MovieContext();
            var graphOptionsWindow = new GraphOptionsWindow(db);
            graphOptionsWindow.Show();
        }
    }
}
