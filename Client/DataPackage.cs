using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class DataPackage
    {
        const int packageSize = 8192;

        public static byte[] GetPackage(FileInfo info, int id)
        {
            byte[] file = File.ReadAllBytes(info.Name);
            byte[] dataPackage = new byte[packageSize];

            using(MemoryStream ms = new MemoryStream())
            {
                WriteString(ms, $"{id}\r\n\r\n");
                if (file.Length > packageSize)
                {
                    var offset = id * (packageSize - 10);
                    var count = packageSize - 10;

                    if (offset + count > info.Length)
                        count = (int)info.Length - offset;

                    ms.Write(file, offset, count);
                }
                else if(file.Length < packageSize)
                {
                    ms.Write(file);
                }

                dataPackage = ms.ToArray();
            }
            return dataPackage;
        }

        public static int GetNumberPackages(FileInfo info)
        {
            return (int)Math.Ceiling((double)info.Length/packageSize);
        }

        private static void WriteString(MemoryStream ms, string s)
        {
            byte[] data = Encoding.UTF8.GetBytes(s);
            ms.Write(data);
        }
    }
}
