using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    public class Client
    {
        public IPAddress IpAddress { get; set; }
        public int TcpPort { get; set; }
        public int UdpPort { get; set; }
        public string FileName { get; set; }
        public int Timeout { get; set; }

        private NetworkStream networkStream;
        private TcpClient tcpClient;
        private UdpClient udpClient;

        public Client(string ip, int tcpPort, int udpPort, string fileName, int timeout)
        {
            IpAddress = IPAddress.Parse(ip);
            TcpPort = tcpPort;
            UdpPort = udpPort;
            FileName = fileName;
            Timeout = timeout;

            udpClient = new UdpClient(ip ,udpPort);
            tcpClient = new TcpClient(ip, tcpPort);
            tcpClient.ReceiveTimeout = Timeout;

            networkStream = tcpClient.GetStream();
        }

        public void Start()
        {
            byte[] message = Encoding.Unicode.GetBytes($"{FileName} {UdpPort}");

            TcpSend(message);
        }

        public void UdpSend(byte[] data)
        {
            udpClient.Send(data, data.Length);
        }

        public void TcpSend(byte[] data)
        {
            networkStream.Write(data, 0, data.Length);
        }

        public byte[] Receive()
        {
            byte[] receivedData = null;
            networkStream.Read(receivedData);
            return receivedData;
        }

        public void CloseConnection()
        {
            networkStream.Close();
            tcpClient.Close();
        }

        //Использовал этот метод для проверки соединения по TCP
        public void CheckConnection()
        {
            if (tcpClient.Connected)
                Console.WriteLine("Есть подключение");
            else
                Console.WriteLine("Подключение разорвано");
        }

        // Использовал этот метод, чтобы проверить корректно ли установлены EndPoint's 
        public void ClientInfo()
        {
            Console.WriteLine($"Tcp Local: {tcpClient.Client.LocalEndPoint}. Tcp Remote: {tcpClient.Client.RemoteEndPoint}\n\r" +
                              $"Udp Local: {udpClient.Client.LocalEndPoint}. Udp Remote: {udpClient.Client.RemoteEndPoint}");
        }
    }
}
