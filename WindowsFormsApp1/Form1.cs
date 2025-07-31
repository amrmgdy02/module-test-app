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
        int devicePort = 65500;
        string logFilePath = "error_log.txt";
        string validModulesFilePath = "valid_modules.txt";
        string broadcastAddress = "192.168.255.255";
        int totalPinsCount = 0;
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

        public async void completeTesting(UdpService service, string myIP)
        {
            ////////////////////////////// Get_Mac command ///////////////////////////
            byte[] getMacBytes = DeviceCommand.Commands["Get_Mac"];
            await udpClient.SendAsync(getMacBytes, getMacBytes.Length, broadcastAddress, devicePort);

            List<(byte[] Data, string SenderIP)> getMacResponses = await service.ReceiveMultipleResponsesAsync(udpClient, myIP);
            List<(byte[] Data, string SenderIP)> validResponses = new List<(byte[] Data, string SenderIP)>();

            if (getMacResponses.Count == 0)
            {
                string errorMessage = $"[{DateTime.Now}] Connection Lost: No response received from {broadcastAddress}:{devicePort}";
                File.AppendAllText(logFilePath, errorMessage + Environment.NewLine);
                MessageBox.Show("Get_Mac: Connection Lost: No response received.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        responseInfo.AppendLine($"Device: MAC = {mac}\nIP = {senderIP}");
                    }
                }
                MessageBox.Show(responseInfo.ToString(), "Devices Found");
            }

            ////////////////////////////////// Find_Last_and_Addr //////////////////////////////////
            byte[] FindLastAndAddrBytes = DeviceCommand.Commands["Find_Last_and_Addr"];
            await udpClient.SendAsync(FindLastAndAddrBytes, FindLastAndAddrBytes.Length, broadcastAddress, devicePort);
            List<(byte[] Data, string SenderIP)> receivedAcks = await service.ReceiveMultipleResponsesAsync(udpClient, myIP);

            if (receivedAcks.Count == 0)
            {
                string errorMessage = $"[{DateTime.Now}] Connection Lost: No response received from {broadcastAddress}:{devicePort}";
                File.AppendAllText(logFilePath, errorMessage + Environment.NewLine);
                MessageBox.Show("Find_Last_and_Addr: Connection Lost: No response received.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    MessageBox.Show("Find_Last_and_Addr: Connection Lost: No response received.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //return;
                }
            }

            /////////////////////////////////// Rlout_Low command ////////////////////////////////////
            int counter = 0;
            string currentModule = validResponses[0].SenderIP;
            bool isEnd = false;
            while (counter < validResponses.Count && !isEnd)
            {
                if (counter == validResponses.Count - 1)
                {
                    isEnd = true;
                }
                byte[] RloutBytes = DeviceCommand.Commands["Rlout_Low"];
                await udpClient.SendAsync(RloutBytes, RloutBytes.Length, currentModule, devicePort);

                List<(byte[] Data, string SenderIP)> RloutResponse = await service.ReceiveMultipleResponsesAsync(udpClient, myIP);

                if (RloutResponse.Count == 0)
                {
                    string errorMessage = $"[{DateTime.Now}] Connection Lost: No response received from {broadcastAddress}:{devicePort}";
                    File.AppendAllText(logFilePath, errorMessage + Environment.NewLine);
                    MessageBox.Show("Rlout_Low: Connection Lost: No response received from Rlout low.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //return;
                }
                else
                {
                    foreach ((byte[] data, string ip) in RloutResponse)
                    {
                        byte[] header = data.Take(2).ToArray();
                        if (data.Length == 4)
                        {
                            bool isAck = service.confirmCommand(header, "ACK");
                            bool validCommand = service.confirmCommand(data.Skip(2).Take(2).ToArray(), "Rlout_Low");
                        }
                        else if (data.Length == 2 && service.confirmCommand(header, "Find_Last_and_Addr_resp"))
                        {
                            currentModule = ip;
                            counter++;
                        }
                    }

                }
            }

            ///////////////////////////////// GET_SOCKETS command /////////////////////////////////
            byte[] getSocketsBytes = DeviceCommand.Commands["GET_SOCKETS"];
            await udpClient.SendAsync(getSocketsBytes, getSocketsBytes.Length, broadcastAddress, devicePort);
            // get sockets response
            List<(byte[] Data, string SenderIP)> getSocketsResponse = await service.ReceiveMultipleResponsesAsync(udpClient, myIP);
            List<(string SenderIP, int pinCount)> validPinCounts = new List<(string SenderIP, int pinCount)>();

            Dictionary<string, int> validPinCountsDict = new Dictionary<string, int>();
            List<(string SenderIP, int negativeDiode, int positiveDiode)> diodesIdxs = new List<(string SenderIP, int negativeDiode, int positiveDiode)>();

            if (getSocketsResponse.Count == 0)
            {
                string errorMessage = $"[{DateTime.Now}] Connection Lost: No response received from {broadcastAddress}:{devicePort}";
                File.AppendAllText(logFilePath, errorMessage + Environment.NewLine);
                MessageBox.Show("GET_SOCKETS: Connection Lost: No response received.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //return;
            }
            else
            {
                bool validDiodes = true;
                StringBuilder responseInfo = new StringBuilder();
                foreach (var (data, senderIP) in getSocketsResponse)
                {
                    if (data.Length != 4) continue;

                    byte[] header = data.Take(2).ToArray();
                    bool valid = service.confirmCommand(header, "SOCKETS_RES");

                    if (valid)
                    {
                        int negDiode = data[2];
                        int posDiode = data[3];
                        if (negDiode == 255)
                        {
                            string errorMessage = $"[{DateTime.Now}] Negative diode not found {senderIP}:{devicePort}";
                            File.AppendAllText(logFilePath, errorMessage + Environment.NewLine);
                            MessageBox.Show($"Negative diode not found for IP: {senderIP}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            validDiodes = false;
                        }
                        if (posDiode == 255)
                        {
                            string errorMessage = $"[{DateTime.Now}] Positive diode not found {broadcastAddress}:{devicePort}";
                            File.AppendAllText(logFilePath, errorMessage + Environment.NewLine);
                            MessageBox.Show("Positive diode not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            validDiodes = false;
                        }
                        int pinCount = posDiode - negDiode - 1;
                        validPinCounts.Add((senderIP, pinCount));
                        validPinCountsDict[senderIP] = pinCount;

                        diodesIdxs.Add((senderIP, negDiode, posDiode));
                        totalPinsCount += pinCount;
                        //MessageBox.Show("IP: " + senderIP + "\npin count =" + pinCount, "Pin count");
                    }
                }
                if (!validDiodes)
                {
                    return;
                }
                progressBar1.PerformStep();
            }

            ///////////////////////// End_addressing ////////////////////////////
            byte[] endAddressingBytes = DeviceCommand.Commands["End_addressing"];
            await udpClient.SendAsync(endAddressingBytes, endAddressingBytes.Length, broadcastAddress, devicePort);
            List<(byte[] Data, string SenderIP)> endAddressingAcks = await service.ReceiveMultipleResponsesAsync(udpClient, myIP);

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
                    return;
                }
                progressBar1.PerformStep();
            }

            ////////////////////////////////////// Learn Cnf //////////////////////////////////////
            int learnCnfCounter = 0;
            int startIdx = 0;
            int endIdx = 0;
            int currPinCount = 0;
            currentModule = validResponses[0].SenderIP;
            isEnd = false;
            int checkAcksount = 0;
            Dictionary<string, int> absIdxs = new Dictionary<string, int>();

            while (learnCnfCounter < validResponses.Count && !isEnd)
            {
                if (learnCnfCounter == validResponses.Count - 1)
                {
                    isEnd = true;
                }

                foreach ((string currIp, int pinCount) in validPinCounts)
                {
                    if (currIp == currentModule)
                    {
                        currPinCount = pinCount;
                        break;
                    }
                }
                byte[] learnCnfBytes = DeviceCommand.Commands["Learn Cnf"];
                byte[] temp = new byte[6];
                temp[0] = (byte)(startIdx >> 8);
                temp[1] = (byte)(startIdx & 0xFF);
                temp[2] = 0;
                temp[3] = (byte)currPinCount;
                temp[4] = 0;
                temp[5] = 0;
                byte[] fullLearnCnfPacket = learnCnfBytes.Concat(temp).ToArray();

                absIdxs[currentModule] = startIdx;

                await udpClient.SendAsync(fullLearnCnfPacket, fullLearnCnfPacket.Length, currentModule, devicePort);

                List<(byte[] Data, string SenderIP)> learnCnfResponse = await service.ReceiveMultipleResponsesAsync(udpClient, myIP);

                if (learnCnfResponse.Count == 0)
                {
                    string errorMessage = $"[{DateTime.Now}] Connection Lost: No response received from {broadcastAddress}:{devicePort}";
                    File.AppendAllText(logFilePath, errorMessage + Environment.NewLine);
                    MessageBox.Show("Learn Config: Connection Lost: No response received from Rlout low.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //return;
                }
                else
                {
                    foreach ((byte[] data, string ip) in learnCnfResponse)
                    {
                        byte[] header = data.Take(2).ToArray();
                        if (data.Length == 4)
                        {
                            bool isAck = service.confirmCommand(header, "ACK");
                            bool validCommand = service.confirmCommand(data.Skip(2).Take(2).ToArray(), "Learn Cnf");
                            if (isAck && validCommand)
                            {
                                MessageBox.Show("Ack from Learn Connfig with ip=" + ip);
                                learnCnfCounter++;
                                if (learnCnfCounter < validResponses.Count)
                                    currentModule = validResponses[learnCnfCounter].SenderIP;
                                startIdx += currPinCount + 2;
                            }

                        }

                    }
                    progressBar1.PerformStep();
                }
            }

            /////////////////////////////////////// Learn Pin ////////////////////////////////////
            //int learnPinCounter = 0;
            //currentModule = validResponses[0].SenderIP;
            //bool shortExists = false;
            //isEnd = false;

            //while (learnPinCounter < validResponses.Count && !isEnd)
            //{
            //    if (learnPinCounter == validResponses.Count - 1)
            //    {
            //        isEnd = true;
            //    }

            //    // Get pin count for current module
            //    currPinCount = 0;
            //    foreach ((string currIp, int pinCount) in validPinCounts)
            //    {
            //        if (currIp == currentModule)
            //        {
            //            currPinCount = pinCount;
            //            break;
            //        }
            //    }
            //    int currAbsIdx = -1;
            //    // Get absolute start index for current module
            //    if (absIdxs.ContainsKey(currentModule))
            //    {
            //        currAbsIdx = absIdxs[currentModule];
            //    }
            //    else
            //    {
            //        string errorMessage = $"[{DateTime.Now}] Unexpected Module: new module with unknown indices responded {currentModule}:{devicePort}";
            //        File.AppendAllText(logFilePath, errorMessage + Environment.NewLine);
            //        MessageBox.Show("Unexpected Module: new module with unknown indices responded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        return;
            //    }

            //    byte[] learnPinBytes = DeviceCommand.Commands["Learn Pin"];

            //    for (int i = currAbsIdx + 1; i <= currAbsIdx + currPinCount; i++)
            //    {
            //        byte[] temp = new byte[2];
            //        temp[0] = (byte)(i >> 8);
            //        temp[1] = (byte)(i & 0xFF);

            //        byte[] fullLearnPinPacket = learnPinBytes.Concat(temp).ToArray();

            //        await udpClient.SendAsync(fullLearnPinPacket, fullLearnPinPacket.Length, broadcastAddress, devicePort);

            //        List<(byte[] Data, string SenderIP)> learnPinResponse = await service.ReceiveMultipleResponsesAsync(udpClient, myIP);

            //        if (learnPinResponse.Count == 0)
            //        {
            //            string errorMessage = $"[{DateTime.Now}] Connection Lost: No response received for pin {i} from {currentModule}:{devicePort}";
            //            File.AppendAllText(logFilePath, errorMessage + Environment.NewLine);
            //            MessageBox.Show($"Learn Pin: No response for pin {i} from {currentModule}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //            continue; // Continue with next pin instead of breaking
            //        }
            //        // Process responses for this specific pin
            //        foreach ((byte[] data, string ip) in learnPinResponse)
            //        {
            //            byte[] header = data.Take(2).ToArray();
            //            bool isLearnPin = service.confirmCommand(header, "Learn Pin Res");

            //            if (isLearnPin)
            //            {
            //                if (isLearnPin && data.Length > 5)
            //                {
            //                    if (data[4] > 0)
            //                    {
            //                        byte[] shortsPins = data.Skip(5).ToArray();
            //                        List<int> shortedPinIndices = new List<int>();

            //                        for (int j = 0; j < shortsPins.Length; j += 2)
            //                        {
            //                            if (j + 1 >= shortsPins.Length) break;

            //                            int pinIndex = (shortsPins[j] << 8) | shortsPins[j + 1];
            //                            if (pinIndex == i)
            //                                continue;
            //                            shortedPinIndices.Add(pinIndex);
            //                        }

            //                        if (shortedPinIndices.Count > 0)
            //                        {
            //                            string pinList = string.Join(", ", shortedPinIndices);
            //                            string errorMessage = $"[{DateTime.Now}] Learn Pin: Pin {i} is shorted with pins: {pinList}";
            //                            File.AppendAllText(logFilePath, errorMessage + Environment.NewLine);
            //                            MessageBox.Show($"Pin {i} is shorted with pins: {pinList}", "Short Circuit", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //                            shortExists = true;
            //                        }

            //                    }
            //                }
            //            }
            //        }
            //    }
            //    learnPinCounter++;
            //    if (learnPinCounter < validResponses.Count)
            //        currentModule = validResponses[learnPinCounter].SenderIP;
            //}
            //progressBar1.PerformStep();

            /////////////////////////////// GET ZEROS /////////////////////////////////

            bool allProbed = false, endProbing = false;
            int currNumProbed = 0;

            Panel pinsCard = new Panel();
            pinsCard.Location = new Point(250, 20);
            pinsCard.Size = new Size(270, 420);
            pinsCard.BackColor = Color.FromArgb(36, 47, 61);
            pinsCard.BorderStyle = BorderStyle.None;
            pinsCard.Padding = new Padding(0);

            Label pinsTitle = new Label();
            pinsTitle.Text = $"Pins ({totalPinsCount})";
            pinsTitle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            pinsTitle.ForeColor = Color.FromArgb(0, 255, 200);
            pinsTitle.BackColor = Color.Transparent;
            pinsTitle.AutoSize = false;
            pinsTitle.TextAlign = ContentAlignment.MiddleCenter;
            pinsTitle.Dock = DockStyle.Top;
            pinsTitle.Height = 48;

            CheckedListBox pinsList = new CheckedListBox();
            pinsList.Location = new Point(20, 60);
            pinsList.Size = new Size(230, 320);
            pinsList.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            pinsList.BackColor = Color.FromArgb(52, 73, 94);
            pinsList.ForeColor = Color.White;
            pinsList.CheckOnClick = true;
            pinsList.BorderStyle = BorderStyle.None;
            pinsList.ItemHeight = 28;
            // Add event handler for automatic scrolling
            pinsList.ItemCheck += (sender, e) =>
            {
                if (e.NewValue == CheckState.Checked)
                {
                    // Use BeginInvoke to ensure the check state has been updated
                    this.BeginInvoke(new Action(() =>
                    {
                        // Scroll to the checked item
                        pinsList.TopIndex = e.Index;

                        // Alternative: Center the item in view if you prefer
                        // int visibleItems = pinsList.ClientSize.Height / pinsList.ItemHeight;
                        // int targetIndex = Math.Max(0, e.Index - (visibleItems / 2));
                        // pinsList.TopIndex = Math.Min(targetIndex, pinsList.Items.Count - visibleItems);
                    }));
                }
            };


            for (int i = 1; i <= totalPinsCount + validResponses.Count*2; i++)
                pinsList.Items.Add($"● Pin {i}");

            Panel accentBar = new Panel();
            accentBar.BackColor = Color.FromArgb(0, 255, 200);
            accentBar.Size = new Size(6, pinsCard.Height);
            accentBar.Location = new Point(0, 0);

            pinsCard.Controls.Add(pinsList);
            pinsCard.Controls.Add(pinsTitle);
            pinsCard.Controls.Add(accentBar);

            pinsCard.Controls.SetChildIndex(accentBar, 0);

            this.Controls.Add(pinsCard);

            // button to end probing
            Button endProbingButton = new Button();
            endProbingButton.Text = "End Probing";
            endProbingButton.Size = new Size(100, 40);
            endProbingButton.Location = new Point(20, 20);
            endProbingButton.BackColor = Color.FromArgb(46, 204, 113);
            endProbingButton.ForeColor = Color.White;
            endProbingButton.FlatStyle = FlatStyle.Flat;
            endProbingButton.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.Controls.Add(endProbingButton);

            endProbingButton.Click += async (s, err) =>
            {
                endProbingButton.Visible = false;
                endProbingButton.Visible = false;
                endProbing = true;
            };

            Dictionary<string, HashSet<int>> probedPins = new Dictionary<string, HashSet<int>>();
            foreach (var (ip, pinCount) in validPinCounts)
            {
                probedPins[ip] = new HashSet<int>();
            }

            while (!allProbed && !endProbing)
            {
                byte[] getZerosBytes = DeviceCommand.Commands["GET ZEROS"];
                await udpClient.SendAsync(getZerosBytes, getZerosBytes.Length, broadcastAddress, devicePort);
                List<(byte[] Data, string SenderIP)> getZerosResponse = await service.ReceiveMultipleResponsesAsync(udpClient, myIP);

                foreach (var (data, senderIP) in getZerosResponse)
                {
                    byte[] header = data.Take(2).ToArray();
                    bool valid = service.confirmCommand(header, "GET_ZEROS_RES");
                    if (!valid || !probedPins.ContainsKey(senderIP)) continue;

                    byte[] payload = data.Skip(2).ToArray();
                    int numZeros = payload[0];
                    for (int i = 0; i < numZeros * 2; i += 2)
                    {
                        int pinIndex = (payload[i + 1] << 8) | payload[i + 2];
                        int absIdx = absIdxs[senderIP];

                        if (!probedPins[senderIP].Contains(pinIndex))
                        {
                            currNumProbed++;
                            probedPins[senderIP].Add(pinIndex);
                            if (pinIndex >= 0 && pinIndex < pinsList.Items.Count)
                            {
                                pinsList.SetItemChecked(pinIndex, true);
                            }
                        }
                    }
                }
                // Check if all pins are probed
                if (currNumProbed == totalPinsCount) allProbed=true;
                
            }
            foreach (var (d, ip) in validResponses)
            {
                if (probedPins[ip].Count == validPinCountsDict[ip])
                {
                    string validModuleMessage = $"[{DateTime.Now}] Module {ip} passed the probing successfully.";
                    File.AppendAllText(validModulesFilePath, validModuleMessage + Environment.NewLine);
                    MessageBox.Show($"[{DateTime.Now}] Module {ip} passed the probing successfully.");
                }
            }
        }

        private async void StartTestingButton_Click(object sender, EventArgs e)
        {
            var service = new UdpService();

            // Get IP and MAC Addresses
            (string ipAddress, string macAddress) = service.GetHostIpAndMac();
            var hostMacIP_BYTES = service.ConvertIpAndMacToBytes(ipAddress, macAddress);

            // MessageBox.Show($"IP Address: {ipAddress}\nMAC Address: {macAddress}", "Host Info");
            //MessageBox.Show("IP & MAC (Hex): " + BitConverter.ToString(hostMacIP_BYTES).Replace("-", " "), "Packet Preview");

            //////////////////////////// 3F command
            byte[] hostmacipCommand = DeviceCommand.Commands["HOSTMACIP"];
            byte[] fullPacket = hostmacipCommand.Concat(hostMacIP_BYTES).ToArray();

            await udpClient.SendAsync(fullPacket, fullPacket.Length, broadcastAddress, devicePort);

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
            List<(byte[] Data, string SenderIP)> ledAckResponses = await service.ReceiveMultipleResponsesAsync(udpClient, ipAddress);
            if (ledAckResponses.Count == 0)
            {
                string errorMessage = $"[{DateTime.Now}] Connection Lost: No response received for LED ON. Port: {devicePort}";
                File.AppendAllText(logFilePath, errorMessage + Environment.NewLine);
                MessageBox.Show("LED ON: Connection Lost: No response received.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                ledAckResponses = await service.ReceiveMultipleResponsesAsync(udpClient, ipAddress);
                if (ledAckResponses.Count==0)
                {
                    string errorMessage = $"[{DateTime.Now}] Connection Lost: No response received for LED OFF. Port: {devicePort}";
                    File.AppendAllText(logFilePath, errorMessage + Environment.NewLine);
                    MessageBox.Show("LED OFF: Connection Lost: No response received.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                progressBar1.PerformStep();
                completeTesting(service, ipAddress);
            };

            ledBurnedButton.Click += async (s, err) =>
            {
                string errorMessage = $"[{DateTime.Now}] LED Burned: User reported failure from device at {deviceIP}";
                File.AppendAllText(logFilePath, errorMessage + Environment.NewLine);
                MessageBox.Show("Issue reported: LED is not functioning properly.", "Reported", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                validateLedButton.Visible = false;
                ledBurnedButton.Visible = false;
                completeTesting(service, ipAddress);
            };
        }
    }
}



