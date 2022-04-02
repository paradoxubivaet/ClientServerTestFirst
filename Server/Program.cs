using System;
using System.IO;
using System.Text;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            string ip = args[0];
            int tcpPort = Int32.Parse(args[1]);
            string folder = args[2];

            Server server = new Server(ip, tcpPort, folder);

            //Прием TcpClient 
            server.StartListen();
            Console.WriteLine("Сервер запущен и ожидает подключения");

            // Получение имени файла и udp порт
            byte[] receivedUdpFileName = server.TcpReceive();
            string stringUdpFileName = Encoding.Unicode.GetString(receivedUdpFileName);
            string[] infoUdpFileName = stringUdpFileName.Split(' ');

            string fileName = infoUdpFileName[0];
            int udpPort = Int32.Parse(infoUdpFileName[1]);
            Console.WriteLine($"Сервер получил имя файла и порт. Udp port: {udpPort}");

            // Получение размера файла и количество пакетов
            byte[] sizeNumberPackageSize = server.TcpReceive();
            string stringNumberPackageSize = Encoding.Unicode.GetString(sizeNumberPackageSize);
            string[] infoNumberPackageSize = stringNumberPackageSize.Split(' ');
            Console.WriteLine("Сервер получил размер файла и количество пакетов");

            // Создание UdpClient 
            server.OpenUdp(udpPort);

            int fileSize = Int32.Parse(infoNumberPackageSize[0]);
            int numberPackage = Int32.Parse(infoNumberPackageSize[1]);

            byte[] data = new byte[fileSize];

            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                for (int i = 0; i < numberPackage; i++)
                {
                    server.ServerInfo();
                    byte[] receivedPackage = server.UdpReceive();
                    Console.WriteLine($"Пакет длинной {receivedPackage.Length} был получен");

                    var headerSize = GetHeaderSize(receivedPackage);
                    var lengthData = receivedPackage.Length - headerSize;

                    //получаем id пакета
                    var raw = Encoding.UTF8.GetString(receivedPackage, 0, headerSize);
                    string[] splitRaw = raw.Split("\r\n\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    int id = Int32.Parse(splitRaw[0]);

                    //отправляем подтверждение о полученном пакете через TCP
                    string confirmation = $"{id} пакет был получен";
                    byte[] byteConfirmation = Encoding.Unicode.GetBytes(confirmation);
                    server.Send(byteConfirmation);

                    memoryStream.Write(receivedPackage, headerSize, lengthData);
                }
            }

            File.WriteAllBytes(fileName, data);
        }

        static int GetHeaderSize(byte[] package)
        {
            var pos = 0;

            for(int i = 0; i < package.Length; i++)
            {
                char c1 = (char)package[i];
                char c2 = (char)package[i+1];
                char c3 = (char)package[i+2];
                char c4 = (char)package[i+3];

                if(c1 == '\r' && c2 == '\n' && c3 == '\r' && c4 == '\n')
                {
                    pos = i + 4;
                    break;
                }
            }

            return pos;
        }

    }
}
