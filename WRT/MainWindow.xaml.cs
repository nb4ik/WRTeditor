using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using static System.Net.Mime.MediaTypeNames;

namespace WRT
{
    public partial class MainWindow : Window
    {
        private SshClient _sshClient;
        private ShellStream _globalStreamRead;
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void ConnectToServer(object sender, RoutedEventArgs e)
        {
            string username = loginBox.Text;
            string password = passwordBox.Text;

            if (sender is System.Windows.Controls.Button connectButton)
            {
                connectButton.IsEnabled = false;
            }

            DisconnectSsh();

            try
            {
                _sshClient = new SshClient("192.168.1.1", username, password);

                await Task.Run(() => _sshClient.Connect());

                _globalStreamRead = _sshClient.CreateShellStream("custom_term", 80, 24, 800, 600, 1024);

                _globalStreamRead.WriteLine("sh <(wget -O - https://raw.githubusercontent.com/StressOzz/Zapret-Manager/main/Zapret-Manager.sh)");

                var menuPattern = new Regex(@"выбор|пункт|[:\]>]\s*$", RegexOptions.IgnoreCase);

                string output = await _globalStreamRead.ExpectAsync(menuPattern, TimeSpan.FromSeconds(15));

                string ansiPattern = @"\x1B(?:[@-Z\\-_]|\[[0-?]*[ -/]*[@-~])";
                string cleanText = Regex.Replace(output, ansiPattern, string.Empty);

                File.WriteAllText("test.txt", cleanText);

                string[] lines = cleanText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                string targetZap = lines.FirstOrDefault(l => l.Contains("Zapret:              "));
                ZapVer.Text = targetZap != null && targetZap.Length >= 21 ? targetZap.Substring(21).Trim() : "Не найдено";

                string targetProxy = lines.FirstOrDefault(l => l.Contains("TG WS Proxy"));
                Proxy.Text = targetProxy != null && targetProxy.Length >= 21 ? targetProxy.Substring(21).Trim() : "Не найдено";

                string targetHosts = lines.FirstOrDefault(l => l.Contains("hosts"));
                Hosts.Text = targetHosts != null && targetHosts.Length >= 21 ? targetHosts.Substring(21).Trim() : "Не найдено";

                string targetStrategy = lines.FirstOrDefault(l => l.Contains("Стратегия"));
                Strategy.Text = targetStrategy != null && targetStrategy.Length >= 21 ? targetStrategy.Substring(21).Trim() : "Не найдено";

                

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения или выполнения: {ex.Message}");
                DisconnectSsh();
            }
            finally
            {
                if (sender is System.Windows.Controls.Button connectBtn)
                {
                    connectBtn.IsEnabled = true;
                }
            }

            //отрисовка кнопок меню

        }
        private void DisconnectSsh()
        {
            _globalStreamRead?.Dispose();
            _globalStreamRead = null;

            if (_sshClient != null)
            {
                if (_sshClient.IsConnected) _sshClient.Disconnect();
                _sshClient.Dispose();
                _sshClient = null;
            }
        }
        private void MenuButtonPress(object sender, RoutedEventArgs e, string output)
        {
            try
            {
                _globalStreamRead.WriteLine(output);
            }
            catch (Exception ex) { }
        }
    }
}
