﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace ObfuscatorProject.Protections.StringProtect_2
{
    internal class EncryptionHelper
    {


        private const string PasswordHash = "da39a3ee5e6b4b0d";
        private const string SaltKey = "1632bfb5c6409f6a";
        private const string VIKey = "c3d20d615ec8cd62";
        private static List<string> _list = new List<string>();
        public static void Generate()
        {
            using (var manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("_._"))
            {
                using (var streamReader = new StreamReader(new MemoryStream(UnHush(Read(manifestResourceStream)))))
                {
                    _list = streamReader.ReadToEnd().Split(new[]
                    {
                        Environment.NewLine
                    }, StringSplitOptions.None).ToList();
                }
            }
        }

        public static string Search(int key)
        {
            return _list.ElementAt(key);
        }

        private static byte[] Read(Stream input)
        {
            using (var memoryStream = new MemoryStream())
            {
                input.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static byte[] UnHush(byte[] text)
        {
            var key = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
            var xor = new byte[text.Length];
            for (var i = 0; i < text.Length; i++)
            {
                xor[i] = (byte)(text[i] ^ key[i % key.Length]);
            }
            return xor;
        }

        public static string Decrypt(string encryptedText)
        {
            if (Assembly.GetExecutingAssembly() != Assembly.GetCallingAssembly()) return "error.png";
            var cipherTextBytes = Convert.FromBase64String(encryptedText);
            var keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7 };

            var decryptor = symmetricKey.CreateDecryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));
            var memoryStream = new MemoryStream(cipherTextBytes);
            var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            var plainTextBytes = new byte[cipherTextBytes.Length];

            var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
        }
    }
}