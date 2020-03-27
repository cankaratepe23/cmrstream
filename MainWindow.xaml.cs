using FluentFTP;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace Comarstream
{
    class DirectoryListingItem
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string FullName { get; set; }
        public string Extension { get; set; }
        public string IconPath { get; set; }

        public DirectoryListingItem(string name, string type, string path)
        {
            Name = name;
            Type = type;
            FullName = path;
            if (type != "directory")
            {
                Extension = Path.GetExtension(name);
                switch (Extension)
                {
                    case "mkv":
                    case "mp4":
                    case "mov":
                        IconPath = "/res/video.png";
                        break;
                    default:
                        IconPath = "res/file.png";
                        break;
                }
            }
            else
            {
                IconPath = "res/directory.png";
            }

        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ObservableCollection<ShowEntry> _showEntries = new ObservableCollection<ShowEntry>();
        
        private List<Series> _series;
        private List<Movie> _movies;
        private Stack<object> _navigationTracker = new Stack<object>();

        private FtpClient ftpClient;

        private Storyboard refreshStoryboard;

        public string TvdbId { get; set; }

        private string _rootPath;
        public string RootPath
        {
            get => _rootPath;
            set
            {
                if (_rootPath == value)
                {
                    return;
                }
                _rootPath = value;
                RaisePropertyChanged("RootPath");
            }
        }

        private string _saveStatus;
        public string SaveStatus
        {
            get => _saveStatus;
            set
            {
                if (_saveStatus == value)
                {
                    return;
                }
                _saveStatus = value;
                RaisePropertyChanged("SaveStatus");
            }
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reload();
            if (string.IsNullOrWhiteSpace(Settings.Default.FTP_Host))
            {
                SettingsWindow settingsWindow = new SettingsWindow();
                bool? dialogResult = settingsWindow.ShowDialog();
                if (dialogResult == null || dialogResult == false)
                {
                    this.Close();
                }
                Settings.Default.Reload();
            }
            showsGrid.ItemsSource = _showEntries;
            CollectionViewSource.GetDefaultView(showsGrid.ItemsSource).SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            
            StartRefreshAnimation();
            await GetDbFilesAsync();
            StopRefreshAnimation();
            UpdateEntries();
        }

        private async Task GetDbFilesAsync()
        {
            if (ftpClient == null || !ftpClient.IsConnected)
            {
                ftpClient = new FtpClient(Settings.Default.FTP_Host, new System.Net.NetworkCredential(Settings.Default.FTP_Username, Settings.Default.FTP_Password));
                await ftpClient.AutoConnectAsync();
            }
            await ftpClient.DownloadFilesAsync(".", new[] {"/files/Çomarstream/db_movies.json", "files/Çomarstream/db_series.json"});
        }

        private void UpdateEntries()
        {
            _series = JsonConvert.DeserializeObject<List<Series>>(File.ReadAllText("db_series.json"));
            _movies = JsonConvert.DeserializeObject<List<Movie>>(File.ReadAllText("db_movies.json"));
            foreach (Movie movie in _movies)
            {
                if (!_showEntries.Contains(movie))
                {
                    _showEntries.Add(movie);
                }
            }
            foreach (Series seriesItem in _series)
            {
                if (!_showEntries.Contains(seriesItem))
                {
                    _showEntries.Add(seriesItem);
                }
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            string filePath;
            if (!((sender as FrameworkElement)?.DataContext is ShowEntry showEntry))
            {
                if (!((sender as FrameworkElement)?.DataContext is Season season))
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
            if (!File.Exists(Settings.Default.MediaPlayerPath))
            {
                SettingsWindow settingsWindow = new SettingsWindow();
                bool? result = settingsWindow.ShowDialog();
                if (result != true)
                {
                    return;
                }
                else
                {
                    Settings.Default.Reload();
                    if (!File.Exists(Settings.Default.MediaPlayerPath))
                    {
                        return;
                    }
                }
            }
            ProcessStartInfo mpvInfo = new ProcessStartInfo()
            {
                FileName = Settings.Default.MediaPlayerPath,
                Arguments = fullPath
            };
            Process.Start(mpvInfo);
        }

        private void Info_Click(object sender, RoutedEventArgs e)
        {
            object oldDataContext = infoGrid.DataContext;
            infoGrid.Tag = (sender as FrameworkElement)?.DataContext.GetType().Name;
            infoGrid.DataContext = (sender as FrameworkElement)?.DataContext;
            switch (infoGrid.Tag?.ToString())
            {
                case "Series":
                    descriptionBlock.DataContext = infoGrid.DataContext;
                    ratingGrid.DataContext = infoGrid.DataContext;
                    seasonsIC.SetBinding(ItemsControl.ItemsSourceProperty, new Binding("Seasons"));
                    seasonsGrid.Visibility = Visibility.Visible;
                    episodesGrid.Visibility = Visibility.Hidden;
                    peopleGrid.Visibility = Visibility.Hidden;
                    break;
                case "Movie":
                    descriptionBlock.DataContext = infoGrid.DataContext;
                    ratingGrid.DataContext = infoGrid.DataContext;
                    seasonsGrid.Visibility = Visibility.Hidden;
                    episodesGrid.Visibility = Visibility.Hidden;
                    peopleGrid.Visibility = Visibility.Visible;
                    break;
                case "Season":
                    descriptionBlock.DataContext = oldDataContext;
                    ratingGrid.DataContext = oldDataContext;
                    episodesIC.SetBinding(ItemsControl.ItemsSourceProperty, new Binding("Episodes"));
                    seasonsGrid.Visibility = Visibility.Hidden;
                    episodesGrid.Visibility = Visibility.Visible;
                    peopleGrid.Visibility = Visibility.Hidden;
                    break;
                case "Episode":
                    descriptionBlock.DataContext = infoGrid.DataContext;
                    ratingGrid.DataContext = infoGrid.DataContext;
                    seasonsGrid.Visibility = Visibility.Hidden;
                    episodesGrid.Visibility = Visibility.Hidden;
                    peopleGrid.Visibility = Visibility.Visible;
                    break;
            }
            infoGrid.Visibility = Visibility.Visible;
            _navigationTracker.Push(infoGrid.DataContext);
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
                _navigationTracker.Pop();
                infoGrid.DataContext = _navigationTracker.Peek();
                seasonsGrid.Visibility = Visibility.Visible;
                episodesGrid.Visibility = Visibility.Hidden;
                peopleGrid.Visibility = Visibility.Hidden;
                infoGrid.Tag = "Series";
            }
            else if (infoGrid.Tag.ToString() == "Episode")
            { // We were on Episode info screen, we go back to the associated season's info screen.
                _navigationTracker.Pop();
                infoGrid.DataContext = _navigationTracker.Peek();
                seasonsGrid.Visibility = Visibility.Hidden;
                episodesGrid.Visibility = Visibility.Visible;
                peopleGrid.Visibility = Visibility.Hidden;
                infoGrid.Tag = "Season";
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            StartRefreshAnimation();
            await GetDbFilesAsync();
            StopRefreshAnimation();
            UpdateEntries();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            MainWrapperGrid.Opacity = 0.1;
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
            MainWrapperGrid.Opacity = 1;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (AddGrid.IsVisible)
            {
                Storyboard sb = new Storyboard();

                DoubleAnimation spinAnimation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(150));
                BackEase backEase = new BackEase {EasingMode = EasingMode.EaseIn};
                spinAnimation.EasingFunction = backEase;
                Storyboard.SetTarget(spinAnimation, addButton.Template.FindName("btnImage", addButton) as Rectangle);
                Storyboard.SetTargetProperty(spinAnimation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
                sb.Children.Add(spinAnimation);

                DoubleAnimation slideAnimation = new DoubleAnimation(110, TimeSpan.FromMilliseconds(150));
                CubicEase cubicEase = new CubicEase {EasingMode = EasingMode.EaseInOut};
                slideAnimation.EasingFunction = cubicEase;
                Storyboard.SetTarget(slideAnimation, saveButton.Template.FindName("btnImage", saveButton) as Rectangle);
                Storyboard.SetTargetProperty(slideAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
                sb.Children.Add(slideAnimation);

                sb.Begin();

                AddGrid.Visibility = Visibility.Hidden;
                saveButton.Visibility = Visibility.Hidden;
                backButton.IsEnabled = true;
            }
            else
            {
                RootPath = "/files";
                Storyboard sb = new Storyboard();

                AddGrid.Visibility = Visibility.Visible;
                saveButton.Visibility = Visibility.Visible;
                backButton.IsEnabled = false;

                DoubleAnimation spinAnimation = new DoubleAnimation(45, TimeSpan.FromMilliseconds(150));
                BackEase backEase = new BackEase {EasingMode = EasingMode.EaseIn};
                spinAnimation.EasingFunction = backEase;
                Storyboard.SetTarget(spinAnimation, addButton.Template.FindName("btnImage", addButton) as Rectangle);
                Storyboard.SetTargetProperty(spinAnimation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
                sb.Children.Add(spinAnimation);

                DoubleAnimation slideAnimation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(150));
                CubicEase cubicEase = new CubicEase {EasingMode = EasingMode.EaseInOut};
                slideAnimation.EasingFunction = cubicEase;
                Storyboard.SetTarget(slideAnimation, saveButton.Template.FindName("btnImage", saveButton) as Rectangle);
                Storyboard.SetTargetProperty(slideAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
                sb.Children.Add(slideAnimation);

                sb.Begin();
                LoadFileBrowser(RootPath);
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            saveStatusGrid.Visibility = Visibility.Visible;
            SaveStatus = "Downloading metadata";
            Series seriesToAdd = await Series.CreateAsync(TvdbId);
            SaveStatus = "Connecting to server";
            if (ftpClient == null || !ftpClient.IsConnected)
            {
                ftpClient = new FtpClient(Settings.Default.FTP_Host, new System.Net.NetworkCredential(Settings.Default.FTP_Username, Settings.Default.FTP_Password));
                await ftpClient.AutoConnectAsync();
            }
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
            ftpClient.Dispose();
            seriesToAdd.Path = RootPath;
            _series.Add(seriesToAdd);
            SaveStatus = "Writing to file";
            File.WriteAllText("db_series.json", JsonConvert.SerializeObject(_series));
            SaveStatus = "Uploading to server";
            await ftpClient.UploadFileAsync("db_series.json", "/files/Çomarstream");
            SaveStatus = "Done!";
            UpdateEntries();
            Add_Click(sender, e);
        }

        private void FileBrowserListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {

            }
            else
            {
                RootPath = ((DirectoryListingItem)e.AddedItems[0]).FullName;
            }
        }

        private void ListViewItemDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            LoadFileBrowser(((DirectoryListingItem)((FrameworkElement)sender).DataContext).FullName);
        }

        private async void LoadFileBrowser(string directoryPath) //TODO Add refresh button to the Add menu
        {
            if (ftpClient == null || !ftpClient.IsConnected)
            {
                ftpClient = new FtpClient(Settings.Default.FTP_Host, new System.Net.NetworkCredential(Settings.Default.FTP_Username, Settings.Default.FTP_Password));
                await ftpClient.AutoConnectAsync();
            }
            await ftpClient.SetWorkingDirectoryAsync(directoryPath);
            List<DirectoryListingItem> directory = new List<DirectoryListingItem>();
            directory.Add(new DirectoryListingItem("..", "directory", directoryPath.Substring(0, directoryPath.LastIndexOf('/'))));
            foreach (FtpListItem ftpListItem in await ftpClient.GetListingAsync())
            {
                directory.Add(new DirectoryListingItem(ftpListItem.Name, ftpListItem.Type.ToString().ToLower(), ftpListItem.FullName));
            }

            FileBrowserListView.ItemsSource = directory;
        }

        private void StartRefreshAnimation()
        {
            if (refreshStoryboard == null)
            {
                refreshStoryboard = new Storyboard();

                DoubleAnimation spinAnimation = new DoubleAnimation(360, TimeSpan.FromMilliseconds(500));
                spinAnimation.RepeatBehavior = RepeatBehavior.Forever;
                Storyboard.SetTarget(spinAnimation, refreshButton.Template.FindName("btnImage", refreshButton) as Rectangle);
                Storyboard.SetTargetProperty(spinAnimation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
                refreshStoryboard.Children.Add(spinAnimation);

                refreshStoryboard.Begin(); 
            }
            else
            {
                refreshStoryboard.Resume();
            }
        }

        private void StopRefreshAnimation()
        {
            refreshStoryboard.Seek(TimeSpan.Zero, TimeSeekOrigin.BeginTime);
            refreshStoryboard.Pause();
        }
    }
}
