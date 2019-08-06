using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Archive
{
    /// <summary>
    /// Encrypt text by password by the AsymmetricAlgorithm
    /// </summary>
    public static class Encrypt
    {
        // This constant is used to determine the keysize of the encryption algorithm
        private const int _keysize = 256;

        private const string _salt = "qshineSalt";

        /// <summary>
        /// Encrypt a text string
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="passwordText"></param>
        /// <returns></returns>
        public static string EncryptString(string plainText, string passwordText)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedArray;
            using (MemoryStream stream = new MemoryStream(byteArray))
            {

                using (MemoryStream resultStream = new MemoryStream())
                {

                    if (EncryptStream(stream, resultStream, Encoding.UTF8.GetBytes(passwordText), Encoding.UTF8.GetBytes(_salt)))
                    {
                        encryptedArray = resultStream.ToArray();
                        return Convert.ToBase64String(encryptedArray);
                    }
                }
            }
            return "";
        }

        private static int _bufferSize = 16384;
        public static bool EncryptStream(Stream inputStream, Stream outputStream, byte[] passwordBytes, byte[] saltBytes)
        {
            int ivLength;
            byte[] iv;
            using (Aes aes = new AesManaged())
            {
                aes.GenerateIV();
                ivLength = aes.IV.Length;
                iv = aes.IV;
            }

            byte[] buffer = new byte[_bufferSize];

            PasswordDeriveBytes password = new PasswordDeriveBytes(passwordBytes, saltBytes);
            byte[] keyBytes = password.GetBytes(_keysize / 8);

            using (RijndaelManaged symmetricKey = new RijndaelManaged())
            {
                symmetricKey.Mode = CipherMode.CBC;

                using (ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, iv))
                {
                    outputStream.Write(BitConverter.GetBytes(ivLength), 0, sizeof(int));
                    outputStream.Write(iv, 0, iv.Length);

                    using (CryptoStream cryptoStream = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write))
                    {
                        int charsRead = inputStream.Read(buffer, 0, _bufferSize);
                        if (charsRead == 0)
                        {
                            return false;
                        }

                        while (charsRead > 0)
                        {
                            cryptoStream.Write(buffer, 0, charsRead);
                            charsRead = inputStream.Read(buffer, 0, _bufferSize);
                        }

                        cryptoStream.FlushFinalBlock();
                        cryptoStream.Close();
                    }
                }
            }
            return true;
        }

        private static byte[] ReadIV(Stream s)
        {
            byte[] rawLength = new byte[sizeof(int)];
            if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
            {
                throw new FormatException("Invalid encrypted text.");
            }

            byte[] buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new FormatException("Invalid encrypted text.");
            }

            return buffer;
        }

        /// <summary>
        /// Decrypt a simple text string 
        /// </summary>
        /// <param name="cipherText">encrypted text</param>
        /// <param name="passwordText">password</param>
        /// <returns></returns>
        public static string DecryptString(string cipherText, string passwordText)
        {
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);

            byte[] plainTextBytes;
            using (MemoryStream stream = new MemoryStream(cipherTextBytes))
            {
                using (MemoryStream resultStream = new MemoryStream())
                {
                    var resultSize = DecryptStream(stream, resultStream, Encoding.UTF8.GetBytes(passwordText), Encoding.UTF8.GetBytes(_salt));

                    plainTextBytes = resultStream.ToArray();
                    return Encoding.UTF8.GetString(plainTextBytes, 0, (int)resultSize);
                }
            }
        }

        public static string DecryptString1(string cipherText, string passwordText)
        {
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
            byte[] rawLength = new byte[sizeof(int)];
            byte[] plainTextBytes;
            int decryptedByteCount;

            using (MemoryStream memoryStream = new MemoryStream(cipherTextBytes)){

                byte[] iv = ReadIV(memoryStream);
                int ivLength = iv.Length;
                int headerSize = sizeof(int) + ivLength;

                PasswordDeriveBytes password = new PasswordDeriveBytes(passwordText, Encoding.UTF8.GetBytes(_salt));
                byte[] keyBytes = password.GetBytes(_keysize / 8);

                RijndaelManaged symmetricKey = new RijndaelManaged();
                symmetricKey.Mode = CipherMode.CBC;
                ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, iv);
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    plainTextBytes = new byte[cipherTextBytes.Length- headerSize];
                    decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                    cryptoStream.Close();
                }
                memoryStream.Close();
            }
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
        }

        public static long DecryptStream(Stream cipherStream, Stream plainStream, byte[] passwordBytes, byte[] saltBytes)
        {
            //byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
            byte[] rawLength = new byte[sizeof(int)];
            int decryptedByteCount;
            byte[] buffer = new byte[_bufferSize];

            long size = cipherStream.Length;
            long resultSize = 0;

            byte[] iv = ReadIV(cipherStream);
            int ivLength = iv.Length;
            int headerSize = sizeof(int) + ivLength;

            PasswordDeriveBytes password = new PasswordDeriveBytes(passwordBytes, saltBytes);
            byte[] keyBytes = password.GetBytes(_keysize / 8);

            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, iv);

            size -= headerSize;
            using (CryptoStream cryptoStream = new CryptoStream(cipherStream, decryptor, CryptoStreamMode.Read))
            {
                long bufferSize = size>_bufferSize?_bufferSize: size;

                while (bufferSize > 0) {
                    decryptedByteCount = cryptoStream.Read(buffer, 0, (int)bufferSize);
                    plainStream.Write(buffer,0, (int)bufferSize);
                    resultSize += decryptedByteCount;

                    size -= bufferSize;
                    bufferSize = size > _bufferSize ? _bufferSize : size;
                }
            }
            return resultSize;
        }
    }
}
