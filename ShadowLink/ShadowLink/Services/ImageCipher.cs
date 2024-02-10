using System;
using System.Drawing;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Linq;

namespace ShadowLink.Services
{
    public class ImageCipher
    {
        private static RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();

        public static Bitmap? EmbedText(string text, string seed, Bitmap? bmp)
        {
            // Convert the seed to an integer
            int seedInt = seed.GetHashCode();

            // Use the seed to initialize the random number generator
            Random rng = new Random(seedInt);
            
            // Encrypt the message before embedding
            text = Encrypt(text, seed);

            // Use a pseudo-random number generator to select pixels for embedding
            int[] pixelIndices = Enumerable.Range(0, bmp.Width * bmp.Height).ToArray();
            Shuffle(pixelIndices);

            int pixelIndex = 0;

            // for each character in the text
            for (int i = 0; i < text.Length; i++)
            {
                // for each bit in the character
                for (int j = 0; j < 8; j += 2) // Use two bits per channel
                {
                    // get the pixel at the current index
                    Color pixel = bmp.GetPixel(pixelIndices[pixelIndex] % bmp.Width, pixelIndices[pixelIndex] / bmp.Width);

                    // get the next two bits in the text we want to hide
                    int bits = (text[i] >> j) & 3;

                    // change the two least significant bits of each color channel
                    int r = pixel.R - pixel.R % 4 + bits;
                    int g = pixel.G - pixel.G % 4 + bits;
                    int b = pixel.B - pixel.B % 4 + bits;

                    // set the new pixel
                    bmp.SetPixel(pixelIndices[pixelIndex] % bmp.Width, pixelIndices[pixelIndex] / bmp.Width, Color.FromArgb(r, g, b));

                    pixelIndex++;
                }
            }

            return bmp;
        }

        public static string ExtractText(string seed, Bitmap bmp)
        {
            // Convert the seed to an integer
            int seedInt = seed.GetHashCode();

            // Use the seed to initialize the random number generator
            Random rng = new Random(seedInt);
            
            int colorUnitIndex = 0;
            int charValue = 0;

            // holds the text that will be extracted from the image
            StringBuilder extractedText = new StringBuilder();

            // for each pixel in the image
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);

                    // for each color component (R, G, B)
                    for (int n = 0; n < 3; n++)
                    {
                        charValue = (colorUnitIndex % 3) switch
                        {
                            0 => charValue * 4 + pixel.R % 4,
                            1 => charValue * 4 + pixel.G % 4,
                            2 => charValue * 4 + pixel.B % 4,
                            _ => charValue
                        };

                        colorUnitIndex++;

                        // if 8 bits have been processed, add the current character to the result text
                        if (colorUnitIndex % 4 == 0)
                        {
                            // reverse, since each time the process happens on the right 
                            charValue = ReverseBits(charValue);

                            // can only be 0 if it is the stop character (the 8 zeros)
                            if (charValue == 0)
                            {
                                // Decrypt the extracted text
                                return Decrypt(extractedText.ToString(), seed);
                            }

                            // convert the character value from int to char
                            char c = (char)charValue;

                            // add the current character to the result text
                            extractedText.Append(c.ToString());
                        }
                    }
                }
            }

            return extractedText.ToString();
        }

        public static int ReverseBits(int n)
        {
            int result = 0;

            for (int i = 0; i < 8; i++)
            {
                result = result * 2 + n % 2;

                n /= 2;
            }

            return result;
        }

        private static void Shuffle(int[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do rngCsp.GetBytes(box);
                while (!(box[0] < n * (byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                int value = array[k];
                array[k] = array[n];
                array[n] = value;
            }
        }

        private static string Encrypt(string clearText, string key)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);

            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(key, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        private static string Decrypt(string cipherText, string key)
        {
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(key, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
    }
}