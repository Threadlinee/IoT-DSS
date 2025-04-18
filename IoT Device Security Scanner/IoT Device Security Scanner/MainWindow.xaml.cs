using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;

namespace IoTDeviceSecurityScanner
{
    public partial class MainWindow : Window
    {
        private readonly Dictionary<int, string> riskyPorts = new Dictionary<int, string>
        {
            { 21, "FTP - Unencrypted file transfer" },
            { 23, "Telnet - Unsecured remote login" },
            { 3389, "RDP - Remote Desktop Protocol" },
            { 445, "SMB - Windows File Sharing" },
            { 5900, "VNC - Virtual Network Computing" }
        };

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void btnScan_Click(object sender, RoutedEventArgs e)
        {
            txtResults.Text = "🔍 Scanning started...\n";
            string ip = txtIP.Text.Trim();

            int[] portsToScan = (chkFullScan.IsChecked == true)
                ? GenerateFullPortRange()
                : new int[] { 21, 22, 23, 80, 443, 445, 3389, 8080, 8443, 5900 };

            foreach (int port in portsToScan)
            {
                bool isOpen = await IsPortOpen(ip, port);
                if (isOpen)
                {
                    string warning = riskyPorts.ContainsKey(port) ? $" ⚠️ {riskyPorts[port]}" : "";
                    txtResults.Text += $"✅ Port {port} is OPEN{warning}\n";
                }
            }

            txtResults.Text += "\n✅ Scan complete.";
        }

        private int[] GenerateFullPortRange()
        {
            int[] ports = new int[65535];
            for (int i = 0; i < ports.Length; i++)
            {
                ports[i] = i + 1;
            }
            return ports;
        }

        private async Task<bool> IsPortOpen(string ip, int port)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    var connectTask = client.ConnectAsync(ip, port);
                    var timeoutTask = Task.Delay(400); // timeout in ms

                    if (await Task.WhenAny(connectTask, timeoutTask) == connectTask)
                        return client.Connected;
                }
            }
            catch
            {
            }
            return false;
        }
    }
}
