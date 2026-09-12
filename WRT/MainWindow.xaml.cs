using Renci.SshNet;
using System;
using System.Collections.Generic;
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

namespace WRT
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ShellStream globalStreamRead;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConnectToServer(object sender, RoutedEventArgs e)
        {
            string username = loginBox.Text;
            string password = passwordBox.Text;

            using (var client = new SshClient("192.168.1.1", username, password))
            {
                try
                {
                    client.Connect();
                    globalStreamRead = client.CreateShellStream("custom_term", 80, 24, 800, 600, 1024);
                    globalStreamRead.WriteLine("sh <(wget -O - https://raw.githubusercontent.com/StressOzz/Zapret-Manager/main/Zapret-Manager.sh)");
                    System.Threading.Thread.Sleep(3000);

                    string output = globalStreamRead.Read();
                    string[] lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                    ZapVer.Text = lines[43];
                    Proxy.Text = lines[44];
                    Hosts.Text = lines[45];
                    Strategy.Text = lines[46];
                }
                catch (Exception ex) { }

            }
        }

        private void MenuButtonPress(object sender, RoutedEventArgs e, string output)
        {
            try
            {
                globalStreamRead.WriteLine(output);
            }
            catch (Exception ex) { }
        }
    }
}
