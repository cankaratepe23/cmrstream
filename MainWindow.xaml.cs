using FluentFTP;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Comarstream
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ObservableCollection<ShowEntry> ShowEntries = new ObservableCollection<ShowEntry>();
        List<Series> series;
        List<Movie> movies;
        Stack<object> navigationTracker = new Stack<object>();

        public string TvdbId { get; set; }
        public string RootPath { get; set; } = "/files";
        private string _SaveStatus;
        public string SaveStatus
        {
            get { return _SaveStatus; }
            set
            {
                _SaveStatus = value;
                RaisePropertyChanged("SaveStatus");
            }
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            showsGrid.ItemsSource = ShowEntries;
            CollectionViewSource.GetDefaultView(showsGrid.ItemsSource).SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            UpdateEntries();
        }

        private void UpdateEntries()
        {
            series = JsonConvert.DeserializeObject<List<Series>>(File.ReadAllText("db_series.json"));
            movies = JsonConvert.DeserializeObject<List<Movie>>(File.ReadAllText("db_movies.json"));
            foreach (Movie movie in movies)
            {
                if (!ShowEntries.Contains(movie))
                {
                    ShowEntries.Add(movie);
                }
            }
            foreach (Series seriesItem in series)
            {
                if (!ShowEntries.Contains(seriesItem))
                {
                    ShowEntries.Add(seriesItem);
                }
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            ShowEntry showEntry = ((sender as FrameworkElement).DataContext as ShowEntry);
            string filePath;
            if (showEntry == null)
            {
                Season season = ((sender as FrameworkElement).DataContext as Season);
                if (season == null)
                {
                    throw new Exception("Could not parse the DataContext into a proper object.");
                }
                filePath = season.Path;
            }
            else
            {
                filePath = showEntry.Path;
            }
            string fullPath = Settings.Default.FTP_Path + "\"" + filePath + "\"";
            ProcessStartInfo mpvInfo = new ProcessStartInfo()
            {
                FileName = "mpv.exe",
                Arguments = fullPath
            };
            Process mpv = Process.Start(mpvInfo);
            mpv.WaitForExit();
        }

        private void Info_Click(object sender, RoutedEventArgs e)
        {
            var oldDataContext = infoGrid.DataContext;
            infoGrid.Tag = (sender as FrameworkElement).DataContext.GetType().Name;
            infoGrid.DataContext = (sender as FrameworkElement).DataContext;
            if (infoGrid.Tag.ToString() == "Series")
            {
                descriptionBlock.DataContext = infoGrid.DataContext;
                ratingGrid.DataContext = infoGrid.DataContext;
                seasonsIC.SetBinding(ItemsControl.ItemsSourceProperty, new Binding("Seasons"));
                seasonsGrid.Visibility = Visibility.Visible;
                episodesGrid.Visibility = Visibility.Hidden;
                peopleGrid.Visibility = Visibility.Hidden;
            }
            else if (infoGrid.Tag.ToString() == "Movie")
            {
                descriptionBlock.DataContext = infoGrid.DataContext;
                ratingGrid.DataContext = infoGrid.DataContext;
                seasonsGrid.Visibility = Visibility.Hidden;
                episodesGrid.Visibility = Visibility.Hidden;
                peopleGrid.Visibility = Visibility.Visible;
            }
            else if (infoGrid.Tag.ToString() == "Season")
            {
                descriptionBlock.DataContext = oldDataContext;
                ratingGrid.DataContext = oldDataContext;
                episodesIC.SetBinding(ItemsControl.ItemsSourceProperty, new Binding("Episodes"));
                seasonsGrid.Visibility = Visibility.Hidden;
                episodesGrid.Visibility = Visibility.Visible;
                peopleGrid.Visibility = Visibility.Hidden;
            }
            else if (infoGrid.Tag.ToString() == "Episode")
            {
                descriptionBlock.DataContext = infoGrid.DataContext;
                ratingGrid.DataContext = infoGrid.DataContext;
                seasonsGrid.Visibility = Visibility.Hidden;
                episodesGrid.Visibility = Visibility.Hidden;
                peopleGrid.Visibility = Visibility.Visible;
            }
            infoGrid.Visibility = Visibility.Visible;
            navigationTracker.Push(infoGrid.DataContext);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (infoGrid.Tag == null)
            {
                return;
            }
            if (infoGrid.Tag.ToString() == "Series" || infoGrid.Tag.ToString() == "Movie")
            { // We were on Series/Movie info screen, we go back to the main grid.
                infoGrid.Visibility = Visibility.Hidden;
                infoGrid.Tag = null;
            }
            else if (infoGrid.Tag.ToString() == "Season")
            { // We were on Season info screen, we go back to the associated series' info screen.
                _ = navigationTracker.Pop();
                infoGrid.DataContext = navigationTracker.Peek();
                seasonsGrid.Visibility = Visibility.Visible;
                episodesGrid.Visibility = Visibility.Hidden;
                peopleGrid.Visibility = Visibility.Hidden;
                infoGrid.Tag = "Series";
            }
            else if (infoGrid.Tag.ToString() == "Episode")
            { // We were on Episode info screen, we go back to the associated season's info screen.
                _ = navigationTracker.Pop();
                infoGrid.DataContext = navigationTracker.Peek();
                seasonsGrid.Visibility = Visibility.Hidden;
                episodesGrid.Visibility = Visibility.Visible;
                peopleGrid.Visibility = Visibility.Hidden;
                infoGrid.Tag = "Season";
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (addGrid.IsVisible)
            {
                Storyboard sb = new Storyboard();

                DoubleAnimation spinAnimation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(150));
                BackEase backEase = new BackEase();
                backEase.EasingMode = EasingMode.EaseIn;
                spinAnimation.EasingFunction = backEase;
                Storyboard.SetTarget(spinAnimation, addButton.Template.FindName("btnImage", addButton) as Rectangle);
                Storyboard.SetTargetProperty(spinAnimation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
                sb.Children.Add(spinAnimation);

                DoubleAnimation slideAnimation = new DoubleAnimation(110, TimeSpan.FromMilliseconds(150));
                CubicEase cubicEase = new CubicEase();
                cubicEase.EasingMode = EasingMode.EaseInOut;
                slideAnimation.EasingFunction = cubicEase;
                Storyboard.SetTarget(slideAnimation, saveButton.Template.FindName("btnImage", saveButton) as Rectangle);
                Storyboard.SetTargetProperty(slideAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
                sb.Children.Add(slideAnimation);

                sb.Begin();

                addGrid.Visibility = Visibility.Hidden;
                saveButton.Visibility = Visibility.Hidden;
                backButton.IsEnabled = true;
            }
            else
            {
                Storyboard sb = new Storyboard();

                addGrid.Visibility = Visibility.Visible;
                saveButton.Visibility = Visibility.Visible;
                backButton.IsEnabled = false;

                DoubleAnimation spinAnimation = new DoubleAnimation(45, TimeSpan.FromMilliseconds(150));
                BackEase backEase = new BackEase();
                backEase.EasingMode = EasingMode.EaseIn;
                spinAnimation.EasingFunction = backEase;
                Storyboard.SetTarget(spinAnimation, addButton.Template.FindName("btnImage", addButton) as Rectangle);
                Storyboard.SetTargetProperty(spinAnimation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
                sb.Children.Add(spinAnimation);

                DoubleAnimation slideAnimation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(150));
                CubicEase cubicEase = new CubicEase();
                cubicEase.EasingMode = EasingMode.EaseInOut;
                slideAnimation.EasingFunction = cubicEase;
                Storyboard.SetTarget(slideAnimation, saveButton.Template.FindName("btnImage", saveButton) as Rectangle);
                Storyboard.SetTargetProperty(slideAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
                sb.Children.Add(slideAnimation);

                sb.Begin();
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            saveStatusGrid.Visibility = Visibility.Visible;
            SaveStatus = "Downloading metadata";
            Series seriesToAdd = await Series.CreateAsync(TvdbId);
            SaveStatus = "Connecting to server";
            FtpClient ftpClient = new FtpClient(Settings.Default.FTP_Host, new System.Net.NetworkCredential(Settings.Default.FTP_Username, Settings.Default.FTP_Password));
            await ftpClient.AutoConnectAsync();
            await ftpClient.SetWorkingDirectoryAsync(RootPath);
            SaveStatus = "Getting files";
            FtpListItem[] seasonsListing = await ftpClient.GetListingAsync();
            for (int seasonIndex = 0; seasonIndex < seriesToAdd.SeasonCount; seasonIndex++)
            {
                if (seasonIndex < seasonsListing.Length)
                {
                    seriesToAdd.Seasons[seasonIndex].Path = RootPath + "/" + seasonsListing[seasonIndex].Name; //TODO Maybe use Path.Combine? to handle errors like extra slashes
                    FtpListItem[] episodesListing = ftpClient.GetListing(seasonsListing[seasonIndex].FullName);
                    for (int episodeIndex = 0; episodeIndex < seriesToAdd.Seasons[seasonIndex].EpisodeCount; episodeIndex++)
                    {
                        if (episodeIndex < episodesListing.Length)
                        {
                            seriesToAdd.Seasons[seasonIndex].Episodes[episodeIndex].Path = seriesToAdd.Seasons[seasonIndex].Path + "/" + episodesListing[episodeIndex].Name;
                            seriesToAdd.Seasons[seasonIndex].Episodes[episodeIndex].Downloaded = true;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }
            }
            seriesToAdd.Path = RootPath;
            series.Add(seriesToAdd);
            SaveStatus = "Writing to file";
            File.WriteAllText("db_series.json", JsonConvert.SerializeObject(series));
            SaveStatus = "Done!";
            UpdateEntries();
            Add_Click(sender, e);
        }
    }
}
