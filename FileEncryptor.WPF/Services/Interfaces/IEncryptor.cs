using System;
using System.Threading;
using System.Threading.Tasks;

namespace FileEncryptor.WPF.Services.Interfaces
{
    internal interface IEncryptor
    {
        void Encrypt(string sourcePath, string destinationPath, string password, int bufferLength = 
            102400);

        bool Decrypt(string sourcePath, string destinationPath, string password, int bufferLength = 
            102400);

        Task EncryptAsync(string sourcePath, string destinationPath, string password, 
            int bufferLength = 102400, IProgress<double> progress = null, 
            CancellationToken cancellationToken = default);

        Task<bool> DecryptAsync(string sourcePath, string destinationPath, string password, 
            int bufferLength = 102400, IProgress<double> progress = null, 
            CancellationToken cancellationToken = default);
    }
}
