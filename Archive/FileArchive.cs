using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace Archive
{
    public class FileArchive
    {
        string _folder, _zipFile, _password, _imageFile;

        public FileArchive(string folder, string zipFile, string password, string imageFile)
        {
            _folder = folder;
            _zipFile = zipFile;
            _password = password;
            _imageFile = imageFile;
        }

        public bool Archive()
        {
            Output.WriteLine($"Archive");
            if (File.Exists(_zipFile))
            {
                File.Delete(_zipFile);
                Output.WriteLine($"The zip file {_zipFile} is deleted.");
            }

            var tempZipFile = _zipFile + ".zip";
            if (File.Exists(tempZipFile))
            {
                File.Delete(tempZipFile);
                Output.WriteLine($"The zip file {tempZipFile} is deleted.");
            }

            Output.Write($"Compress the folder {_folder}...");

            ZipFile.CreateFromDirectory(_folder, tempZipFile);
            Output.WriteLine($"\nThe zip file {tempZipFile} is created.");

            var imageFileBytes = File.ReadAllBytes(_imageFile);

            var fileEncryptor = new EncryptFile(imageFileBytes);

            Output.Write($"Encrypt...");
            fileEncryptor.Encrypt(tempZipFile, _zipFile, _password);

            File.Delete(tempZipFile);
            Output.WriteLine($"\nThe zip file {tempZipFile} is deleted.");

            Output.WriteLine($"The file {_zipFile} is created and secured.");
            return true;
        }


        public bool Restore()
        {
            Output.WriteLine($"Restore");
            if (!File.Exists(_zipFile))
            {
                Output.WriteLine($"Couldn't find encrypted zip file {_zipFile}.");
                return false;
            }
            var imageFileBytes = File.ReadAllBytes(_imageFile);

            var fileEncryptor = new EncryptFile(imageFileBytes);

            var tempZipFile = _zipFile + ".zip";
            if (File.Exists(tempZipFile))
            {
                File.Delete(tempZipFile);
                Output.WriteLine($"The zip file {tempZipFile} is deleted.");
            }

            Output.Write($"Decrypt...");
            fileEncryptor.Decrypt(_zipFile, tempZipFile, _password);
            Output.WriteLine($"\nThe file {_zipFile} is created.");

            Output.Write($"Extract...");
            ZipFile.ExtractToDirectory(tempZipFile, _folder);
            File.Delete(tempZipFile);
            Output.WriteLine($"\nThe file {tempZipFile} is deleted.");

            Output.WriteLine($"The file {_zipFile} is restored.");


            return true;
        }

        public bool EncryptFile()
        {
            if (File.Exists(_zipFile))
            {
                File.Delete(_zipFile);
            }

            var imageFileBytes = File.ReadAllBytes(_imageFile);

            var fileEncryptor = new EncryptFile(imageFileBytes);

            Output.Write($"Encrypt...");
            fileEncryptor.Encrypt(_folder, _zipFile, _password);

            Output.WriteLine($"\nThe file {_folder} is secured.");
            return true;
        }

        public bool DecryptFile()
        {
            if (!File.Exists(_folder))
            {
                Output.WriteLine($"Couldn't find encrypted zip file {_folder}.");
                return false;
            }
            var imageFileBytes = File.ReadAllBytes(_imageFile);

            var fileEncryptor = new EncryptFile(imageFileBytes);

            Output.WriteLine($"Decrypt...");
            fileEncryptor.Decrypt(_folder, _zipFile, _password);

            Output.WriteLine($"\nThe file {_zipFile} is restored.");

            return true;
        }
    }
}
