using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SortingAlgorithms.Pages
{
    public partial class InternalSorting : Page
    {
        private async Task QuickSortStart()
        {
            await QuickSort(0, rectangles.Count - 1);
        }

        private async Task QuickSort(int left, int right)
        {
            if (left >= right)
            {
                if (right >= 0 && right < rectangles.Count)
                    rectangles[right].Fill = final;

                return;
            }

            int swapInd = await Partition(left, right);

            await QuickSort(left, swapInd - 1);
            await QuickSort(swapInd, right);
        }

        private async Task<int> Partition(int left, int right)
        {
            await UpdateComments($"Выбираем элемент №{right + 1} как опорный");
            Rectangle partition = rectangles[right];
            partition.Fill = Brushes.Red;

            await UpdateComments($"Сортировка происходит в области от элемента №{left + 1} до №{right + 1}");
            for (int i = left * 2; i <= right * 2; i++)
            {
                background[i].Fill = Brushes.LightGray;
            }
            await Task.Delay((int)DelaySlider.Value);

            int swapInd = left - 1;

            for (int j = left; j < right; j++)
            {
                if (rectangles[j].Height <= partition.Height)
                {
                    swapInd++;
                    if (swapInd != j)
                    {
                        await Swap(swapInd, j);
                    }
                }
            }

            if (swapInd + 1 != right)
            {
                await Swap(swapInd + 1, right);
            }

            for (int i = left * 2; i <= right * 2; i++)
            {
                background[i].Fill = Brushes.White;
            }

            partition.Fill = active;

            return swapInd + 1;
        }
    }
}
