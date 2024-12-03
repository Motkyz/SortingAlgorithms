using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SortingAlgorithms.Pages
{
    public partial class ExternalSorting : Page
    {
        CancellationTokenSource? _cancellationTokenSource;
        List<string> logs = new List<string>();

        private string inputFilePath;
        public string outputFilePath = "output.csv";

        private string? _headers;
        private int _keyInd;
        private string _attribute;
        private long _sizeOfBlocks, _segments;

        public ExternalSorting()
        {
            InitializeComponent();
        }

        private async void SortBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            logs.Clear();
            logTxt.Text = "";
            try
            {
                string selectedSort = Sorts.Text;
                int selectedAttribute = Attributes.SelectedIndex;
                _attribute = Attributes.Text;
                switch (selectedSort)
                {
                    case "Прямое слияние":
                        await DirectOuterSort(selectedAttribute);
                        break;
                    case "Естественное слияние":
                        await NaturalOuterSort(selectedAttribute);
                        break;
                    case "Многопутевое слияние":
                        await MultiThreadOuterSort(selectedAttribute);
                        break;
                    default:
                        MessageBox.Show("Выберите метод слияния.");
                        break;
                }
            }
            catch (OperationCanceledException) { }          
            catch { MessageBox.Show("Выберите файл"); }
        }

        private void SelectFileBtn_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                inputFilePath = openFileDialog.FileName;
                LoadDataFromFile();
            }
        }

        private void cansel_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
        }

        private void ToStart_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            _ = NavigationService.Navigate(new Uri("/Pages/Start.xaml", UriKind.Relative));
        }

        private void LoadDataFromFile()
        {
            if (!File.Exists(inputFilePath)) return;

            var lines = File.ReadAllLines(inputFilePath);
            if (lines.Length == 0) return;
            var output = new StreamWriter(outputFilePath);
            foreach (var line in lines)
            {
                output.WriteLine(line);
            }
            output.Close();

            var headers = lines[0];

            Attributes.ItemsSource = headers.Split(',');
            Attributes.SelectedIndex = 0;
        }

        public int CompareElements(string firstEl, string secondEl) //-1, если первое меньше второго, 0, если равны, 1, если первое больше второго
        {
            string firstElDouble = firstEl.Replace('.', ',');
            if (double.TryParse(firstElDouble, out double firstRes))
            {
                string secondElDouble = secondEl.Replace('.', ',');
                double secondRes = double.Parse(secondElDouble);
                if (firstRes < secondRes) return -1;
                if (firstRes > secondRes) return 1;
                return 0;
            }
            else return firstEl.CompareTo(secondEl);
        }

        public async Task UpdateLog(string comment)
        {
            await Task.Run(async () =>
                await Log(comment));
        }

        private void Sorts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedSort = ((ComboBoxItem)Sorts.SelectedItem).Content.ToString()!;
            descTxt.Text = Description.GetDesc(selectedSort);
        }

        private Task Log(string comment)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                logs.Add($"{comment}");
                logTxt.Text = string.Join("\n", logs);
            });
            return Task.CompletedTask;
        }
    }
}
