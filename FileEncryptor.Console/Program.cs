using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace FileEncryptor.Console
{
    internal class Program
    {
        private const string EncryptedExtension  = ".aes";

        private const int BufferSize = 1024 * 1024;

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                System.Console.WriteLine("No file specified for processing");
                return;
            }

            foreach (FileInfo file in args.Select(fileName => new FileInfo(fileName))
                .Where(f => f.Exists))
            {
                string ext = file.Extension;

                if (string.Equals(ext, EncryptedExtension, StringComparison.OrdinalIgnoreCase))
                {
                    Decrypt(file);
                }
                else
                {
                    Encrypt(file);
                }
            }
        }

        private static void Encrypt(FileInfo file)
        {
            System.Console.WriteLine("\nThe file is being encrypted...");
            System.Console.WriteLine($"\nSource file: {file.FullName}");

            var encodedFile = new FileInfo($"{file.FullName}{EncryptedExtension}");
            System.Console.WriteLine($"\nDestination file: {encodedFile.FullName}\n");

            string fileName = Path.GetFileNameWithoutExtension(file.Name);

            using Aes aes = CreateAes(fileName);

            ICryptoTransform encryptor = aes.CreateEncryptor();

            using FileStream sourceFile = file.OpenRead();
            using FileStream destinationFile = encodedFile.Create();
            using var cryptoStream = new CryptoStream(destinationFile, encryptor, CryptoStreamMode
                .Write);

            CopyToCryptoStream(sourceFile, cryptoStream);
        }

        private static void Decrypt(FileInfo file)
        {
            System.Console.WriteLine("\nThe file is being decrypted...");
            System.Console.WriteLine($"\nSource file: {file.FullName}");

            string sourceFileNameWithoutEncryptedExtension = Path.GetFileNameWithoutExtension(file
                .FullName);
            var source = new FileInfo($"{file.DirectoryName}\\" +
                                      $"{sourceFileNameWithoutEncryptedExtension}");
            System.Console.WriteLine($"\nDestination file: {source.FullName}\n");

            string password = Path.GetFileNameWithoutExtension(
                sourceFileNameWithoutEncryptedExtension);
            using Aes aes = CreateAes(password);
            ICryptoTransform decryptor = aes.CreateDecryptor();

            try
            {
                using FileStream sourceFile = file.OpenRead();
                using FileStream destinationFile = source.Create();
                using var cryptoStream = new CryptoStream(destinationFile, decryptor, 
                    CryptoStreamMode.Write);

                CopyToCryptoStream(sourceFile, cryptoStream);
            }
            catch (CryptographicException)
            {
                source.Delete();
                System.Console.CursorLeft = 0;
                System.Console.WriteLine("\nFile name error\n");
                System.Console.WriteLine();
            }
        }

        private static void CopyToCryptoStream(Stream source, CryptoStream destination)
        {
            var buffer = new byte[BufferSize];
            var readInfoTotal = 0L;
            double totalLength = source.Length;
            int readInfo;

            do
            {
                readInfo = source.Read(buffer);
                destination.Write(buffer, 0, readInfo);

                readInfoTotal += readInfo;
                System.Console.CursorLeft = 0;
                System.Console.Write("Completed {0:p2}", readInfoTotal / totalLength);
            }
            while (readInfo == BufferSize);

            destination.FlushFinalBlock();

            System.Console.CursorLeft = 0;
            System.Console.Write("Operation completed successfully\n");
            System.Console.WriteLine();
        }

        private static Aes CreateAes(string password)
        {
            var pdb = new Rfc2898DeriveBytes(password, Constants.Salt);
            var aes = Aes.Create();
            aes.Key = pdb.GetBytes(32);
            aes.IV = pdb.GetBytes(16);
            return aes;
        }
    }
}
