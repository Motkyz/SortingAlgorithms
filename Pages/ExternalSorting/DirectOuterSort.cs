using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using System.Xml.Linq;

namespace SortingAlgorithms.Pages
{
    public partial class ExternalSorting : Page
    {        
        public async Task DirectOuterSort(int keyInd)
        {
            _keyInd = keyInd;
            _sizeOfBlocks = 1;

        
            await UpdateLog($"~~~Запускаем сортировку прямым слиянием по аттрибуту \"{_attribute}\"~~~\n");
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

                await SplitToFiles();

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

                //Если после разделения цепочка осталась одна, значит, записи в файле отсортированы
                if (_segments == 1)
                {
                    await UpdateLog("\nЗаписи уместились в одном файле => файл отсортирован\n");
                    break;
                }

                //Сливаем вместе цепочки из под файлов
                await UpdateLog("\nСЛИЯНИЕ ПОДФАЙЛОВ\n");
                await Task.Delay((int)DelaySlider.Value);

                await MergePairs();

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
        private async Task SplitToFiles()
        {
            _segments = 1;
            using var fileA = new StreamReader(outputFilePath);
            _headers = fileA.ReadLine()!;

            using var fileB = new StreamWriter("B.csv");
            using var fileC = new StreamWriter("C.csv");

            string? currentRecord = fileA.ReadLine();
            bool flag = true;
            int counter = 0;

            await UpdateLog($"Заполняем подфайлы поочерёдно по {_sizeOfBlocks} записи/ей в файл\n");
            await Task.Delay((int)DelaySlider.Value);

            //цикл прекратится, когда мы дойдём до конца исходного файла
            while (currentRecord is not null)
            {
                _cancellationTokenSource!.Token.ThrowIfCancellationRequested();
                //дошли до конца цепочки, переключаемся на запись новой
                if (counter == _sizeOfBlocks)
                {
                    counter = 0;
                    flag = !flag;
                    _segments++;
                }

                if (flag)
                {
                    //Запись отправляется в подфайл В
                    await UpdateLog($"==> Запись отправляется в подфайл B\n\"{currentRecord}\"\n");
                    await Task.Delay((int)DelaySlider.Value);

                    fileB.WriteLine(currentRecord);
                }
                else
                {
                    //Запись отправляется в подфайл С
                    await UpdateLog($"==> Запись отправляется в подфайл C\n\"{currentRecord}\"\n");
                    await Task.Delay((int)DelaySlider.Value);

                    fileC.WriteLine(currentRecord);
                }

                //считываем следующую запись
                currentRecord = fileA.ReadLine();
                counter++;
            }  
        }

        private async Task MergePairs()
        {
            using var writerA = new StreamWriter(outputFilePath);
            using var readerB = new StreamReader("B.csv");
            using var readerC = new StreamReader("C.csv");

            //Не забываем вернуть заголовки таблицы на своё место, в начало исходного файла
            writerA.WriteLine(_headers);

            string? elementB = readerB.ReadLine();
            string? elementC = readerC.ReadLine();

            int counterB = 0;
            int counterC = 0;
            //Итерации будут происходить, когда 
            while (elementB is not null || elementC is not null)
            {
                _cancellationTokenSource!.Token.ThrowIfCancellationRequested();
                string? currentRecord;
                bool flag = false;

                //Обрабатываем случай, когда закончился весь файл B, или цепочка из данной пары в нём
                if (elementB is null || counterB == _sizeOfBlocks)
                {
                    currentRecord = elementC;
                }
                else if (elementC is null || counterC == _sizeOfBlocks) //аналогично предыдущему блоку if, но для подфайла С
                {
                    currentRecord = elementB;
                    flag = true;
                }
                else
                {
                    //Если оба подфайла ещё не закончились, то сравниваем записи по нужному полю
                    await UpdateLog($"Сравниваем записи из подфайлов B и C по аттрибуту ~{_attribute}~\n" +
                        $"B: {elementB}\n" +
                        $"C: {elementC}\n");
                    await Task.Delay((int)DelaySlider.Value);

                    if (CompareElements(elementB.Split(',')[_keyInd], elementC.Split(',')[_keyInd]) < 0)
                    {
                        //Если запись из файла В оказалась меньше
                        await UpdateLog($"\"{elementB.Split(',')[_keyInd]}\" < \"{elementC.Split(',')[_keyInd]}\"\n");
                        await Task.Delay((int)DelaySlider.Value);

                        currentRecord = elementB;
                        flag = true;
                    }
                    else
                    {
                        //Если запись из файла С оказалась меньше
                        await UpdateLog($"\"{elementC.Split(',')[_keyInd]}\" > \"{elementB.Split(',')[_keyInd]}\"\n");
                        await Task.Delay((int)DelaySlider.Value);

                        currentRecord = elementC;
                    }
                }

                //Записываем в исходный файл выбранную нами запись
                if (elementB is null || counterB == _sizeOfBlocks)
                {
                    await UpdateLog($"Для записи из файла C не достало пары из файла B\nC: \"{currentRecord}\"\n");
                }
                else if (elementC is null || counterC == _sizeOfBlocks) //аналогично предыдущему блоку if, но для подфайла С
                {
                    await UpdateLog($"Для записи из файла B не достало пары из файла C\nB: \"{currentRecord}\"\n");
                }

                await UpdateLog($"==> Записываем в результирующий файл\n\"{currentRecord}\"\n\n");
                await Task.Delay((int)DelaySlider.Value);

                writerA.WriteLine(currentRecord);

                if (flag)
                {
                    elementB = readerB.ReadLine();

                    await UpdateLog($"Следующая запись B:\n" +
                    $"\"{elementB}\"\n");

                    counterB++;
                }
                else
                {
                    elementC = readerC.ReadLine();

                    await UpdateLog($"Следующая запись C:\n" +
                    $"\"{elementC}\"\n");

                    counterC++;
                }

                if (counterB != _sizeOfBlocks || counterC != _sizeOfBlocks)
                {
                    continue;
                }

                //Если серии в обоих файлах закончились, то обнуляем соответствующие счётчики
                counterC = 0;
                counterB = 0;
            }

            _sizeOfBlocks *= 2;
        }
    }
}