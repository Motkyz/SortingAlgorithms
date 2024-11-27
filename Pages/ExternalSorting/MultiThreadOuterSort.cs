using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SortingAlgorithms.Pages
{
    public partial class ExternalSorting : Page
    {
        private async Task MultiThreadOuterSort(string attribute)
        {
            await NaturalOuterSort(attribute);
        }

        private async Task<List<List<Dictionary<string, string>>>> MultiThreadMerge(List<List<Dictionary<string, string>>> chunk, string attribute)
        {
            var priorityQueue = new SortedDictionary<(string, int), Dictionary<string, string>>(new ComparerForMerge());

            var indixes = new int[chunk.Count];

            for (int i = 0; i < chunk.Count; i++)
            {
                _cancellationTokenSource!.Token.ThrowIfCancellationRequested();


                if (chunk[i].Count > 0)
                {
                    var key = (chunk[i][0][attribute], i);
                    priorityQueue.Add(key, chunk[i][0]);
                }
            }

            var merged = new List<Dictionary<string, string>>();

            while (priorityQueue.Count > 0)
            {
                _cancellationTokenSource!.Token.ThrowIfCancellationRequested();


                var firstKey = priorityQueue.First().Key;
                var currentRecord = priorityQueue.First().Value;
                priorityQueue.Remove(firstKey);

                merged.Add(currentRecord);

                int chunkIndex = firstKey.Item2;
                indixes[chunkIndex]++;

                if (indixes[chunkIndex] < chunk[chunkIndex].Count)
                {
                    var nextKey = (chunk[chunkIndex][indixes[chunkIndex]][attribute], chunkIndex);
                    priorityQueue.Add(nextKey, chunk[chunkIndex][indixes[chunkIndex]]);
                }
            }

            return new List<List<Dictionary<string, string>>> { merged };
        }

        private class ComparerForMerge : IComparer<(string, int)>
        {
            public ComparerForMerge()
            { }

            public int Compare((string, int) x, (string, int) y)
            {
                int comparison;
                if (long.TryParse(x.Item1, out _))
                {
                    comparison = Convert.ToInt64(x.Item1).CompareTo(Convert.ToInt64(y.Item1));
                }
                else if (double.TryParse(x.Item1.Replace('.', ','), out _))
                {
                    comparison = Convert.ToDouble(x.Item1.Replace('.', ',')).CompareTo(Convert.ToDouble(y.Item1.Replace('.', ',')));
                }
                else
                {
                    comparison = string.Compare(x.Item1, y.Item1, StringComparison.Ordinal);
                }

                return comparison;
            }
        }
    }
}
