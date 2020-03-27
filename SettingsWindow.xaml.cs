using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Comarstream.Annotations;

namespace Comarstream
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : INotifyPropertyChanged
    {
        private string _hostname;
        private string _username;
        private string _password;
        private string _mediaPlayerPath;
        private string _token;

        public string Hostname
        {
            get => _hostname;
            set
            {
                if (value == _hostname) return;
                _hostname = value;
                OnPropertyChanged();
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                if (value == _username) return;
                _username = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (value == _password) return;
                _password = value;
                OnPropertyChanged();
            }
        }

        public string MediaPlayerPath
        {
            get => _mediaPlayerPath;
            set
            {
                if (value == _mediaPlayerPath) return;
                _mediaPlayerPath = value;
                OnPropertyChanged();
            }
        }

        public string Token
        {
            get => _token;
            set
            {
                if (value == _token) return;
                _token = value;
                OnPropertyChanged();
            }
        }

        public SettingsWindow()
        {
            InitializeComponent();
            Settings.Default.Reload();
            this.Hostname = Settings.Default.FTP_Host;
            this.Username = Settings.Default.FTP_Username;
            this.Password = Settings.Default.FTP_Password;
            this.MediaPlayerPath = Settings.Default.MediaPlayerPath;
            this.Token = Settings.Default.TVDB_Token;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Hostname) || string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                this.DialogResult = false;
            }
            else
            {
                this.DialogResult = true;
            }
            Settings.Default.FTP_Host = Hostname;
            Settings.Default.FTP_Username = Username;
            Settings.Default.FTP_Password = Password;
            Settings.Default.FTP_Path = $"ftp://{Username}:{Password}@{Hostname}/";
            Settings.Default.MediaPlayerPath = MediaPlayerPath;
            Settings.Default.TVDB_Token = Token;
            Settings.Default.Save();
            this.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
