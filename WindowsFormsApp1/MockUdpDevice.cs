using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class MockUdpDevice
{
    private const int DevicePort = 12345;

    public async Task StartAsync()
    {
        using (UdpClient udpServer = new UdpClient(DevicePort))
        {
            Console.WriteLine($"Mock device listening on port {DevicePort}...");
            while (true)
            {
                UdpReceiveResult received = await udpServer.ReceiveAsync();
                byte[] request = received.Buffer;

                Console.WriteLine($"Received: {BitConverter.ToString(request)} from {received.RemoteEndPoint}");

                // Simulate a response
                byte[] response = Encoding.ASCII.GetBytes("Mock_Response_OK");
                await udpServer.SendAsync(response, response.Length, received.RemoteEndPoint);
            }
        }
    }
}
