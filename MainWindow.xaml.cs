using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Comarstream
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ObservableCollection<ShowEntry> showEntries = new ObservableCollection<ShowEntry>();
            showsGrid.ItemsSource = showEntries;
            List<Series> series = JsonConvert.DeserializeObject<List<Series>>(File.ReadAllText("db_series.json"));
            List<Movie> movies = JsonConvert.DeserializeObject<List<Movie>>(File.ReadAllText("db_movies.json"));
            foreach (Series seriesItem in series)
            {
                showEntries.Add(seriesItem);
            }
            foreach (Movie movie in movies)
            {
                showEntries.Add(movie);
            }

            //List<Series> series = new List<Series>();
            //List<Movie> movies = new List<Movie>();
            //Series houseMd = await Series.CreateAsync("73255");
            //houseMd.Path = "/files/House M.D. (2004) The Complete Series [1080p] [BD] [x265] [pseudo]";
            //houseMd.Seasons[0].Path = houseMd.Path + "/Season 01";
            //houseMd.Seasons[0].Episodes[0].Path = houseMd.Seasons[0].Path + "/House M.D. S01E01 [1080p] [x265] [pseudo].mkv";
            //series.Add(houseMd);

            //Movie spiderman1 = await Movie.CreateAsync("301");
            //spiderman1.Path = "/files/Spider-Man.Trilogy.2002-2007.Mastered.In.4k.1080p.BluRay.DTS.x264-ETRG/Spider-Man.2002.Mastered.In.4k.1080p.BluRay.DTS.x264-ETRG/Spider-Man.2002.Mastered.In.4k.1080p.BluRay.DTS.x264-ETRG.mkv";
            //movies.Add(spiderman1);

            //Movie spiderman2 = await Movie.CreateAsync("6389");
            //spiderman2.Path = "/files/Spider-Man.Trilogy.2002-2007.Mastered.In.4k.1080p.BluRay.DTS.x264-ETRG/Spider-Man.2.2004.Mastered.In.4k.1080p.BluRay.DTS.x264-ETRG/Spider-Man.2.2004.Mastered.In.4k.1080p.BluRay.DTS.x264-ETRG.mkv";
            //movies.Add(spiderman2);

            //Movie spiderman3 = await Movie.CreateAsync("586");
            //spiderman3.Path = "/files/Spider-Man.Trilogy.2002-2007.Mastered.In.4k.1080p.BluRay.DTS.x264-ETRG/Spider-Man.3.2007.Mastered.In.4k.1080p.BluRay.DTS.x264-ETRG/Spider-Man.3.2007.Mastered.In.4k.1080p.BluRay.DTS.x264-ETRG.mkv";
            //movies.Add(spiderman3);

            //File.WriteAllText("db_series.json", JsonConvert.SerializeObject(series));
            //File.WriteAllText("db_movies.json", JsonConvert.SerializeObject(movies));
        }

        private void ShowGridItem_LeftMouseDown(object sender, MouseButtonEventArgs e)
        {
            string filePath = ((sender as Image).DataContext as ShowEntry).Path;
            string fullPath = Settings.Default.FTP_Path + "\"" + filePath + "\"";
            ProcessStartInfo mpvInfo = new ProcessStartInfo()
            {
                FileName = "mpv.exe",
                Arguments = fullPath
            };
            Process mpv = Process.Start(mpvInfo);
            mpv.WaitForExit();
        }
    }
}
