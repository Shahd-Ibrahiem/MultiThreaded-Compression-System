using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CompressionServer
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpListener server = new TcpListener(IPAddress.Any, 5000);

            server.Start();

            Console.WriteLine("Server started...");
            Console.WriteLine("Waiting for clients...");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();

                Console.WriteLine("Client connected!");

                Thread clientThread = new Thread(() => HandleClient(client));
                clientThread.Start();
            }
        }

        static void HandleClient(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();

                // Receive file size
                byte[] sizeBytes = new byte[8];
                stream.Read(sizeBytes, 0, 8);

                long fileSize = BitConverter.ToInt64(sizeBytes, 0);

                Console.WriteLine("Receiving file size: " + fileSize);

                // Receive file data
                byte[] fileBytes = new byte[fileSize];

                int totalRead = 0;

                while (totalRead < fileSize)
                {
                    int read = stream.Read(
                        fileBytes,
                        totalRead,
                        (int)(fileSize - totalRead));

                    if (read == 0)
                        break;

                    totalRead += read;
                }

                Console.WriteLine("File received successfully!");

                // Compress file
                byte[] compressedData = CompressData(fileBytes);

                Console.WriteLine("Compressed size: " + compressedData.Length);

                // Send compressed size
                byte[] compressedSize =
                    BitConverter.GetBytes((long)compressedData.Length);

                stream.Write(compressedSize, 0, 8);

                // Send compressed file
                stream.Write(compressedData, 0, compressedData.Length);

                Console.WriteLine("Compressed file sent!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                client.Close();
            }
        }

        static byte[] CompressData(byte[] data)
        {
            using (MemoryStream output = new MemoryStream())
            {
                using (GZipStream gzip =
                    new GZipStream(output, CompressionMode.Compress))
                {
                    gzip.Write(data, 0, data.Length);
                }

                return output.ToArray();
            }
        }
    }
}
