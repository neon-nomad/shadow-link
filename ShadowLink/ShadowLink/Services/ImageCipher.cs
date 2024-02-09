using System.Drawing;
using System.Text;

namespace ShadowLink.Models;

public class ImageCipher
{
      public static Bitmap? EmbedText(string text, Bitmap? bmp)
    {
        int pixelIndex = 0;

        // for each character in the text
        for (int i = 0; i < text.Length; i++)
        {
            // for each bit in the character
            for (int j = 0; j < 8; j++)
            {
                // get the pixel at the current index
                Color pixel = bmp.GetPixel(pixelIndex % bmp.Width, pixelIndex / bmp.Width);

                // get the next bit in the text we want to hide
                int bit = text[i] >> j & 1;

                // change the least significant bit of each color channel
                int r = pixel.R - pixel.R % 2 + bit;
                int g = pixel.G - pixel.G % 2 + bit;
                int b = pixel.B - pixel.B % 2 + bit;

                // set the new pixel
                bmp.SetPixel(pixelIndex % bmp.Width, pixelIndex / bmp.Width, Color.FromArgb(r, g, b));

                pixelIndex++;
            }
        }

        return bmp;
    }

    public static string ExtractText(Bitmap bmp)
    {
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
                        0 => charValue * 2 + pixel.R % 2,
                        1 => charValue * 2 + pixel.G % 2,
                        2 => charValue * 2 + pixel.B % 2,
                        _ => charValue
                    };

                    colorUnitIndex++;

                    // if 8 bits have been processed, add the current character to the result text
                    if (colorUnitIndex % 8 == 0)
                    {
                        // reverse, since each time the process happens on the right 
                        charValue = ReverseBits(charValue);

                        // can only be 0 if it is the stop character (the 8 zeros)
                        if (charValue == 0)
                        {
                            return extractedText.ToString();
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
}