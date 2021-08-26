using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace folderintegrity
{
    public static class EncH
    {
        private readonly static string vector = "8947az34awl34kjq";
        public static byte[] GenerateAESKey()
        {
            byte[] b;
            using (AesCryptoServiceProvider aesCryptoService = new AesCryptoServiceProvider())
            {
                do
                {
                    aesCryptoService.KeySize = 256;
                    aesCryptoService.GenerateKey();
                    string temp = Encoding.Default.GetString(aesCryptoService.Key);
                    b = Strtobytearray(temp);
                }
                while (b.Length != aesCryptoService.Key.Length);
            }
            return b;
        }
        public static string Bytearraytostring(byte[] b)
        {
            return Encoding.Default.GetString(b);
        }
        public static byte[] Strtobytearray(string str)
        {
            return Encoding.Default.GetBytes(str);
        }
        public static byte[] Encrypt(string plainText, byte[] Key)
        {
            byte[] vectorBytes = Encoding.Default.GetBytes(vector);
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            byte[] encrypted;
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = Key;
                aesAlg.IV = vectorBytes;
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return encrypted;
        }
        public static string Decrypt(byte[] cipherText, byte[] Key)
        {
            byte[] vectorBytes = Encoding.Default.GetBytes(vector);
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            string plaintext = null;
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = Key;
                aesAlg.IV = vectorBytes;
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }
        public static string Hash(string input)
        {
            byte[] hash;
            using (var sha256 = new SHA256CryptoServiceProvider())
                hash = sha256.ComputeHash(Encoding.Unicode.GetBytes(input));
            var sb = new StringBuilder();
            foreach (byte b in hash) sb.AppendFormat("{0:x2}", b);
            return sb.ToString();
        }
        public static string Hash(byte[] imput)
        {
            byte[] hash;
            using (var sha256 = new SHA256CryptoServiceProvider())
                hash = sha256.ComputeHash(imput);
            var sb = new StringBuilder();
            foreach (byte b in hash) sb.AppendFormat("{0:x2}", b);
            return sb.ToString();
        }
    }
}