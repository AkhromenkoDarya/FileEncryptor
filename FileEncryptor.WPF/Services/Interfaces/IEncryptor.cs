namespace FileEncryptor.WPF.Services.Interfaces
{
    internal interface IEncryptor
    {
        void Encrypt(string sourcePath, string destinationPath, string password, int bufferLength = 
            104200);

        bool Decrypt(string sourcePath, string destinationPath, string password, int bufferLength = 
            104200);
    }
}
