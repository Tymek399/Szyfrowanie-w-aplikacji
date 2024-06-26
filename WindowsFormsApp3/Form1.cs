using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        private List<string> selectedFiles = new List<string>();
        private string encryptionKey = "twoj-klucz-szyfrowania";

        public Form1()
        {
            InitializeComponent();
        }

        private void btnSelectFiles_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Multiselect = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedFiles.AddRange(openFileDialog.FileNames);
                    UpdateFileList();
                }
            }
        }

        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            Parallel.ForEach(selectedFiles, file => EncryptFile(file, encryptionKey));
            MessageBox.Show("Szyfrowanie zakończone!");
        }

        private void btnDecrypt_Click(object sender, EventArgs e)
        {
            Parallel.ForEach(selectedFiles, file => DecryptFile(file, encryptionKey));
            MessageBox.Show("Deszyfrowanie zakończone!");
        }

        private void btnSendToServer_Click(object sender, EventArgs e)
        {
            Parallel.ForEach(selectedFiles, async file => await SendFileToServer(file));
            MessageBox.Show("Wysyłanie zakończone!");
        }

        private void UpdateFileList()
        {
            lstFiles.Items.Clear();
            foreach (var file in selectedFiles)
                lstFiles.Items.Add(file);
        }

        private void EncryptFile(string filePath, string key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = new byte[16];
                using (FileStream fsInput = new FileStream(filePath, FileMode.Open))
                using (FileStream fsEncrypted = new FileStream(filePath + ".enc", FileMode.Create))
                using (CryptoStream cs = new CryptoStream(fsEncrypted, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    fsInput.CopyTo(cs);
            }
        }

        private void DecryptFile(string filePath, string key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = new byte[16];
                using (FileStream fsEncrypted = new FileStream(filePath, FileMode.Open))
                using (FileStream fsDecrypted = new FileStream(filePath.Replace(".enc", ".dec"), FileMode.Create))
                using (CryptoStream cs = new CryptoStream(fsEncrypted, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    cs.CopyTo(fsDecrypted);
            }
        }

        private async Task SendFileToServer(string filePath)
        {
            using (HttpClient client = new HttpClient())
            {
                var content = new MultipartFormDataContent();
                content.Add(new StreamContent(File.OpenRead(filePath)), "file", Path.GetFileName(filePath));
                await client.PostAsync("http://twój-serwer.com/upload", content);
            }
        }
    }
}
