using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingAlgorithms.Pages
{
    public class QuickSortStrings
    {
        public string[] SortedWords;

        public QuickSortStrings(string text)
        {
            SortedWords = text.ToLower().Split(" ", StringSplitOptions.RemoveEmptyEntries);
            Sort(SortedWords, 0, SortedWords.Length - 1);
        }

        public void Sort(string[] arr, int left, int right)
        {
            if (left >= right)
                return; 

            int swapInd = Partition(arr, left, right);

            Sort(arr, left, swapInd - 1);
            Sort(arr, swapInd + 1, right);
        }

        private static int Partition(string[] arr, int left, int right)
        {
            string partition = arr[right];
            int swapInd = (left - 1);

            for (int j = left; j < right; j++)
            {
                if (String.Compare(arr[j], partition) <= 0)
                {
                    swapInd++;

                    (arr[swapInd], arr[j]) = (arr[j], arr[swapInd]);
                }
            }

            (arr[swapInd + 1], arr[right]) = (arr[right], arr[swapInd + 1]);

            return swapInd + 1;
        }
    }
}
