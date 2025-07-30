using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class UdpService
{
    private int localPort = 65500;
    private int devicePort = 65500;

    public async Task<string> SendCommandAsync(string commandName, string deviceIP)
    {
        if (!DeviceCommand.Commands.TryGetValue(commandName, out byte[] commandBytes))
            return $"Unknown command: {commandName}";

        using (UdpClient udpClient = new UdpClient(localPort))
        {
            await SendAsync(udpClient, commandBytes, deviceIP, devicePort);

            byte[] response = await ReceiveAsync(udpClient);
            if (response != null)
            {
                return $"Response ({response.Length} bytes): {BitConverter.ToString(response)}";
            }
            else
            {
                return "Timeout: No response.";
            }
        }
    }

    public async Task SendAsync(UdpClient client, byte[] packet, string ipAddress, int port)
    {
        await client.SendAsync(packet, packet.Length, ipAddress, port);
    }

    public async Task<List<(byte[] Data, string SenderIP)>> ReceiveMultipleResponsesAsync(UdpClient client, int timeoutMs = 500)
    {
        List<(byte[] Data, string SenderIP)> responses = new List<(byte[] Data, string SenderIP)>();
        DateTime endTime = DateTime.Now.AddMilliseconds(timeoutMs);

        while (DateTime.Now < endTime)
        {
            if (client.Available > 0)
            {
                var result = await client.ReceiveAsync();
                string senderIP = result.RemoteEndPoint.Address.ToString();
                responses.Add((result.Buffer, senderIP));
            }
            await Task.Delay(100); 
        }
        return responses;
    }

    public async Task<byte[]> ReceiveAsync(UdpClient client, int timeoutMs = 5000)
    {
        try
        {
            var receiveTask = client.ReceiveAsync();
            var completedTask = await Task.WhenAny(receiveTask, Task.Delay(timeoutMs));

            if (completedTask == receiveTask)
            {
                // Await the completed task to get its result or catch any exceptions it threw
                UdpReceiveResult result = await receiveTask;
                return result.Buffer;
            }
            else
            {
                // Timeout
                return null;
            }
        }
        catch (Exception ex)
        {
            //LogError($"ReceiveAsync Exception: {ex.Message}\n{ex.StackTrace}");
            return null;
        }
    }

    public byte[] ConvertIpAndMacToBytes(string ipAddress, string macAddress)
    {
        byte[] ipBytes = ipAddress.Split('.')
            .Select(part => byte.Parse(part))
            .ToArray();

        byte[] macBytes = macAddress
            .Split(new[] { '-', ':' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(part => Convert.ToByte(part, 16))
            .ToArray();

        return ipBytes.Concat(macBytes).ToArray();
    }


    public (string ipAddress, string macAddress) GetHostIpAndMac()
    {
        string ipAddress = "";
        string macAddress = "";

        foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.OperationalStatus == OperationalStatus.Up &&
                nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            {
                var ipProps = nic.GetIPProperties();
                var ip = ipProps.UnicastAddresses
                    .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(a => a.Address.ToString())
                    .FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(ip))
                {
                    ipAddress = ip;
                    macAddress = BitConverter.ToString(nic.GetPhysicalAddress().GetAddressBytes());
                    break;
                }
            }
        }

        return (ipAddress, macAddress);
    }

    public bool confirmCommand(byte[] packet, string command)
    {
        return DeviceCommand.Commands[command].SequenceEqual(packet);
    }
}
