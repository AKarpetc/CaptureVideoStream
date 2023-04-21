using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AForge.Net.Streams
{
    internal static class ByteArrayUtilsNew
    {
        //
        // Сводка:
        //     Check if the array contains needle at specified position.
        //
        // Параметры:
        //   array:
        //     Source array to check for needle.
        //
        //   needle:
        //     Needle we are searching for.
        //
        //   startIndex:
        //     Start index in source array.
        //
        // Возврат:
        //     Returns true if the source array contains the needle at the specified index.
        //     Otherwise it returns false.
        public static bool Compare(byte[] array, byte[] needle, int startIndex)
        {
            int num = needle.Length;
            int num2 = 0;
            int num3 = startIndex;
            while (num2 < num)
            {
                if (array[num3] != needle[num2])
                {
                    return false;
                }

                num2++;
                num3++;
            }

            return true;
        }

        //
        // Сводка:
        //     Find subarray in the source array.
        //
        // Параметры:
        //   array:
        //     Source array to search for needle.
        //
        //   needle:
        //     Needle we are searching for.
        //
        //   startIndex:
        //     Start index in source array.
        //
        //   sourceLength:
        //     Number of bytes in source array, where the needle is searched for.
        //
        // Возврат:
        //     Returns starting position of the needle if it was found or -1 otherwise.
        public static int Find(byte[] array, byte[] needle, int startIndex, int sourceLength)
        {
            int num = needle.Length;
            while (sourceLength >= num)
            {
                int num2 = Array.IndexOf(array, needle[0], startIndex, sourceLength - num + 1);
                if (num2 == -1)
                {
                    return -1;
                }

                int num3 = 0;
                int num4 = num2;
                while (num3 < num && array[num4] == needle[num3])
                {
                    num3++;
                    num4++;
                }

                if (num3 == num)
                {
                    return num2;
                }

                sourceLength -= num2 - startIndex + 1;
                startIndex = num2 + 1;
            }

            return -1;
        }
    }
}
