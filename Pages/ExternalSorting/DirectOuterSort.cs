using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using System.Xml.Linq;

namespace SortingAlgorithms.Pages
{
    public partial class ExternalSorting : Page
    {
        private async Task DirectOuterSort(string attribute)
        {
            for (int i = 1; i < table.Count; i++)
            {
                var current = table[i];
                int j = i - 1;

                while (j >= 0)
                {
                    _cancellationTokenSource!.Token.ThrowIfCancellationRequested();


                    bool isSwapNeeded;
                    if (long.TryParse(this.table.First()[attribute], out _))
                    {
                        isSwapNeeded = Convert.ToInt64(table[j][attribute]) > Convert.ToInt64(current[attribute]);
                    }
                    else if (double.TryParse(this.table.First()[attribute].Replace('.', ','), out _))
                    {
                        isSwapNeeded = Convert.ToDouble(table[j][attribute].Replace('.', ',')) > Convert.ToDouble(current[attribute].Replace('.', ','));
                    }
                    else
                    {
                        isSwapNeeded = string.Compare(table[j][attribute], current[attribute]) > 0;
                    }

                    if (!isSwapNeeded) break;

                    table[j + 1] = table[j];
                    j--;
                }
                table[j + 1] = current;
            }
        }
    }
}