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
            // Выход из рекурсии
            if (endIndex <= startIndex)
            {
                return;
            }

            // Массив для подсчета количества подкатегорий символов (256 символов ASCII + 1 для управления пустыми строками)
            int[] count = new int[257];

            // Временная переменная для хранения отсортированных строк
            Dictionary<int, string> temp = new Dictionary<int, string>();

            // Подсчёт частоты появления символов в текущем разряде (currentIndex)
            for (int i = startIndex; i <= endIndex; i++)
            {
                // Получаем символ в текущем разряде и увеличиваем его счетчик
                int c = CharAt(SortedWords[i], currentIndex);
                count[c + 2]++;
            }

            for (int r = 0; r < 256; r++)
            {
                count[r + 1] += count[r]; // Накопительное суммирование
            }

            // Заполнение временной структуры temp, в которой строки будут отсортированы по текущему индексу символов
            for (int i = startIndex; i <= endIndex; i++)
            {
                // Определяем символ в текущем разряде
                int c = CharAt(SortedWords[i], currentIndex);
                temp.Add(count[c + 1]++, SortedWords[i]);
            }

            // Переносим отсортированные строки из временной структуры обратно в оригинальный массив
            for (int i = startIndex; i <= endIndex; i++)
            {
                SortedWords[i] = temp[i - startIndex];
            }

            // Рекурсивно сортируем каждую категорию символов, чтобы отсортировать по следующему разряду
            for (int r = 0; r < 256; r++)
            {
                Sort(startIndex + count[r], startIndex + count[r + 1] - 1, currentIndex + 1);
            }
        }
    }
}
