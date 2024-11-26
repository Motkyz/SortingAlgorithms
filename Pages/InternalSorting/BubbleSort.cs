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
        public async Task BubbleSort()
        {
            for (int i = 0; i < rectangles.Count; i++)
            {
                for (int j = 0; j < rectangles.Count - 1 - i; j++)
                {
                    await UpdateComments($"Сравниваем элементы №{j + 1} и №{j + 2}, если №{j + 1} больше, чем №{j + 2}, то меняем местами");
                    rectangles[j].Fill = Brushes.DeepPink;
                    rectangles[j + 1].Fill = Brushes.DeepPink;
                    await Task.Delay((int)DelaySlider.Value);

                    if (rectangles[j].Height > rectangles[j + 1].Height)
                    {
                        await Swap(j, j + 1);
                    }


                    rectangles[j].Fill = active;
                    rectangles[j + 1].Fill = active;
                }

                rectangles[rectangles.Count - 1 - i].Fill = final;
            }
        }
    }
}
