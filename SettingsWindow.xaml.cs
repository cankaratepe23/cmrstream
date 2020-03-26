using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Comarstream
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public string Hostname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }

        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.FTP_Host = Hostname;
            Settings.Default.FTP_Username = Username;
            Settings.Default.FTP_Password = Password;
            Settings.Default.FTP_Path = String.Format("ftp://{0}:{1}@{2}/", Username, Password, Hostname);
            Settings.Default.TVDB_Token = Token;
            Settings.Default.Save();
            this.Close();
        }
    }
}
