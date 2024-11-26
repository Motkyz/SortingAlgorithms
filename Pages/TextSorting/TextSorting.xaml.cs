using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SortingAlgorithms.Pages
{
    public partial class TextSorting : Page
    {
        public TextSorting()
        {
            InitializeComponent();
        }

        private void ToStart_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _ = NavigationService.Navigate(new Uri("/Pages/Start.xaml", UriKind.Relative));
        }

        private void SortBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                Dictionary<string, int> countOfWords = new Dictionary<string, int>();
                string selectedSort = ((ComboBoxItem)Sorts.SelectedItem).Content.ToString()!;
                switch (selectedSort)
                {
                    case "Quick Sort":
                        QuickSortStrings QSort = new QuickSortStrings(OrigTxt.Text);
                        SortedTxt.Text = string.Join(" ", QSort.SortedWords);
                        countOfWords = WordsCounter.CountWords(QSort.SortedWords);
                        break;
                    case "Radix Sort":
                        RadixSortStrings RSort = new RadixSortStrings(OrigTxt.Text);
                        SortedTxt.Text = string.Join(" ", RSort.SortedWords);
                        countOfWords = WordsCounter.CountWords(RSort.SortedWords);
                        break;
                    default:
                        MessageBox.Show("Сначала выберите алгоритм");
                        break;
                }

                StringBuilder sb = new StringBuilder();
                foreach (var pair in countOfWords)
                {
                    sb.Append(string.Format("{0} - {1}\n", pair.Key, pair.Value));
                }
                WordsCount.Text = sb.ToString();
            }

            catch (OperationCanceledException)
            {
            }

            catch (NullReferenceException)
            {
                MessageBox.Show("Сначала напишите или сгенерируйте текст");
            }
        }

        private void GenerateText_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OrigTxt.Text = TextGenerator.GenerateText(int.Parse(LenOfArr.Text));
            }
            catch { MessageBox.Show("Введите количество слов"); }
        }

        private void MeasureBtn_Click(object sender, RoutedEventArgs e)
        {
            double[][] measures = MeasureTime();

            List<Measure> measureTable = new List<Measure>();
            for (int i = 0; i < measures.Length; i++)
            {
                measureTable.Add(new Measure
                {
                    Count = measures[i][0].ToString()!,
                    QuickSortTime = measures[i][1].ToString()!,
                    RadixSortTime = measures[i][2].ToString()!
                });
            }

            MeasureTable.ItemsSource = measureTable;
        }

        private double[][] MeasureTime()
        {
            int[] intervals = { 100, 500, 1000, 2000, 5000, 10000, 15000 };
            double[][] measures = new double[intervals.Length][];

            for (int i = 0; i < intervals.Length; i++)
            {
                string text = TextGenerator.GenerateText(intervals[i]);

                Stopwatch swQ = Stopwatch.StartNew();
                QuickSortStrings QS = new QuickSortStrings(text);
                swQ.Stop();

                Stopwatch swR = Stopwatch.StartNew();
                RadixSortStrings RQ = new RadixSortStrings(text);
                swR.Stop();

                measures[i] = new double[3];
                measures[i][0] = intervals[i];
                measures[i][1] = swQ.Elapsed.TotalMilliseconds;
                measures[i][2] = swR.Elapsed.TotalMilliseconds;
            }

            return measures;
        }

        public class Measure
        {
            public string Count { get; set; }
            public string QuickSortTime { get; set; }
            public string RadixSortTime { get; set; }
        }
    }
}
