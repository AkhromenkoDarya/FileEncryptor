using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace FileEncryptor.Console
{
    internal class Program
    {
        private const string EncodedExtension  = ".aes";

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

                if (string.Equals(ext, EncodedExtension, StringComparison.OrdinalIgnoreCase))
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
            System.Console.WriteLine("The file is being encrypted...");
            System.Console.WriteLine(file.FullName);

            var encodedFile = new FileInfo($"{file.FullName}{EncodedExtension}");
            System.Console.WriteLine(encodedFile.FullName);

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
            System.Console.WriteLine("The file is being decrypted...");
            System.Console.WriteLine(file.FullName);

            string sourceFileFullName = Path.GetFileNameWithoutExtension(file.FullName);
            var source = new FileInfo(sourceFileFullName);
            System.Console.WriteLine(source.FullName);

            string password = Path.GetFileNameWithoutExtension(sourceFileFullName);
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
                System.Console.WriteLine("File name error");
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
                System.Console.Write("Completed in {0:p2}", readInfoTotal / totalLength);
            }
            while (readInfo == BufferSize);

            destination.FlushFinalBlock();

            System.Console.CursorLeft = 0;
            System.Console.WriteLine("Completed");
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
