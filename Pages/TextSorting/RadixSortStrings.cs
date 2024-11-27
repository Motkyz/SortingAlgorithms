using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingAlgorithms.Pages
{
    public class RadixSortStrings
    {
        public string[] SortedWords;

        public RadixSortStrings(string text)
        {
            SortedWords = text.ToLower().Split(" ", StringSplitOptions.RemoveEmptyEntries);
            Sort(0, SortedWords.Length - 1, 0);
        }

        private static int CharAt(string str, int index)
        {
            if (str.Length <= index)
            {
                return -1;
            }
            return str[index];
        }

        void Sort(int startIndex, int endIndex, int currentIndex)
        {
            if (endIndex <= startIndex)
            {
                return;
            }

            int[] count = new int[257];

            Dictionary<int, string> tempDict = new Dictionary<int, string>();

            for (int i = startIndex; i <= endIndex; i++)
            {
                int c = CharAt(SortedWords[i], currentIndex);
                count[c + 2]++;
            }

            for (int r = 0; r < 256; r++)
            {
                count[r + 1] += count[r];
            }

            for (int i = startIndex; i <= endIndex; i++)
            {
                int c = CharAt(SortedWords[i], currentIndex);
                tempDict.Add(count[c + 1]++, SortedWords[i]);
            }

            for (int i = startIndex; i <= endIndex; i++)
            {
                SortedWords[i] = tempDict[i - startIndex];
            }

            for (int r = 0; r < 256; r++)
            {
                Sort(startIndex + count[r], startIndex + count[r + 1] - 1, currentIndex + 1);
            }
        }
    }
}
