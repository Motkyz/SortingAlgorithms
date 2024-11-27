using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;

namespace SortingAlgorithms.Pages
{
    public partial class InternalSorting : Page
    {
        CancellationTokenSource? _cancellationTokenSource;

        Brush active = Brushes.Blue;
        Brush swapping = Brushes.Yellow;
        Brush final = Brushes.Green;

        List<Rectangle> background = new List<Rectangle>();

        List<Rectangle> rectangles = new List<Rectangle>();
        List<Rectangle> originals = new List<Rectangle>();
        List<string> logs = new List<string>();
        public InternalSorting()
        {
            InitializeComponent();
        }

        public void DrawRects()
        {
            long[] array = ArrayGenerator.CreateArray();

            for (int i = 0; i < array.Length * 2; i++)
            {
                Rectangle back = new Rectangle()
                {
                    Fill = Brushes.White,
                    Height = canvas.ActualHeight,
                    Width = canvas.ActualWidth / 40,
                    Margin = new Thickness(10, 0, 0, 5)
                };

                background.Add(back);
                canvas.Children.Add(back);
                Canvas.SetBottom(back, 0);
                Canvas.SetLeft(back, i * canvas.ActualWidth / 40);
            }

            for (int i = 0; i < array.Length; i++)
            {
                Rectangle rect = new Rectangle()
                {
                    Fill = active,
                    Height = array[i] * canvas.ActualHeight / 20,
                    Width = canvas.ActualWidth / 40,
                    Margin = new Thickness(10, 0, 0, 5)
                };

                rectangles.Add(rect);
                originals.Add(rect);
                canvas.Children.Add(rect);
                Canvas.SetBottom(rect, 0);
                Canvas.SetLeft(rect, i * canvas.ActualWidth / 20);
            }
        }

        private void Draw_Click(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
            rectangles.Clear();
            originals.Clear();
            background.Clear();
            logTxt.Text = "";
            logs.Clear();
            DrawRects();
        }

        private async void Sort_Click(object sender, RoutedEventArgs e)
        {
            logTxt.Text = "";
            logs.Clear();

            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                string selectedSort = ((ComboBoxItem)Sorts.SelectedItem).Content.ToString()!;
                switch (selectedSort)
                {
                    case "Bubble Sort":
                        await BubbleSort();
                        break;
                    case "Select Sort":
                        await SelectSort();
                        break;
                    case "Quick Sort":
                        await QuickSortStart();
                        break;
                    case "Heap Sort":
                        await HeapSort();
                        break;
                    default:
                        MessageBox.Show("Сначала выберите алгоритм");
                        break;
                }
            }

            catch (OperationCanceledException)
            {
                Reset();
            }

            catch (NullReferenceException)
            {
                MessageBox.Show("Сначала создайте массив и выберите алгоритм сортировки");
            }
        }

        private async Task Swap(int firstRect, int secondRect)
        {
            _cancellationTokenSource!.Token.ThrowIfCancellationRequested();

            if (firstRect != secondRect)
            {
                await UpdateLog($"Меняем местами элементы №{secondRect + 1} и №{firstRect + 1}");
            }
            else
            {
                await UpdateLog($"Элемент №{secondRect + 1} на своём месте");
            }

            rectangles[firstRect].Fill = swapping;
            rectangles[secondRect].Fill = swapping;
            await Task.Delay((int)DelaySlider.Value / 2);

            await Task.Run(async () =>
                await SwapAsync(firstRect, secondRect));

            await Task.Delay((int)DelaySlider.Value / 2);
            rectangles[firstRect].Fill = active;
            rectangles[secondRect].Fill = active;
        }

        public Task SwapAsync(int firstRect, int secondRect)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                (rectangles[firstRect], rectangles[secondRect]) = (rectangles[secondRect], rectangles[firstRect]);

                Canvas.SetLeft(rectangles[firstRect], firstRect * canvas.ActualWidth / 20);
                Canvas.SetLeft(rectangles[secondRect], secondRect * canvas.ActualWidth / 20);
            });

            return Task.CompletedTask;
        }

        private Task UpdateLog(string comment)
        {
            logs.Add($"{logs.Count + 1}) {comment}");
            logTxt.Text = string.Join("\n", logs);
            return Task.CompletedTask;
        }

        private void cansel_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
        }

        private void Reset()
        {
            canvas.Children.Clear();
            logTxt.Text = "";
            logs.Clear();

            for (int i = 0; i < originals.Count; i++)
            {
                rectangles[i].Fill = active;
                rectangles[i] = originals[i];
                canvas.Children.Add(rectangles[i]);
                Canvas.SetBottom(rectangles[i], 0);
                Canvas.SetLeft(rectangles[i], i * canvas.ActualWidth / 20);
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedSort = ((ComboBoxItem)Sorts.SelectedItem).Content.ToString()!;
            description.Text = Description.GetDesc(selectedSort);
        }

        private void ToStart_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            _ = NavigationService.Navigate(new Uri("/Pages/Start.xaml", UriKind.Relative));
        }
    }
}
