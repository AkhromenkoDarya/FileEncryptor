using FileEncryptor.WPF.Services.Interfaces;
using System.IO;
using System.Security.Cryptography;

namespace FileEncryptor.WPF.Services
{
    internal class Encryptor : IEncryptor
    {
        private static readonly byte[] Salt =
        {
            0x26, 0xdc, 0xff, 0x00,
            0xad, 0xed, 0x7a, 0xee,
            0xc5, 0xfe, 0x07, 0xaf,
            0x4d, 0x08, 0x22, 0x3c
        };

        private static ICryptoTransform GetEncryptor(string password, byte[] salt = null)
        {
            var pdb = new Rfc2898DeriveBytes(password, salt ?? Salt);
            var algorithm = Rijndael.Create();
            algorithm.Key = pdb.GetBytes(32);
            algorithm.IV = pdb.GetBytes(16);
            return algorithm.CreateEncryptor();
        }

        private static ICryptoTransform GetDecryptor(string password, byte[] salt = null)
        {
            var pdb = new Rfc2898DeriveBytes(password, salt ?? Salt);
            var algorithm = Rijndael.Create();
            algorithm.Key = pdb.GetBytes(32);
            algorithm.IV = pdb.GetBytes(16);
            return algorithm.CreateDecryptor();
        }

        public void Encrypt(string sourcePath, string destinationPath, string password, 
            int bufferLength = 104200)
        {
            ICryptoTransform encryptor = GetEncryptor(password /*, Encoding.UTF8.GetBytes(SourcePath)*/);

            using FileStream destinationEncrypted = File.Create(destinationPath, bufferLength);
            using var destination = new CryptoStream(destinationEncrypted, encryptor, 
                CryptoStreamMode.Write);
            using FileStream source = File.OpenRead(sourcePath);

            var buffer = new byte[bufferLength];
            int reader;
            do
            {
                reader = source.Read(buffer, 0, bufferLength);
                destination.Write(buffer, 0, reader);
            } 
            while (reader > 0);

            destination.FlushFinalBlock();
        }

        public bool Decrypt(string sourcePath, string destinationPath, string password, 
            int bufferLength = 104200)
        {
            ICryptoTransform decryptor = GetDecryptor(password);

            using FileStream destinationDecrypted = File.Create(destinationPath, bufferLength);
            using var destination = new CryptoStream(destinationDecrypted, decryptor, 
                CryptoStreamMode.Write);
            using FileStream encryptedSource = File.OpenRead(sourcePath);

            var buffer = new byte[bufferLength];
            int reader;
            do
            {
                reader = encryptedSource.Read(buffer, 0, bufferLength);
                destination.Write(buffer, 0, reader);
            } 
            while (reader > 0);

            try
            {
                destination.FlushFinalBlock();
            }
            catch (CryptographicException)
            {
                return false;
            }

            return true;
        }
    }
}
