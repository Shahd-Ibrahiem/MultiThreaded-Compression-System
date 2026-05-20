using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System;
using System.IO;
using System.Net.Sockets;
using System.Windows.Forms;


namespace CompressionClient
{
    public partial class Form1 : Form
    {
        string selectedFile = "";
        public Form1()
        {
            InitializeComponent();
        }

        private void btnChoose_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();

            if (open.ShowDialog() == DialogResult.OK)
            {
                selectedFile = open.FileName;
                lblPath.Text = selectedFile;
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (selectedFile == "")
            {
                MessageBox.Show("Choose a file first!");
                return;
            }

            try
            {
                TcpClient client = new TcpClient();

                client.Connect(txtServerIP.Text, 5000);

                NetworkStream stream = client.GetStream();

                // Read file
                byte[] fileBytes = File.ReadAllBytes(selectedFile);

                // Send file size
                byte[] sizeBytes =
                    BitConverter.GetBytes((long)fileBytes.Length);

                stream.Write(sizeBytes, 0, 8);

                // Send file
                stream.Write(fileBytes, 0, fileBytes.Length);

                // Receive compressed size
                byte[] compressedSizeBytes = new byte[8];

                stream.Read(compressedSizeBytes, 0, 8);

                long compressedSize =
                    BitConverter.ToInt64(compressedSizeBytes, 0);

                // Receive compressed file
                byte[] compressedData = new byte[compressedSize];

                int totalRead = 0;

                while (totalRead < compressedSize)
                {
                    int read = stream.Read(
                        compressedData,
                        totalRead,
                        (int)(compressedSize - totalRead));

                    if (read == 0)
                        break;

                    totalRead += read;
                }

                // Save compressed file
                string savePath =
                    selectedFile + ".gz";

                File.WriteAllBytes(savePath, compressedData);

                MessageBox.Show(
                    "Compressed file saved:\n" + savePath);

                client.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}