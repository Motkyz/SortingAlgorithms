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

        private List<Dictionary<string, string>> table = new List<Dictionary<string, string>>();
        private string inputFilePath;
        private string outputFilePath = "../../../Files/output.csv";
        public ExternalSorting()
        {
            InitializeComponent();
        }

        string CSVDataBase = "../../../Files/output.csv";
        ICollection CreateDataSource()
        {
            string text = File.ReadAllText(CSVDataBase);
            string[] lines = text.Split("\r\n");

            //Create new DataTables and Rows
            DataTable dt = new DataTable();
            DataRow dr;

            for (int i = 0; i < lines[0].Split(',').Length;i++)
            {
                dt.Columns.Add(new DataColumn(lines[0].Split(',')[i], typeof(string)));
            }

            //For each line in the File
            for (int i = 1; i<lines.Length;i++)
            {
                //Create new Row
                dr = dt.NewRow();
                //Split lines at delimiter ';''
                for (int j = 0; j < lines[i].Split(',').Length; j++)
                {
                    dr[j] = lines[i].Split(',')[j];
                }

                //Add the row we created
                dt.Rows.Add(dr);
            }

            //Return Dataview 
            DataView dv = new DataView(dt);
            return dv;
        }

        public void LoadCsvToListView()
        {            
            try
            {
                DataListView.ItemsSource = CreateDataSource();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при чтении файла: " + ex.Message);
            }
        }

        private async void SortBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                string selectedAttribute = Attributes.SelectedItem.ToString()!;
                string selectedSort = ((ComboBoxItem)Sorts.SelectedItem).Content.ToString()!;

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

                SaveDataToFile();

                LoadCsvToListView();
            }
            catch (OperationCanceledException) { }
            catch { MessageBox.Show("Выберите файл"); }

        }
        private void LoadDataFromFile()
        {
            if (!File.Exists(inputFilePath)) return;

            table.Clear();
            var lines = File.ReadAllLines(inputFilePath);
            if (lines.Length == 0) return;

            var headers = lines[0].Split(',');
            foreach (var line in lines.Skip(1))
            {
                var values = line.Split(',');
                var record = new Dictionary<string, string>();
                for (int i = 0; i < headers.Length; i++)
                {
                    record[headers[i]] = values[i];
                }
                table.Add(record);
            }

            Attributes.ItemsSource = headers;
            Attributes.SelectedIndex = 0;
        }

        private void SaveDataToFile()
        {
            var headers = table.First().Keys.ToArray();
            var lines = new List<string> { string.Join(",", headers) };

            foreach (var record in table)
            {
                lines.Add(string.Join(",", record.Values));
            }

            File.WriteAllLines(outputFilePath, lines);
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
    }
}
