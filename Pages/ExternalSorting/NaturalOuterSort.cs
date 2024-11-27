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
        private async Task NaturalOuterSort(string attribute)
        {
            List<List<Dictionary<string, string>>> chunks = new List<List<Dictionary<string, string>>>();
            List<Dictionary<string, string>> currentChunk = new List<Dictionary<string, string>>();

            for (int i = 0; i < table.Count; i++)
            {
                _cancellationTokenSource!.Token.ThrowIfCancellationRequested();

                if (i > 0)
                {
                    bool isCurrentLess;
                    if (long.TryParse(table[i - 1][attribute], out _))
                    {
                        isCurrentLess = Convert.ToInt64(table[i - 1][attribute]) > Convert.ToInt64(table[i][attribute]);
                    }
                    else if (double.TryParse(table[i - 1][attribute].Replace('.', ','), out _))
                    {
                        isCurrentLess = Convert.ToDouble(table[i - 1][attribute].Replace('.', ',')) > Convert.ToDouble(table[i][attribute].Replace('.', ','));
                    }
                    else
                    {
                        isCurrentLess = string.Compare(table[i - 1][attribute], table[i][attribute]) > 0;
                    }

                    if (isCurrentLess)
                    {
                        chunks.Add(currentChunk);
                        currentChunk = new List<Dictionary<string, string>>();
                    }
                }
                currentChunk.Add(table[i]);
            }

            if (currentChunk.Count > 0)
                chunks.Add(currentChunk);

            while (chunks.Count > 1)
            {
                _cancellationTokenSource!.Token.ThrowIfCancellationRequested();

                chunks = await MultiThreadMerge(chunks, attribute);
            }

            table = chunks.First();
        }
    }
}
