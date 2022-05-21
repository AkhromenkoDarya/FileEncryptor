using FileEncryptor.WPF.Services.Interfaces;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

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

        public void Encrypt(
            string sourcePath, 
            string destinationPath, 
            string password, 
            int bufferLength = 102400)
        {
            ICryptoTransform encryptor = GetEncryptor(password /*, Encoding.UTF8.GetBytes(SourcePath)*/);

            using FileStream destinationEncrypted = File.Create(destinationPath, bufferLength);
            using var destination = new CryptoStream(destinationEncrypted, encryptor, 
                CryptoStreamMode.Write);
            using FileStream source = File.OpenRead(sourcePath);

            var buffer = new byte[bufferLength];
            int readCount;

            do
            {
                Thread.Sleep(1);
                readCount = source.Read(buffer, 0, bufferLength);
                destination.Write(buffer, 0, readCount);
            } 
            while (readCount > 0);

            destination.FlushFinalBlock();
        }

        public bool Decrypt(
            string sourcePath, 
            string destinationPath, 
            string password, 
            int bufferLength = 102400)
        {
            ICryptoTransform decryptor = GetDecryptor(password);

            using FileStream destinationDecrypted = File.Create(destinationPath, bufferLength);
            using var destination = new CryptoStream(destinationDecrypted, decryptor, 
                CryptoStreamMode.Write);
            using FileStream encryptedSource = File.OpenRead(sourcePath);

            var buffer = new byte[bufferLength];
            int readCount;

            do
            {
                readCount = encryptedSource.Read(buffer, 0, bufferLength);
                destination.Write(buffer, 0, readCount);
            } 
            while (readCount > 0);

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

        public async Task EncryptAsync(
            string sourcePath, 
            string destinationPath, 
            string password, 
            int bufferLength = 102400, 
            IProgress<double> progress = null, 
            CancellationToken cancellationToken = default)
        {
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException("Source file for encryption process not found", 
                    sourcePath);
            }

            if (bufferLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferLength), bufferLength, 
                    "Read buffer size must be greater than 0");
            }

            cancellationToken.ThrowIfCancellationRequested();

            ICryptoTransform encryptor = GetEncryptor(password/*, Encoding.UTF8.GetBytes(SourcePath)*/);

            try
            {
                await using FileStream destinationEncrypted = File.Create(destinationPath, 
                    bufferLength);
                await using var destination = new CryptoStream(destinationEncrypted, encryptor, 
                    CryptoStreamMode.Write);
                await using FileStream source = File.OpenRead(sourcePath);

                long fileLength = source.Length;
                var buffer = new byte[bufferLength];
                int readCount;
                var lastPercent = 0.0;

                do
                {
                    readCount = await source.ReadAsync(buffer, 0, bufferLength, 
                        cancellationToken).ConfigureAwait(false);
                    // Дополнительные действия по завершению асинхронной операции.
                    await destination.WriteAsync(buffer, 0, readCount, cancellationToken)
                        .ConfigureAwait(false);

                    long position = source.Position;
                    double percent = (double)position / fileLength;

                    if (percent - lastPercent >= 0.001)
                    {
                        progress?.Report(percent);
                        lastPercent = percent;
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        // Очистка состояния операции.
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
                while (readCount > 0);

                //destination.FlushFinalBlock();
                progress?.Report(1);
            }
            catch (OperationCanceledException)
            {
                progress?.Report(0);
                throw;
            }
        }

        public async Task<bool> DecryptAsync(
            string sourcePath, 
            string destinationPath, 
            string password, 
            int bufferLength = 102400, 
            IProgress<double> progress = null, 
            CancellationToken cancellationToken = default)
        {
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException("Source file for encryption process not found",
                    sourcePath);
            }

            if (bufferLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferLength), bufferLength,
                    "Read buffer size must be greater than 0");
            }

            cancellationToken.ThrowIfCancellationRequested();

            ICryptoTransform decryptor = GetDecryptor(password);

            await using FileStream destinationDecrypted = File.Create(destinationPath,
                bufferLength);
            await using var destination = new CryptoStream(destinationDecrypted, decryptor,
                CryptoStreamMode.Write);
            await using FileStream encryptedSource = File.OpenRead(sourcePath);

            try
            {
                long fileLength = encryptedSource.Length;
                var buffer = new byte[bufferLength];
                int readCount;
                var lastPercent = 0.0;

                do
                {
                    readCount = await encryptedSource.ReadAsync(buffer, 0, bufferLength,
                        cancellationToken).ConfigureAwait(false);
                    await destination.WriteAsync(buffer, 0, readCount, cancellationToken)
                        .ConfigureAwait(false);

                    long position = encryptedSource.Position;
                    double percent = (double)position / fileLength;

                    if (percent - lastPercent >= 0.001)
                    {
                        progress?.Report(percent);
                        lastPercent = percent;
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                } 
                while (readCount > 0);

                try
                {
                    destination.FlushFinalBlock();
                }
                catch (CryptographicException)
                {
                    //return Task.FromResult(false);
                    return false;
                }

                progress?.Report(1);
            }
            catch (OperationCanceledException)
            {
                try
                {
                    destination.FlushFinalBlock();
                }
                catch (CryptographicException)
                {
                    //Действия по обработке исключения CryptographicException.
                }

                progress?.Report(0);
                throw;
            }

            //return Task.FromResult(true);
            return true;
        }
    }
}
