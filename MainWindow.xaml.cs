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

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
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
