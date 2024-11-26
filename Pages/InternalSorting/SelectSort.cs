using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SortingAlgorithms.Pages
{
    public partial class InternalSorting : Page
    {
        public async Task SelectSort()
        {
            for (int i = 0; i < rectangles.Count - 1; i++)
            {
                //поиск минимального элемента
                int indOfMin = i;
                await UpdateComments($"Ищем минимальный неотсортированный элемент");
                for (int j = i + 1; j < rectangles.Count; j++)
                {
                    _cancellationTokenSource!.Token.ThrowIfCancellationRequested();

                    rectangles[indOfMin].Fill = Brushes.DeepPink;
                    rectangles[j].Fill = Brushes.DeepPink;
                    await Task.Delay((int)DelaySlider.Value);
                    rectangles[indOfMin].Fill = active;
                    rectangles[j].Fill = active;

                    if (rectangles[j].Height < rectangles[indOfMin].Height)
                    {
                        indOfMin = j;
                    }
                }

                if (commentaries.Count > 0)
                {
                    commentaries[commentaries.Count - 1] += $" => №{indOfMin + 1}";
                }
                //обмен элементов
                await Swap(indOfMin, i);
                rectangles[i].Fill = final;
            }

            rectangles[rectangles.Count - 1].Fill = final;
        }
    }
}
