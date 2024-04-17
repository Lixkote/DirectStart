using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B8TAM
{
    class FrequentAppsHelper
    {
        public static byte[] DecodeUserAssistValue(byte[] encodedData)
        {
            byte[] decodedData = new byte[encodedData.Length];
            byte key = 0xAB;

            for (int i = 0; i < encodedData.Length; i++)
            {
                decodedData[i] = (byte)(encodedData[i] ^ key);
            }

            return decodedData;
        }

        public static string ExtractPath(string decodedString)
        {
            // Assuming the decoded string contains the path to .lnk or .exe file
            // You may need to implement a more sophisticated parsing logic based on your specific data format
            return decodedString;
        }
    }
}
