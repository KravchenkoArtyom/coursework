using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Wpf;

namespace WpfApp
{
    /// <summary>
    /// Логика взаимодействия для GraphOptionsWindow.xaml
    /// </summary>
    public partial class GraphOptionsWindow : Window
    {

        public Movie Movie { get; }
        public MovieContext _dbContext;
        public GraphOptionsWindow(MovieContext dbContext)
        {
            InitializeComponent();

            _dbContext = dbContext;
            string[] options = { "Rate", "Year", "Duration" };
            FirstParameterComboBox.ItemsSource = options;
            SecondParameterComboBox.ItemsSource = options;
        }

        private void BuildGraph_Click(object sender, RoutedEventArgs e)
        {
            string firstParam = FirstParameterComboBox.SelectedItem as string;
            string secondParam = SecondParameterComboBox.SelectedItem as string;
            if(firstParam == null || secondParam == null)
            {
                MessageBox.Show("Пожалуйста, выберите параметры");
                return;
            }
             var movies = _dbContext.Movies.ToList();
             BuildGraphic(movies, firstParam, secondParam);
           

            
            ReportButton.IsEnabled = true;
            ExportGraph.IsEnabled = true;

        }

        private void BuildGraphic(List<Movie> movies, string firstParam, string secondParam)
        {
            movies = movies.OrderBy(m => GetPropertyValue(m, firstParam)).ToList();

            var plotModel = new PlotModel { Title = "" };
            var minValueX = movies.Min(m => GetPropertyValue(m, firstParam));
            var maxValueX = movies.Max(m => GetPropertyValue(m, firstParam));

            var minValueY = movies.Min(m => GetPropertyValue(m,secondParam));
            var maxValueY = movies.Max(m => GetPropertyValue(m, secondParam));

            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = minValueX,
                Maximum = maxValueX,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            };

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = minValueY,
                Maximum = maxValueY,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            };



            plotModel.Axes.Add(xAxis);
            plotModel.Axes.Add(yAxis);


