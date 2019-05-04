#region Usings

using System;
using System.Windows.Media;

#endregion

namespace AxialisIconGeneratorHelper.Utils
{
    public static class ColorParser
    {
        #region Public Methods

        public static Color ParseHexColor(string trimmedColor)
        {
            var alpha = (int) byte.MaxValue;
            int red;
            int green;
            int blue;
            if (trimmedColor.Length > 7)
            {
                alpha = ParseHexChar(trimmedColor[1]) * 16 + ParseHexChar(trimmedColor[2]);
                red = ParseHexChar(trimmedColor[3]) * 16 + ParseHexChar(trimmedColor[4]);
                green = ParseHexChar(trimmedColor[5]) * 16 + ParseHexChar(trimmedColor[6]);
                blue = ParseHexChar(trimmedColor[7]) * 16 + ParseHexChar(trimmedColor[8]);
            }
            else if (trimmedColor.Length > 5)
            {
                red = ParseHexChar(trimmedColor[1]) * 16 + ParseHexChar(trimmedColor[2]);
                green = ParseHexChar(trimmedColor[3]) * 16 + ParseHexChar(trimmedColor[4]);
                blue = ParseHexChar(trimmedColor[5]) * 16 + ParseHexChar(trimmedColor[6]);
            }
            else if (trimmedColor.Length > 4)
            {
                var hexChar1 = ParseHexChar(trimmedColor[1]);
                alpha = hexChar1 + hexChar1 * 16;
                var hexChar2 = ParseHexChar(trimmedColor[2]);
                red = hexChar2 + hexChar2 * 16;
                var hexChar3 = ParseHexChar(trimmedColor[3]);
                green = hexChar3 + hexChar3 * 16;
                var hexChar4 = ParseHexChar(trimmedColor[4]);
                blue = hexChar4 + hexChar4 * 16;
            }
            else
            {
                var hexChar1 = ParseHexChar(trimmedColor[1]);
                red = hexChar1 + hexChar1 * 16;
                var hexChar2 = ParseHexChar(trimmedColor[2]);
                green = hexChar2 + hexChar2 * 16;
                var hexChar3 = ParseHexChar(trimmedColor[3]);
                blue = hexChar3 + hexChar3 * 16;
            }

            return Color.FromArgb((byte) alpha, (byte) red, (byte) green, (byte) blue);
        }

        #endregion

        #region Private Methods

        private static int ParseHexChar(char c)
        {
            var num = (int) c;
            if (num >= 48 && num <= 57)
                return num - 48;
            if (num >= 97 && num <= 102)
                return num - 97 + 10;
            if (num >= 65 && num <= 70)
                return num - 65 + 10;
            throw new FormatException("Parsers_IllegalToken");
        }

        #endregion
    }
}