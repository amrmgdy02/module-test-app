using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {

        UdpClient udpClient;
        string deviceIP = "192.168.8.4";
        string subnet = "255.255.0.0";
        int devicePort = 65500;
        string logFilePath = "error_log.txt";
        string broadcastAddress = "192.168.255.255";
        public Form1()
        {
            InitializeComponent();
            udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 65500));
            comboBox1.Items.Add(1);
            comboBox1.Items.Add(2);
            comboBox1.Items.Add(3);
            comboBox1.Items.Add(5);
            comboBox1.Items.Add(50);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private async void StartTestingButton_Click(object sender, EventArgs e)
        {
            //var mockDevice = new MockUdpDevice();
            var service = new UdpService();

            // Get IP and MAC Addresses
            (string ipAddress, string macAddress) = service.GetHostIpAndMac();
            var hostMacIP_BYTES = service.ConvertIpAndMacToBytes(ipAddress, macAddress);

            MessageBox.Show($"IP Address: {ipAddress}\nMAC Address: {macAddress}", "Host Info");
            //MessageBox.Show("IP & MAC (Hex): " + BitConverter.ToString(hostMacIP_BYTES).Replace("-", " "), "Packet Preview");

            // 3f Packet command
            byte[] hostmacipCommand = DeviceCommand.Commands["HOSTMACIP"];
            byte[] fullPacket = hostmacipCommand.Concat(hostMacIP_BYTES).ToArray();

            // Convert bytes to hex string
            string hexString = BitConverter.ToString(fullPacket).Replace("-", "");
            // Or with spaces: string hexString = BitConverter.ToString(fullPacket).Replace("-", " ");

            // Convert hex string to bytes for transmission
            byte[] hexBytes = Encoding.UTF8.GetBytes(hexString);

            MessageBox.Show($"Sent bytes: {BitConverter.ToString(hexBytes).Replace("-", " ")}\nSending as hex string: {hexString}", "Packet Preview");
            MessageBox.Show("Full packet length: " + fullPacket.Length);

            await udpClient.SendAsync(fullPacket, fullPacket.Length, deviceIP, devicePort);

            // Loading
            using (var loadingForm = new Form())
            {
                loadingForm.StartPosition = FormStartPosition.CenterParent;
                loadingForm.FormBorderStyle = FormBorderStyle.None;
                loadingForm.Width = 200;
                loadingForm.Height = 100;
                var label = new Label
                {
                    Text = "LOADING...",
                    AutoSize = true,
                    Font = new Font("Segoe UI", 12),
                    Location = new Point(50, 40)
                };
                loadingForm.Controls.Add(label);
                loadingForm.Show();

                await Task.Delay(3000);
                loadingForm.Close();
            }

            // Send Led On
            byte[] ledOnBytes = DeviceCommand.Commands["LED ON"];
            Console.WriteLine(ledOnBytes);
            await udpClient.SendAsync(ledOnBytes, ledOnBytes.Length, broadcastAddress, devicePort);
            byte[] ledAckResponse = await service.ReceiveAsync(udpClient);
            if (ledAckResponse == null)
            {
               string errorMessage = $"[{DateTime.Now}] Connection Lost: No response received from {deviceIP}:{devicePort}";
               File.AppendAllText(logFilePath, errorMessage + Environment.NewLine);
               MessageBox.Show("Connection Lost: No response received.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
              return;
            }


            

            Button validateLedButton = new Button();
            validateLedButton.Text = "Validate";
            validateLedButton.Size = new Size(60, 40);
            validateLedButton.Location = new Point(20, 20);
            this.Controls.Add(validateLedButton);

            // button to press if led isn't turned on (Error)
            Button ledBurnedButton = new Button();
            ledBurnedButton.Text = "Burned Led";
            ledBurnedButton.Size = new Size(60, 40);
            ledBurnedButton.Location = new Point(20, 60);
            this.Controls.Add(ledBurnedButton);

            validateLedButton.Click += async (s, err) =>
            {
                validateLedButton.Visible = false;
                ledBurnedButton.Visible = false;
                // send led off command
                byte[] ledOffBytes = DeviceCommand.Commands["LED OFF"];
                Console.WriteLine(ledOffBytes);
                await udpClient.SendAsync(ledOffBytes, ledOffBytes.Length, broadcastAddress, devicePort);
                ledAckResponse = await service.ReceiveAsync(udpClient);
                if (ledAckResponse == null)
                {
                    string errorMessage = $"[{DateTime.Now}] Connection Lost: No response received from {deviceIP}:{devicePort}";
                    File.AppendAllText(logFilePath, errorMessage + Environment.NewLine);
                    MessageBox.Show("Connection Lost: No response received.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                progressBar1.PerformStep();
            };

            ledBurnedButton.Click += async (s, err) => 
            {
                string errorMessage = $"[{DateTime.Now}] LED Burned: User reported failure from device at {deviceIP}";
                File.AppendAllText(logFilePath, errorMessage + Environment.NewLine);
                MessageBox.Show("Issue reported: LED is not functioning properly.", "Reported", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                validateLedButton.Visible = false;
                ledBurnedButton.Visible = false;
            };
            //await tcs.Task;
            //get mac command
            byte[] getMacBytes = DeviceCommand.Commands["Get_Mac"];
            await udpClient.SendAsync(getMacBytes, getMacBytes.Length, broadcastAddress, devicePort);
            List<(byte[] Data, string SenderIP)> getMacResponses = await service.ReceiveMultipleResponsesAsync(udpClient);
            List<(byte[] Data, string SenderIP)> validResponses = new List<(byte[] Data, string SenderIP)>();
            if (getMacResponses.Count == 0)
            {
                string errorMessage = $"[{DateTime.Now}] Connection Lost: No response received from {broadcastAddress}:{devicePort}";
                File.AppendAllText(logFilePath, errorMessage + Environment.NewLine);
                MessageBox.Show("Connection Lost: No response received.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                StringBuilder responseInfo = new StringBuilder();
                foreach (var (data, senderIP) in getMacResponses)
                {
                    if (data.Length < 8) continue;

                    byte[] header = data.Take(2).ToArray();
                    bool valid = service.confirmCommand(header, "Get_Mac_Response");

                    if (valid)
                    {
                        string mac = BitConverter.ToString(data.Skip(2).Take(6).ToArray());
                        validResponses.Add((data.Skip(2).Take(6).ToArray(), senderIP));
                        responseInfo.AppendLine($"Device: MAC = {mac}, IP = {senderIP}");
                    }
                }
                MessageBox.Show(responseInfo.ToString(), "Devices Found");
            }




            // get sockets command
            //byte[] getSocketsBytes = DeviceCommand.Commands["GET_SOCKETS"];
            //await udpClient.SendAsync(getSocketsBytes, getSocketsBytes.Length, broadcastAddress, devicePort);
            //// response
            //byte[] getSocketsResponse = await service.ReceiveAsync(udpClient);
            
            //if (getSocketsResponse == null) // No response sent
            //{
            //    string errorMessage = $"[{DateTime.Now}] Connection Lost: No response received from {broadcastAddress}:{devicePort}";
            //    File.AppendAllText(logFilePath, errorMessage + Environment.NewLine);
            //    MessageBox.Show("Connection Lost: No response received.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    // return;
            //}
            //else // Got response
            //{
            //    byte[] socketResHeader = getSocketsResponse.Take(2).ToArray();
            //    byte negativeDiodeIndex = getSocketsResponse[2];
            //    byte positiveDiodeIndex = getSocketsResponse[3];
            //    bool getSocketsResponseReceived = service.confirmCommand(socketResHeader.Take(2).ToArray(), "GET_SOCKETS_RESPONSE");
            //}

        }

    }
}
