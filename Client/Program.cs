using System;
using System.IO;
using System.Text;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            //const int SizePackage = 8192;

            string ip = args[0];
            int tcpPort = Int32.Parse(args[1]);
            int udpPort = Int32.Parse(args[2]);
            string fileName = args[3];
            int timeout = Int32.Parse(args[4]);

            Client client = new Client(ip, tcpPort, udpPort, fileName, timeout);

            FileInfo fileInfo = new FileInfo(fileName);

            int fileSize = (int)fileInfo.Length;
            byte[] fileByte = new byte[fileSize];

            string sendFileNameUdpPort = $"{fileName} {udpPort}";
            Console.WriteLine($"Серверу отправится udp port: {udpPort}");
            byte[] FileNameUdpPort = ToByteArray(sendFileNameUdpPort);

            fileByte = File.ReadAllBytes(fileName);

            //Отправляем серверу имя файла и udp port 
            client.TcpSend(FileNameUdpPort);
            Console.WriteLine("Серверу отправлены имя и udp port");

            int numberPackages = DataPackage.GetNumberPackages(fileInfo);

            //Отправляем через TCP размер файла и количество пакетов
            string sendFileSizeNumberPackage = $"{fileSize} {numberPackages}";
            byte[] FileSizeNumberPackage = ToByteArray(sendFileSizeNumberPackage);
            client.TcpSend(FileSizeNumberPackage);
            Console.WriteLine("Серверу отправлены размер файла и количество пакетов");

            client.ClientInfo();

            SendingFile(fileInfo, numberPackages, client, timeout);

            Console.WriteLine($"Пакетов отправлено: {numberPackages}. Файл был успешно отправлен. Закрываем соединение");
            client.CloseConnection();
            Console.ReadLine();
        }

        static byte[] ToByteArray(string data)
        {
            return Encoding.Unicode.GetBytes(data);
        }

        static void SendingFile(FileInfo fileInfo, int numberPackages, Client client, int timeout)
        {
            for (int i = 0; i < numberPackages; i++)
            {
                byte[] packageToSend = DataPackage.GetPackage(fileInfo, i);
                
                again:
                client.UdpSend(packageToSend);
                Console.WriteLine($"Пакет длинной {packageToSend.Length} был отправлен");
                byte[] confirmation = null;

                //ожидаем подтверждения 
                try
                {
                    client.CheckConnection();
                    confirmation = client.Receive();
                }
                catch(IOException ex)
                {
                    goto again;
                }

                if(confirmation != null)
                {
                    Console.WriteLine($"Пакет №{i} был успешно доставлен. Доставлено {i} из {numberPackages}");
                }
            }
        }
    }
}
