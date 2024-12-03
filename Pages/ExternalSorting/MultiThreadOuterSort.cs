using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SortingAlgorithms.Pages
{
    public partial class ExternalSorting : Page
    {
        public async Task MultiThreadOuterSort(int keyInd)
        {
            _keyInd = keyInd;
            _sizeOfBlocks = 1;


            await UpdateLog($"~~~Запускаем сортировку многопутевым слиянием по аттрибуту \"{_attribute}\"~~~\n");
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

                //Разбиваем записи на подфайлы
                await UpdateLog("\nРАЗБИВАЕМ ЗАПИСИ ПО ПОДФАЙЛАМ\n");
                await Task.Delay((int)DelaySlider.Value);

                await SplitToFilesMulti();

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

                records = File.ReadAllLines("D.csv");
                if (records.Length > 0)
                    await UpdateLog("\nФайл D:");
                foreach (var record in records)
                {
                    await UpdateLog($"{record}");
                }

                if (_segments == 1)
                {
                    await UpdateLog("\nЗаписи уместились в одном файле => файл отсортирован\n");
                    break;
                }

                //Сливаем вместе цепочки из под файлов
                await UpdateLog("\nСЛИЯНИЕ ПОДФАЙЛОВ\n");
                await Task.Delay((int)DelaySlider.Value);

                await MergeFilesMulti();

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
            File.Delete("D.csv");
        }

        private async Task SplitToFilesMulti()
        {
            _segments = 1;
            using var fileA = new StreamReader(outputFilePath);
            _headers = fileA.ReadLine();

            using var fileB = new StreamWriter("B.csv");
            using var fileC = new StreamWriter("C.csv");
            using var fileD = new StreamWriter("D.csv");
            string? currentRecord = fileA.ReadLine();

            int flag = 0;
            int counter = 0;

            await UpdateLog($"Заполняем подфайлы поочерёдно по {_sizeOfBlocks} записи/ей в файл\n");
            await Task.Delay((int)DelaySlider.Value);

            while (currentRecord is not null)
            {
                _cancellationTokenSource!.Token.ThrowIfCancellationRequested();

                if (counter == _sizeOfBlocks)
                {
                    //Случай, когда мы дошли до конца цепочки
                    counter = 0;
                    flag = GetNextFileIndexToWrite(flag);
                    _segments++;
                }

                switch (flag)
                {
                    case 0:
                        //Запись отправляется в подфайл B
                        await UpdateLog($"==> Запись отправляется в подфайл B\n\"{currentRecord}\"\n");
                        await Task.Delay((int)DelaySlider.Value);

                        fileB.WriteLine(currentRecord);
                        break;
                    case 1:
                        //Запись отправляется в подфайл С
                        await UpdateLog($"==> Запись отправляется в подфайл C\n\"{currentRecord}\"\n");
                        await Task.Delay((int)DelaySlider.Value);

                        fileC.WriteLine(currentRecord);
                        break;
                    case 2:
                        //Запись отправляется в подфайл С
                        await UpdateLog($"==> Запись отправляется в подфайл D\n\"{currentRecord}\"\n");
                        await Task.Delay((int)DelaySlider.Value);

                        fileD.WriteLine(currentRecord);
                        break;
                }

                currentRecord = fileA.ReadLine();
                counter++;
            }
        }

        //Метод получения следующего индекса файла для записи (B = 0, C = 1, D = 2)
        private static int GetNextFileIndexToWrite(int currentIndex)
                => currentIndex switch
                {
                    0 => 1,
                    1 => 2,
                    2 => 0,
                    _ => throw new Exception()
                };

        private async Task MergeFilesMulti()
        {
            using var writerA = new StreamWriter(outputFilePath);

            using var readerB = new StreamReader("B.csv");
            using var readerC = new StreamReader("C.csv");
            using var readerD = new StreamReader("D.csv");

            writerA.WriteLine(_headers);

            string? elementB = readerB.ReadLine();
            string? elementC = readerC.ReadLine();
            string? elementD = readerD.ReadLine();

            int counterB = 0;
            int counterC = 0;
            int counterD = 0;
            while (elementB is not null || elementC is not null || elementD is not null)
            {
                _cancellationTokenSource!.Token.ThrowIfCancellationRequested();

                string? currentRecord = elementB;
                int flag = 0;

                if (CheckElement(elementB, counterB) && !CheckElement(elementC, counterC) && !CheckElement(elementD, counterD))
                {
                    //Случай, когда цепочка закончилась только в файле B
                    await UpdateLog($"В файле B закончился блок из {_sizeOfBlocks} записей\n" +
                        $"Выбираем минимальную запись из данного блока в оставшихся файлах по аттрибуту ~{_attribute}~\n" +
                        $"C: {elementC} => \"{elementC.Split(',')[_keyInd]}\"\n" +
                        $"D: {elementD} => \"{elementD.Split(',')[_keyInd]}\"\n");

                    int min = GetMinOfElements(elementC, elementD);
                    switch (min)
                    {
                        case 0:
                            (currentRecord, flag) = (elementC, 1);

                            await UpdateLog($"Минимальная => \"{elementC.Split(',')[_keyInd]}\"");
                            break;
                        case 1:
                            (currentRecord, flag) = (elementD, 2);

                            await UpdateLog($"Минимальная => \"{elementD.Split(',')[_keyInd]}\"");
                            break;
                    }
                }
                else if (CheckElement(elementC, counterC) && !CheckElement(elementB, counterB) && !CheckElement(elementD, counterD))
                {
                    //Случай, когда цепочка закончилась только в файле С
                    await UpdateLog($"В файле C закончился блок из {_sizeOfBlocks} записей\n" +
                        $"Выбираем минимальную запись из данного блока в оставшихся файлах по аттрибуту ~{_attribute}~\n" +
                        $"B: {elementB} => \"{elementB.Split(',')[_keyInd]}\"\n" +
                        $"D: {elementD} => \"{elementD.Split(',')[_keyInd]}\"\n");

                    int min = GetMinOfElements(elementB, elementD);
                    switch (min)
                    {
                        case 0:
                            (currentRecord, flag) = (elementB, 0);

                            await UpdateLog($"Минимальная => \"{elementB.Split(',')[_keyInd]}\"");
                            break;
                        case 1:
                            (currentRecord, flag) = (elementD, 2);

                            await UpdateLog($"Минимальная => \"{elementD.Split(',')[_keyInd]}\"");
                            break;
                    }
                }
                else if (CheckElement(elementD, counterD) && !CheckElement(elementB, counterB) && !CheckElement(elementC, counterC))
                {
                    //Случай, когда цепочка закончилась только в файле D
                    await UpdateLog($"В файле D закончился блок из {_sizeOfBlocks} записей\n" +
                        $"Выбираем минимальную запись из данного блока в оставшихся файлах по аттрибуту ~{_attribute}~\n" +
                        $"B: {elementB} => \"{elementB.Split(',')[_keyInd]}\"\n" +
                        $"C: {elementC} => \"{elementC.Split(',')[_keyInd]}\"\n");

                    int min = GetMinOfElements(elementB, elementC);
                    switch (min)
                    {
                        case 0:
                            (currentRecord, flag) = (elementB, 0);

                            await UpdateLog($"Минимальная => \"{elementB.Split(',')[_keyInd]}\"");
                            break;
                        case 1:
                            (currentRecord, flag) = (elementC, 1);

                            await UpdateLog($"Минимальная => \"{elementC.Split(',')[_keyInd]}\"");
                            break;
                    }
                }
                else if (CheckElement(elementB, counterB) && CheckElement(elementC, counterC) && !CheckElement(elementD, counterD))
                {
                    //Случай, когда цепочки закончились в файлах В и С
                    currentRecord = elementD;

                    await UpdateLog($"Для записи из файла D не достало пары из файлов B и C\nD: \"{currentRecord}\"\n");
                    flag = 2;
                }
                else if (CheckElement(elementB, counterB) && !CheckElement(elementC, counterC) && CheckElement(elementD, counterD))
                {
                    //Случай, когда цепочки закончились в файлах В и D
                    currentRecord = elementC;

                    await UpdateLog($"Для записи из файла C не достало пары из файлов B и D\nC: \"{currentRecord}\"\n");
                    flag = 1;
                }
                else if (!CheckElement(elementB, counterB) && CheckElement(elementC, counterC) && CheckElement(elementD, counterD))
                {
                    //Случай, когда цепочки закончились в файлах C и D
                    currentRecord = elementB;

                    await UpdateLog($"Для записи из файла B не достало пары из файла C и D\nB: \"{currentRecord}\"\n");
                    flag = 0;
                }
                else
                {
                    //Случай, когда не закончилась ни одна из 3 цепочек
                    await UpdateLog($"Выбираем минимальную запись среди всех файлов по аттрибуту ~{_attribute}~\n" +
                        $"B: {elementB} => \"{elementB.Split(',')[_keyInd]}\"\n" +
                        $"C: {elementC} => \"{elementC.Split(',')[_keyInd]}\"\n" +
                        $"D: {elementD} => \"{elementD.Split(',')[_keyInd]}\"\n");

                    int min = GetMinOfElements(elementB, elementC, elementD);
                    switch (min)
                    {
                        case 0:
                            (currentRecord, flag) = (elementB, 0);

                            await UpdateLog($"Минимальная => \"{elementB.Split(',')[_keyInd]}\"");
                            break;
                        case 1:
                            (currentRecord, flag) = (elementC, 1);

                            await UpdateLog($"Минимальная => \"{elementC.Split(',')[_keyInd]}\"");
                            break;
                        case 2:
                            (currentRecord, flag) = (elementD, 2);

                            await UpdateLog($"Минимальная => \"{elementD.Split(',')[_keyInd]}\"");
                            break;
                    }
                }

                switch (flag)
                {
                    case 0:
                        await UpdateLog($"==> Записываем в результирующий файл\n\"{currentRecord}\"\n\n");
                        await Task.Delay((int)DelaySlider.Value);

                        writerA.WriteLine(currentRecord);
                        elementB = readerB.ReadLine();

                        await UpdateLog($"Следующая запись B:\n" +
                            $"\"{elementB}\"\n");

                        counterB++;
                        break;
                    case 1:
                        await UpdateLog($"==> Записываем в результирующий файл\n\"{currentRecord}\"\n\n");
                        await Task.Delay((int)DelaySlider.Value);

                        writerA.WriteLine(currentRecord);
                        elementC = readerC.ReadLine();

                        await UpdateLog($"Следующая запись C:\n" +
                            $"\"{elementC}\"\n");

                        counterC++;
                        break;
                    case 2:
                        await UpdateLog($"==> Записываем в результирующий файл\n\"{currentRecord}\"\n\n");
                        await Task.Delay((int)DelaySlider.Value);

                        writerA.WriteLine(currentRecord);
                        elementD = readerD.ReadLine();

                        await UpdateLog($"Следующая запись D:\n" +
                            $"\"{elementD}\"\n");

                        counterD++;
                        break;
                }

                if (counterB != _sizeOfBlocks || counterC != _sizeOfBlocks || counterD != _sizeOfBlocks)
                {
                    continue;
                }

                //Обнуляем все 3 счётчика, если достигли конца всех цепочек во всех файлах
                counterC = 0;
                counterB = 0;
                counterD = 0;
            }

            _sizeOfBlocks *= 3;
        }

        //Ниже дан ряд методов для поиска минимального из 3 элементов (с учётом того),
        //что некоторые из них могут отсутствовать
        private int GetMinOfElements(params string?[] elements)
        {
            if (elements.Contains(null))
            {
                switch (elements.Length)
                {
                    case 2:
                        return elements[0] is null ? 1 : 0;
                    case 3 when elements[0] is null && elements[1] is null:
                        return 2;
                    case 3 when elements[0] is null && elements[2] is null:
                        return 1;
                    case 3 when elements[1] is null && elements[2] is null:
                        return 0;
                }
            }

            string doubleCheck = elements[0].Split(',')[_keyInd].Replace('.', ',');
            if (double.TryParse(doubleCheck, out _))
            {
                return GetMinDouble(elements
                    .Select(s => s is null ? double.MaxValue : double.Parse(s.Split(',')[_keyInd].Replace('.', ',')))
                    .ToArray());
            }

            return GetMinString(elements!);
        }

        private int GetMinString(IReadOnlyList<string> elements)
        {
            if (elements.Count == 1)
            {
                return 0;
            }

            var min = elements[0].Split(',')[_keyInd];
            var minIndex = 0;
            for (var i = 1; i < elements.Count; i++)
            {
                _cancellationTokenSource!.Token.ThrowIfCancellationRequested();

                if (CompareElements(elements[i].Split(',')[_keyInd], min) > 0)
                {
                    continue;
                }

                min = elements[i].Split(',')[_keyInd];
                minIndex = i;
            }

            return minIndex;
        }

        private int GetMinDouble(IReadOnlyList<double> elements)
        {
            if (elements.Count == 1)
            {
                return 0;
            }

            var min = elements[0];
            var minIndex = 0;
            for (var i = 1; i < elements.Count; i++)
            {
                _cancellationTokenSource!.Token.ThrowIfCancellationRequested();

                if (elements[i] > min)
                {
                    continue;
                }

                min = elements[i];
                minIndex = i;
            }

            return minIndex;
        }

        private bool CheckElement(string? element, int counter)
        {
            return element is null || counter == _sizeOfBlocks;
        }
    }
}
