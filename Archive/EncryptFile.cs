using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace Archive
{

    public class EncryptFile
    {
        byte[] _imageBytes;
        byte[] _salt;
        int _imageSize;

        public EncryptFile(byte[] imageBytes)
        {
            _imageBytes = imageBytes;
            _imageSize = imageBytes.Length;
            _salt = GetSaltBytes();
        }

        /// <summary>
        /// Get salt bytes from image bytes.
        /// It get 256 bytes from image file
        /// </summary>
        /// <returns></returns>
        byte[] GetSaltBytes()
        {
            return GetMixBytes(256, 21, "");
        }

        byte[] GetSuperPassword(string password)
        {
            return GetMixBytes(128, 511, password);
        }

        byte[] GetMixBytes(int size, int position, string text)
        {
            string password = text + "qshineSalt";
            int index = position;
            int count = 0;
            int byteSize = size;
            byte prevByte = 0;
            byte v = 0;
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            int passwordSize = passwordBytes.Length;
            byte[] supperPasswordBytes = new byte[byteSize];

            while (index < _imageSize)
            {
                //skip 0 and previous value
                do
                {
                    v = (byte)(_imageBytes[index] ^ passwordBytes[index % passwordSize]);
                    index++;
                } while ((v == 0 || v == prevByte) && index < _imageSize);

                supperPasswordBytes[count++] = v;
                if (count == byteSize)
                {
                    return supperPasswordBytes;
                }
            }
            throw new FormatException("The image file is not qualified. Try to select a large and colorful image.");
        }
        /// <summary>
        /// encrypt file by password
        /// </summary>
        /// <param name="plainFile"></param>
        /// <param name="password"></param>
        public void Encrypt(string inFile, string outFile, string password)
        {
            var superPassword = GetSuperPassword(password);

            if (File.Exists(outFile))
            {
                File.Delete(outFile);
            }

            var tempFile = outFile + ".tmp";
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }


            using (FileStream outputFileStream = File.Create(tempFile))
            {
                using (FileStream inFileStream = File.OpenRead(inFile))
                {
                    Archive.Encrypt.EncryptStream(inFileStream, outputFileStream, superPassword, _salt);
                }
            }
            XorFile(tempFile, outFile);
            File.Delete(tempFile);
        }

        void XorFile(string tempFile, string outFile)
        {
            var buffer = GetDataBuffer();
            using (FileStream outputFileStream = File.Create(outFile))
            {
                using (FileStream inFileStream = File.OpenRead(tempFile))
                {
                    int charsRead = inFileStream.Read(buffer, 0, _bufferSize);
                    if (charsRead == 0)
                    {
                        return;
                    }
                    int size = 0;
                    while (charsRead > 0)
                    {
                        var xorBuffer = GetXorBuffer(size);
                        Xor(buffer, xorBuffer);

                        size += charsRead;

                        outputFileStream.Write(buffer, 0, charsRead);
                        charsRead = inFileStream.Read(buffer, 0, _bufferSize);
                    }
                }
            }
        }

        const int _bufferSize = 16384;
        byte[] _buffer = new byte[_bufferSize];
        byte[] _xorBuffer = new byte[_bufferSize];

        byte[] GetDataBuffer()
        {
            return _buffer;
        }

        void Xor(byte[] buffer, byte[] xorBuffer)
        {
            for(int i=0;i< _bufferSize; i++)
            {
                buffer[i] ^= xorBuffer[i];
            }
        }

        byte[] GetXorBuffer(int startPosition)
        {
            int begin = startPosition % _imageSize;
            if (_imageSize > begin + _bufferSize)
            {
                Buffer.BlockCopy(_imageBytes, begin, _xorBuffer, 0, _bufferSize);
            }else if(_imageSize > _bufferSize)
            {
                int count = _imageSize - begin;
                Buffer.BlockCopy(_imageBytes, begin, _xorBuffer, 0, count);
                Buffer.BlockCopy(_imageBytes, 0, _xorBuffer, count, _bufferSize-count);
            }
            else
            {
                int block = _bufferSize/ _imageSize;
                int left = _bufferSize % _imageSize;
                for (int i = 0; i < block; i++)
                {
                    Buffer.BlockCopy(_imageBytes, 0, _xorBuffer, i * _imageSize, _imageSize);
                }
                if (left > 0)
                {
                    Buffer.BlockCopy(_imageBytes, 0, _xorBuffer, block * _imageSize, left);
                }
            }
            return _xorBuffer;
        }


        /// <summary>
        /// encrypt file by password
        /// </summary>
        /// <param name="plainFile"></param>
        /// <param name="password"></param>
        public void Decrypt(string inFile, string outFile, string password)
        {
            var superPassword = GetSuperPassword(password);

            if (File.Exists(outFile))
            {
                File.Delete(outFile);
            }
            var tempFile = outFile + ".tmp";
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }

            XorFile(inFile, tempFile);


            using (FileStream outputFileStream = File.Create(outFile))
            {
                using (FileStream inFileStream = File.OpenRead(tempFile))
                {
                    Archive.Encrypt.DecryptStream(inFileStream, outputFileStream, superPassword, _salt);
                }
            }

            File.Delete(tempFile);
        }
    }
}
