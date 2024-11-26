﻿using System;
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
        public async Task HeapSort()
        {
            int n = rectangles.Count;

            await UpdateComments("Строим кучу (перегруппируем массив)");
            for (int i = n / 2 - 1; i >= 0; i--)
                await heapify(n, i);
            await UpdateComments("Куча построена");
            await Task.Delay((int)DelaySlider.Value);

            // Один за другим извлекаем элементы из кучи
            for (int i = n - 1; i >= 0; i--)
            {
                // Перемещаем текущий корень в конец
                await UpdateComments("Перемещаем корень в конец неотсортированной части массива");
                await Swap(0, i);
                rectangles[i].Fill = final;


                await UpdateComments("Строим кучу на уменьшенном массиве");
                await heapify(i, 0);
                await UpdateComments("Куча построена");
                await Task.Delay((int)DelaySlider.Value);
            }

            rectangles[0].Fill = final;
        }

        // Процедура для преобразования в двоичную кучу поддерева с корневым узлом i
        async Task heapify(int n, int i)
        {
            await UpdateComments($"Выбираем корнем элемент №{i + 1}, его же считаем наибольшим элементом");
            int largest = i;
            rectangles[largest].Fill = Brushes.Purple;
            // Инициализируем наибольший элемент как корень

            int left = 2 * i + 1; // left = 2*i + 1
            int right = 2 * i + 2; // right = 2*i + 2
           
            await Task.Delay((int)DelaySlider.Value);

            await UpdateComments($"Далее сравниваем его с дочерними элементами, если таковые имеются и меняем местами, если дочерний элемент больше");

            if (left < n)
            {
                rectangles[left].Fill = Brushes.DeepPink;
            }

            if (right < n) 
            {
                rectangles[right].Fill = Brushes.DeepPink;
            }

            await Task.Delay((int)DelaySlider.Value);
            // Если левый дочерний элемент больше корня
            if (left < n && rectangles[left].Height > rectangles[largest].Height)
                largest = left;

            // Если правый дочерний элемент больше, чем самый большой элемент на данный момент
            if (right < n && rectangles[right].Height > rectangles[largest].Height)
                largest = right;

            // Если самый большой элемент не корень
            if (largest != i)
            {
                rectangles[i].Fill = active;
                if (left < n)
                {
                    rectangles[left].Fill = active;
                }

                if (right < n)
                {
                    rectangles[right].Fill = active;
                }

                await Swap(largest, i);

                // Рекурсивно преобразуем в двоичную кучу затронутое поддерево
                await heapify(n, largest);
            }

            rectangles[i].Fill = active;
            if (left < n)
            {
                rectangles[left].Fill = active;
            }

            if (right < n)
            {
                rectangles[right].Fill = active;
            }
        }
    }
}