            var series = new LineSeries();
            foreach(var movie in movies)
            {
                double x = GetPropertyValue(movie, firstParam);
                double y = GetPropertyValue(movie, secondParam);

                series.Points.Add(new DataPoint(x, y));
            }
            plotModel.Series.Add(series);
            this.MyPlotView.Model = plotModel;
        }

        private double GetPropertyValue(Movie movie, string propertyName)
        {
            switch (propertyName)
            {
                case "Rate":
                    return movie.Rate;
                case "Year":
                    return movie.Year;
                case "Duration":
                    return movie.Duration;
                default:
                    throw new ArgumentException("Свойство не найдено", propertyName);
            }
        }

        private void ExportGraph_Click(object sender, RoutedEventArgs e)
        {
            var plotModel = this.MyPlotView.Model;
            if (plotModel == null) return;
            plotModel.Background = OxyColors.White;
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PNG Files(*.png)|*.png|All files (*.*)|*.*",
                Title = "Экспортировать график"
            };

            bool? result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                var pngExporter = new PngExporter
                {
                    Width = (int)this.MyPlotView.ActualWidth,
                    Height = (int)this.MyPlotView.ActualHeight,
                };
                pngExporter.ExportToFile(plotModel, saveFileDialog.FileName);
            }

            
        }

        [Obsolete]
        private void GenerateReport(string reportPath)
        {
            //путь к шаблону отчета
            var templatePath = ".\\Report.docx";
            

            //загрузка шаблона документа с помощью DocX
            var report = Xceed.Words.NET.DocX.Load(templatePath);
           
            var totalMovies = _dbContext.Movies.Count();
            var highestRate = _dbContext.Movies.Max(m => m.Rate);
            var lowestRate = _dbContext.Movies.Min(m => m.Rate);
            var averageRate = _dbContext.Movies.Average(m => m.Rate);

            string foundMovie = FindMovie.Text;
            string foundCountry = FindCountry.Text;

            var foundMovieEntity = _dbContext.Movies
            .Include("Genres")
            .Include("Countries")
            .Include("Directors")
            .FirstOrDefault(m => m.Name == foundMovie); 

            var moviesByCountry = _dbContext.Movies.     
                Include("Countries").   
                Where(m => m.Countries.Any(c => c.CountryName == foundCountry)).
                OrderByDescending(m => m.Rate).
                ToList();



            if (foundMovieEntity == null)
            {
                MessageBox.Show("Фильм с таким названием не найден в базе данных.");
                return;
            }


            string foundMovieDescription = GenerateMovieDescription(foundMovieEntity);
            StringBuilder moviesByCountryStringBuilder = new StringBuilder();
            foreach(var movie in moviesByCountry)
            {
                moviesByCountryStringBuilder.AppendLine(movie.Name);
                
            }
            string moviesByCountryList = moviesByCountryStringBuilder.ToString();
            
            report.ReplaceText("<count>", totalMovies.ToString());
            report.ReplaceText("<bestRate>", highestRate.ToString("0.0"));
            report.ReplaceText("<averageRate>", averageRate.ToString("0.0"));
            report.ReplaceText("<worstRate>", lowestRate.ToString("0.0"));
            report.ReplaceText("<movieTitle>", foundMovie);
            report.ReplaceText("<movieInfo>", foundMovieDescription);
            report.ReplaceText("<country>", foundCountry);
            report.ReplaceText("<listMovies>", moviesByCountryList);

            var plotImage = SavePlotImage();
            if (plotImage != null)
            {
                var image = report.AddImage(plotImage);
                var picture = image.CreatePicture();
                report.InsertParagraph().AppendPicture(picture);
            }

            report.SaveAs(reportPath);
            MessageBox.Show("Отчет успешно создан!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            System.Diagnostics.Process.Start(reportPath);


        }

        public string GenerateMovieDescription(Movie movie)
        {
            string genres = string.Join(", ", movie.Genres.Select(g => g.GenreName));
            string countries = string.Join(", ", movie.Countries.Select(c => c.CountryName));
            string directors = string.Join(", ", movie.Directors.Select(d => d.DirectorName));

            StringBuilder description = new StringBuilder();
            description.AppendLine($"Название фильма: {movie.Name}");
            description.AppendLine($"Синопсис фильма: {movie.Synopsis}");
            description.AppendLine($"Продолжительность фильма: {movie.Duration}");
            description.AppendLine($"Год выхода фильма: {movie.Year}");
            description.AppendLine($"Рейтинг фильма: {movie.Rate}");
            description.AppendLine($"Жанры фильма: {genres}");
            description.AppendLine($"Страна(ы) производства: {countries}");
            description.AppendLine($"Режиссер(ы) фильма: {directors}");
            description.AppendLine();

            return description.ToString();
        }

        
        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            string first = SecondParameterComboBox.SelectedValue.ToString();
            string second = FirstParameterComboBox.SelectedValue.ToString();
            List<string> forbiddenCharacters = new List<string> { "\"", ":", ".", ";",",","'"," "};
            string movieTitle = FindMovie.Text.Trim();
            if(FindMovie.Text.Length == 0)
            {
                MessageBox.Show("Введите корректный фильм");
                return;
            }
            foreach (string forbiddenCharacter in forbiddenCharacters)
            {
                movieTitle = movieTitle.Replace(forbiddenCharacter, "");
            }
            string country = FindCountry.Text.Trim();
            if (FindCountry.Text.Length == 0)
            {
                MessageBox.Show("Введите корректную страну");
                return;
            }

            string reportFileName = $"{first}{second}_{movieTitle}_{country}.docx";

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = reportFileName,
                DefaultExt = ".docx",
                Filter = "Word Documents (*.docx)|*.docx",
                Title = "Сохранить как"
            };
            if(saveFileDialog.ShowDialog() == true)
            {
                string reportPath = saveFileDialog.FileName;
                GenerateReport(reportPath);
            }


            
        }
        private string SavePlotImage()
        {
            var pngExporter = new PngExporter { Width = 600, Height = 400 };
            var plotImageStream = new MemoryStream();
            pngExporter.Export(this.MyPlotView.Model, plotImageStream);
            var imagePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "plot.png");
            File.WriteAllBytes(imagePath, plotImageStream.ToArray());
            return imagePath;
        }




        //поиск названия фильма и страны для генерации отчета
        private void FindMovie_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string currentText = textBox.Text.Trim();
            if (currentText.Length < 3)
            {
                SearchResultsListBox.Visibility = Visibility.Collapsed; // Скрыть если меньше 3 символов
                return;
            }
            if (_dbContext != null)
            {
                List<string> searchResults = SearchDatabaseForMovies(currentText);
                if (searchResults.Any())
                {
                    SearchResultsListBox.ItemsSource = searchResults;
                    SearchResultsListBox.Visibility = Visibility.Visible;
                }
                else
                {
                    SearchResultsListBox.Visibility = Visibility.Collapsed; // Скрыть если нет результатов
                }
            }
        }

        private List<string> SearchDatabaseForMovies(string searchText)
        {
            var queryResults = _dbContext.Movies.
                Where(m => m.Name.Contains(searchText)).
                Select(m => m.Name).
                ToList();


            return queryResults;
        }



        private void FindMovie_GotFocus(object sender, RoutedEventArgs e)
        {
            if(FindMovie.Text == "Введите название фильма")
            {
                FindMovie.Text = string.Empty;
            }
        }
        private void FindCountry_GotFocus(object sender, RoutedEventArgs e)
        {

            if (FindCountry.Text == "Введите название страны")
            {
                FindCountry.Text = string.Empty;
            }
        }

        private void SearchResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            if (SearchResultsListBox.SelectedItem is string selectedMovie)
            {
                FindMovie.Text = selectedMovie;
            }

            SearchResultsListBox.Visibility = Visibility.Collapsed; 
        }


   

        private void FindCountry_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string currentText = textBox.Text.Trim();
            if (currentText.Length < 3)
            {
                SearchCountryListBox.Visibility = Visibility.Collapsed; // Скрыть если меньше 3 символов
                return;
            }
            if (_dbContext != null)
            {
                List<string> searchResults = SearchDatabaseForCountries(currentText);
                if (searchResults.Any())
                {
                    SearchCountryListBox.ItemsSource = searchResults;
                    SearchCountryListBox.Visibility = Visibility.Visible;
                }
                else
                {
                    SearchCountryListBox.Visibility = Visibility.Collapsed; // Скрыть если нет результатов
                }
            }
        }


        private List<string> SearchDatabaseForCountries(string searchText)
        {
            var queryResults = _dbContext.Countries.
                Where(m => m.CountryName.Contains(searchText)).
                Select(m => m.CountryName).
                ToList();
            return queryResults;
        }

        private void SearchResultsListBoxCountry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (SearchCountryListBox.SelectedItem is string selectedCountry)
            {
                FindCountry.Text = selectedCountry;
                
            }

            SearchCountryListBox.Visibility = Visibility.Collapsed;
        }

    }
}
