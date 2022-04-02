using System;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class Server
    {
        public IPAddress IpAddress { get; set; }
        public int TcpPort { get; set; }
        public string StorageFolder { get; set; }

        private NetworkStream networkStream;
        private TcpListener tcpListener;
        private TcpClient tcpClient; 
        private UdpClient udpClient;
        private int udpPort;

        public Server(string ip, int tcpPort, string folder)
        {
            IpAddress = IPAddress.Parse(ip);
            TcpPort = tcpPort;
            StorageFolder = folder;

            tcpListener = new TcpListener(IpAddress, tcpPort);
        }

        public void StartListen()
        {
            tcpListener.Start();
            tcpClient = tcpListener.AcceptTcpClient();
            networkStream = tcpClient.GetStream();
        }

        public void Send(byte[] data)
        {
            networkStream.Write(data);
        }

        public byte[] TcpReceive()
        {
            byte[] receivedData = new byte[128];
            networkStream.Read(receivedData);

            return receivedData;
        }

        public byte[] UdpReceive()
        {
            var ipEndPoint = new IPEndPoint(IpAddress, udpPort);
            byte[] receivedData = udpClient.Receive(ref ipEndPoint);

            return receivedData;
        }

        public void OpenUdp(int port)
        {
            udpPort = port;
            udpClient = new UdpClient(port);
        }

        public void ServerInfo()
        {
            Console.WriteLine($"Tcp Local: {tcpClient.Client.LocalEndPoint}. Tcp Remote: {tcpClient.Client.RemoteEndPoint}\n\r" +
                              $"Udp Local: {udpClient.Client.LocalEndPoint}. Udp Remote: {udpClient.Client.RemoteEndPoint}");
        }
    }
}
