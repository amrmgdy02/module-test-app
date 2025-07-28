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
    internal class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        public static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse
        );
    }

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
            var service = new UdpService();

            // Get IP and MAC Addresses
            (string ipAddress, string macAddress) = service.GetHostIpAndMac();
            var hostMacIP_BYTES = service.ConvertIpAndMacToBytes(ipAddress, macAddress);

            // MessageBox.Show($"IP Address: {ipAddress}\nMAC Address: {macAddress}", "Host Info");
            //MessageBox.Show("IP & MAC (Hex): " + BitConverter.ToString(hostMacIP_BYTES).Replace("-", " "), "Packet Preview");

            // 3F command
            byte[] hostmacipCommand = DeviceCommand.Commands["HOSTMACIP"];
            byte[] fullPacket = hostmacipCommand.Concat(hostMacIP_BYTES).ToArray();

            await udpClient.SendAsync(fullPacket, fullPacket.Length, deviceIP, devicePort);

            // Loading
            using (var loadingForm = new Form())
            {
                loadingForm.StartPosition = FormStartPosition.CenterParent;
                loadingForm.FormBorderStyle = FormBorderStyle.None;
                loadingForm.BackColor = Color.FromArgb(36, 47, 61);
                loadingForm.Width = 250;
                loadingForm.Height = 120;
                loadingForm.ShowInTaskbar = false;
                loadingForm.TopMost = true;

                loadingForm.Region = System.Drawing.Region.FromHrgn(
                    NativeMethods.CreateRoundRectRgn(0, 0, loadingForm.Width, loadingForm.Height, 20, 20));

                var label = new Label
                {
                    Text = "⏳ Loading...",
                    AutoSize = true,
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 255, 200),
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point((loadingForm.Width - 160) / 2, 30)
                };

                var progress = new ProgressBar
                {
                    Style = ProgressBarStyle.Marquee,
                    MarqueeAnimationSpeed = 30,
                    Size = new Size(160, 15),
                    Location = new Point((loadingForm.Width - 160) / 2, 70),
                    BackColor = Color.FromArgb(52, 73, 94),
                    ForeColor = Color.FromArgb(0, 255, 200)
                };

                loadingForm.Controls.Add(label);
                loadingForm.Controls.Add(progress);

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

            // button to press if led turned on
            Button validateLedButton = new Button();
            validateLedButton.Text = "Validate";
            validateLedButton.Size = new Size(100, 40);
            validateLedButton.Location = new Point(20, 20);
            validateLedButton.BackColor = Color.FromArgb(46, 204, 113);
            validateLedButton.ForeColor = Color.White;
            validateLedButton.FlatStyle = FlatStyle.Flat;
            validateLedButton.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.Controls.Add(validateLedButton);

            // button to press if led isn't turned on (Error)
            Button ledBurnedButton = new Button();
            ledBurnedButton.Text = "Burned Led";
            ledBurnedButton.Size = new Size(100, 40);
            ledBurnedButton.Location = new Point(20, 70);
            ledBurnedButton.BackColor = Color.FromArgb(231, 76, 60);
            ledBurnedButton.ForeColor = Color.White;
            ledBurnedButton.FlatStyle = FlatStyle.Flat;
            ledBurnedButton.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.Controls.Add(ledBurnedButton);

            validateLedButton.Click += async (s, err) =>
            {
                validateLedButton.Visible = false;
                ledBurnedButton.Visible = false;
                // LED OFF command
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

            ////////////////////////////// Get_Mac command ///////////////////////////
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
                    if (data.Length != 8) continue;

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

            ////////////////////////////////// Find_Last_and_Addr //////////////////////////////////
            byte[] FindLastAndAddrBytes = DeviceCommand.Commands["Find_Last_and_Addr"];
            await udpClient.SendAsync(FindLastAndAddrBytes, FindLastAndAddrBytes.Length, broadcastAddress, devicePort);
            List<(byte[] Data, string SenderIP)> receivedAcks = await service.ReceiveMultipleResponsesAsync(udpClient);

            if (receivedAcks.Count == 0)
            {
                string errorMessage = $"[{DateTime.Now}] Connection Lost: No response received from {broadcastAddress}:{devicePort}";
                File.AppendAllText(logFilePath, errorMessage + Environment.NewLine);
                MessageBox.Show("Connection Lost: No response received.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //return;
            }
            else
            {
                int acksCount = 0;
                StringBuilder responseInfo = new StringBuilder();
                foreach (var (data, senderIP) in receivedAcks)
                {
                    if (data.Length != 4) continue;

                    byte[] header = data.Take(2).ToArray();
                    bool isAck = service.confirmCommand(header, "ACK");
                    bool validCommand = service.confirmCommand(data.Skip(2).Take(2).ToArray(), "Find_Last_and_Addr");

                    if (isAck && validCommand)
                    {
                        acksCount += 1;
                    }
                }
                // compare acksCount to length of validResponses
                if (acksCount != validResponses.Count)
                {
                    string errorMessage = $"[{DateTime.Now}] Connection Lost: Not all modules respond to find last and addr {broadcastAddress}:{devicePort}";
                    File.AppendAllText(logFilePath, errorMessage + Environment.NewLine);
                    MessageBox.Show("Connection Lost: No response received.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //return;
                }
            }

            /////////////////////////////////// Rlout_Low command ////////////////////////////////////
            foreach (var (data, moduleIP) in validResponses)
            {
                byte[] RloutBytes = DeviceCommand.Commands["Rlout_Low"];
                await udpClient.SendAsync(RloutBytes, RloutBytes.Length, moduleIP, devicePort);
                byte[] ackResponse = await service.ReceiveAsync(udpClient);

                if (ackResponse == null)
                {
                    string errorMessage = $"[{DateTime.Now}] Connection Lost: No response received from {broadcastAddress}:{devicePort}";
                    File.AppendAllText(logFilePath, errorMessage + Environment.NewLine);
                    MessageBox.Show("Connection Lost: No response received from Rlout low.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //return;
                }
                if (ackResponse.Length != 4)
                {
                    string errorMessage = $"[{DateTime.Now}] Unexpected Command: Expected ack {broadcastAddress}:{devicePort}";
                    File.AppendAllText(logFilePath, errorMessage + Environment.NewLine);
                    MessageBox.Show("Unexpected Command: Expected ack.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //return;
                }
            }


            ///////////////////////////////// GET_SOCKETS command /////////////////////////////////
            byte[] getSocketsBytes = DeviceCommand.Commands["GET_SOCKETS"];
            await udpClient.SendAsync(getSocketsBytes, getSocketsBytes.Length, broadcastAddress, devicePort);
            // get sockets response
            List<(byte[] Data, string SenderIP)> getSocketsResponse = await service.ReceiveMultipleResponsesAsync(udpClient);
            List<(int pinCount, string SenderIP)> validPinCounts = new List<(int pinCount, string SenderIP)>();

            if (getSocketsResponse.Count == 0)
            {
                string errorMessage = $"[{DateTime.Now}] Connection Lost: No response received from {broadcastAddress}:{devicePort}";
                File.AppendAllText(logFilePath, errorMessage + Environment.NewLine);
                MessageBox.Show("Connection Lost: No response received.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //return;
            }
            else
            {
                StringBuilder responseInfo = new StringBuilder();
                foreach (var (data, senderIP) in getSocketsResponse)
                {
                    if (data.Length != 4) continue;

                    byte[] header = data.Take(2).ToArray();
                    bool valid = service.confirmCommand(header, "SOCKETS_RES");

                    if (valid)
                    { 
                        int negDiodeCount = data[2]; 
                        int posDiodeCount = data[3];
                        if (negDiodeCount ==255)
                        {
                            string errorMessage = $"[{DateTime.Now}] Negative diode not found {broadcastAddress}:{devicePort}";
                            File.AppendAllText(logFilePath, errorMessage + Environment.NewLine);
                            MessageBox.Show("Negative diode not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        if (posDiodeCount == 255)
                        {
                            string errorMessage = $"[{DateTime.Now}] Positive diode not found {broadcastAddress}:{devicePort}";
                            File.AppendAllText(logFilePath, errorMessage + Environment.NewLine);
                            MessageBox.Show("Positive diode not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        int pinCount= posDiodeCount-negDiodeCount-1;
                        validPinCounts.Add((pinCount, senderIP));
                        MessageBox.Show("IP: " + senderIP + "\npin count =" + pinCount, "Pin count");
                    }
                }
            }

            ///////////////////////// End_addressing ////////////////////////////
            byte[] endAddressingBytes = DeviceCommand.Commands["End_addressing"];
            await udpClient.SendAsync(endAddressingBytes, endAddressingBytes.Length, broadcastAddress, devicePort);
            List<(byte[] Data, string SenderIP)> endAddressingAcks = await service.ReceiveMultipleResponsesAsync(udpClient);

            if (endAddressingAcks.Count == 0)
            {
                string errorMessage = $"[{DateTime.Now}] Connection Lost: No response received from {broadcastAddress}:{devicePort} after endAddressing";
                File.AppendAllText(logFilePath, errorMessage + Environment.NewLine);
                MessageBox.Show("Connection Lost: No response received.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //return;
            }
            else
            {
                StringBuilder responseInfo = new StringBuilder();
                int acksCount = 0;
                foreach (var (data, senderIP) in endAddressingAcks)
                {
                    if (data.Length != 4) continue;

                    byte[] header = data.Take(2).ToArray();
                    bool isAck = service.confirmCommand(header, "ACK");
                    bool validCommand = service.confirmCommand(data.Skip(2).Take(2).ToArray(), "End_addressing");

                    if (isAck && validCommand)
                    {
                        acksCount += 1;
                    }
                }
                // compare acksCount to length of validResponses
                if (acksCount != validResponses.Count)
                {
                    string errorMessage = $"[{DateTime.Now}] Connection Lost: Not all modules respond to end addressing {broadcastAddress}:{devicePort}";
                    File.AppendAllText(logFilePath, errorMessage + Environment.NewLine);
                    MessageBox.Show("Connection Lost: Not all modules respond to end addressing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //return;
                }
            }
        }
    }
}
