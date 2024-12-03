using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SortingAlgorithms.Pages
{
    public partial class ExternalSorting : Page
    {
        private readonly List<int> _series = new();

        public async Task NaturalOuterSort(int keyInd)
        {
            _keyInd = keyInd;


            await UpdateLog($"~~~Запускаем сортировку естественным слиянием по аттрибуту \"{_attribute}\"~~~\n");
            await Task.Delay((int)DelaySlider.Value);

            await UpdateLog("\nИСХОДНЫЙ ФАЙЛ\n");
            var records = File.ReadAllLines(outputFilePath);
            foreach (var record in records)
            {
                await UpdateLog($"{record}");
            }
            await Task.Delay((int)DelaySlider.Value);

            while (true)
            {
                _cancellationTokenSource!.Token.ThrowIfCancellationRequested();
                _series.Clear();
                //Разбиваем записи на подфайлы
                await UpdateLog("\nРАЗБИВАЕМ ЗАПИСИ ПО ПОДФАЙЛАМ\n");
                await Task.Delay((int)DelaySlider.Value);

                await SplitToFilesForNat();

                await UpdateLog("\nПОЛУЧИВШИЕСЯ ФАЙЛЫ\n");
                await UpdateLog("Файл B:");
                records = File.ReadAllLines("B.csv");
                foreach (var record in records)
                {
                    await UpdateLog($"{record}");
                }
                await Task.Delay((int)DelaySlider.Value);

                records = File.ReadAllLines("C.csv");
                if (records.Length > 0)
                    await UpdateLog("\nФайл C:");
                foreach (var record in records)
                {
                    await UpdateLog($"{record}");
                }


                //Если у нас осталась всего одна серия, значит, записи в файле отсортированы
                if (_series.Count == 1)
                {
                    await UpdateLog("\nЗаписи уместились в одном файле => файл отсортирован\n");
                    break;
                }

                //Сливаем вместе цепочки из под файлов
                await UpdateLog("\nСЛИЯНИЕ ПОДФАЙЛОВ\n");
                await Task.Delay((int)DelaySlider.Value);

                await MergePairsForNat();

                await UpdateLog("\nПРОМЕЖУТОЧНЫЙ РЕЗУЛЬТАТ\n");
                records = File.ReadAllLines(outputFilePath);
                foreach (var record in records)
                {
                    await UpdateLog($"{record}");
                }
            }

            await UpdateLog("\nОТСОРТИРОВАННЫЙ ФАЙЛ\n");
            records = File.ReadAllLines(outputFilePath);
            foreach (var record in records)
            {
                await UpdateLog($"{record}");
            }
            await Task.Delay((int)DelaySlider.Value);

            File.Delete("B.csv");
            File.Delete("C.csv");
        }
        private async Task SplitToFilesForNat()
        {
            using var fileA = new StreamReader(outputFilePath);
            _headers = fileA.ReadLine();

            using var fileB = new StreamWriter("B.csv");
            using var fileC = new StreamWriter("C.csv");

            string? firstStr = fileA.ReadLine();
            string? secondStr = fileA.ReadLine();

            await UpdateLog($"Считываем две подряд идущие строки, чтобы посмотреть прервалась серия или нет\n" +
                $"1) {firstStr}\n" +
                $"2) {secondStr}\n");
            await Task.Delay((int)DelaySlider.Value);

            bool flag = true;
            int counter = 0;
            while (firstStr is not null)
            {
                _cancellationTokenSource!.Token.ThrowIfCancellationRequested();

                bool tempFlag = flag;
                if (secondStr is not null)
                {
                    await UpdateLog($"Сравниваем записи по аттрибуту ~{_attribute}~\n" +
                        $"1) {firstStr}\n" +
                        $"2) {secondStr}\n");

                    if (CompareElements(firstStr.Split(',')[_keyInd], secondStr.Split(',')[_keyInd]) <= 0)
                    {
                        await UpdateLog($"\"{firstStr.Split(',')[_keyInd]}\" ≤ \"{secondStr.Split(',')[_keyInd]}\"\n");
                        await Task.Delay((int)DelaySlider.Value);

                        await UpdateLog($"Серия продолжается, запись добавляется в текущий файл");
                        counter++;
                    }
                    else
                    {
                        await UpdateLog($"\"{firstStr.Split(',')[_keyInd]}\" > \"{secondStr.Split(',')[_keyInd]}\"\n");
                        await Task.Delay((int)DelaySlider.Value);

                        await UpdateLog($"Серия прервалась, дальше запись будет идти во второй файл, длина серии - {counter + 1}");

                        tempFlag = !tempFlag;
                        _series.Add(counter + 1);
                        counter = 0;
                    }
                }

                if (flag)
                {
                    await UpdateLog($"==> Запись отправляется в подфайл B\n\"{firstStr}\"\n");
                    await Task.Delay((int)DelaySlider.Value);

                    fileB.WriteLine(firstStr);
                }
                else
                {
                    await UpdateLog($"==> Запись отправляется в подфайл C\n\"{firstStr}\"\n");
                    await Task.Delay((int)DelaySlider.Value);

                    fileC.WriteLine(firstStr);
                }

                //движемся к следующей записи
                firstStr = secondStr;
                secondStr = fileA.ReadLine();

                await UpdateLog($"Следующая запись:\n" +
                    $"\"{secondStr}\"\n");
                flag = tempFlag;
            }

            _series.Add(counter + 1);
        }

        private async Task MergePairsForNat()
        {
            using var writerA = new StreamWriter(outputFilePath);
            using var readerB = new StreamReader("B.csv");
            using var readerC = new StreamReader("C.csv");

            //Не забываем про заголовки
            writerA.WriteLine(_headers);
            //Индекс, по которому находится очередная серия в подфайле B
            int indexB = 0;
            //Индекс, по которому находится очередная серия в подфайле С
            int indexC = 1;
            //Счётчики, чтобы случайно не выйти за пределы серии
            int counterB = 0;
            int counterC = 0;
            string? elementB = readerB.ReadLine();
            string? elementC = readerC.ReadLine();
            //Цикл закончит выполнение только когда 
            while (elementB is not null || elementC is not null)
            {
                _cancellationTokenSource!.Token.ThrowIfCancellationRequested();

                if (counterB == _series[indexB] && counterC == _series[indexC])
                {
                    //Случай, когда мы дошли до конца серий в обоих подфайлах
                    counterB = 0;
                    counterC = 0;
                    indexB += 2;
                    indexC += 2;
                    continue;
                }

                if (indexB == _series.Count || counterB == _series[indexB])
                {
                    //Случай, когда мы дошли до конца серии в подфайле B
                    await UpdateLog($"Серия в подфайле B закончилась, переходим к серии файла C");

                    writerA.WriteLine(elementC);

                    await UpdateLog($"==> Записываем в результирующий файл\n\"{elementC}\"\n");
                    await Task.Delay((int)DelaySlider.Value);

                    elementC = readerC.ReadLine();

                    await UpdateLog($"Следующая запись C:\n" +
                    $"\"{elementC}\"\n");
                    counterC++;
                    continue;
                }

                if (indexC == _series.Count || counterC == _series[indexC])
                {
                    //Случай, когда мы дошли до конца серии в подфайле C
                    await UpdateLog($"Серия в подфайле C закончилась, переходим к серии файла B");

                    writerA.WriteLine(elementB);

                    await UpdateLog($"==> Записываем в результирующий файл\n\"{elementB}\"\n");
                    await Task.Delay((int)DelaySlider.Value);

                    elementB = readerB.ReadLine();

                    await UpdateLog($"Следующая запись B:\n" +
                    $"\"{elementB}\"\n");
                    counterB++;
                    continue;
                }

                //Сравниваем записи по заданному полю и вписывам в исходный файл меньшую из них
                await UpdateLog($"Сравниваем записи из подфайлов B и C по аттрибуту ~{_attribute}~\n" +
                        $"B: {elementB}\n" +
                        $"C: {elementC}\n");

                if (CompareElements(elementB!.Split(',')[_keyInd], elementC!.Split(',')[_keyInd]) <= 0)
                {
                    await UpdateLog($"\"{elementB.Split(',')[_keyInd]}\" ≤ \"{elementC.Split(',')[_keyInd]}\"\n");
                    await Task.Delay((int)DelaySlider.Value);

                    writerA.WriteLine(elementB);

                    await UpdateLog($"==> Записываем в результирующий файл\n\"{elementB}\"\n\n");
                    await Task.Delay((int)DelaySlider.Value);

                    elementB = readerB.ReadLine();

                    await UpdateLog($"Следующая запись B:\n" +
                    $"\"{elementB}\"\n");
                    counterB++;
                }
                else
                {
                    await UpdateLog($"\"{elementB.Split(',')[_keyInd]}\" > \"{elementC.Split(',')[_keyInd]}\"\n");
                    await Task.Delay((int)DelaySlider.Value);

                    writerA.WriteLine(elementC);

                    await UpdateLog($"==> Записываем в результирующий файл\n\"{elementC}\"\n\n");
                    await Task.Delay((int)DelaySlider.Value);

                    elementC = readerC.ReadLine();

                    await UpdateLog($"Следующая запись B:\n" +
                    $"\"{elementC}\"\n");
                    counterC++;
                }
            }
        }

    }
}
