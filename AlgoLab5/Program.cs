using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AlgoLab5
{
    public static class Program
    {
        private static void PolyPhase(int countOfFiles)
        {
            List<int> fibonacciDistribution = new List<int>(); // точное фибоначчиево распредедение к которому мы стремимся
            List<int> emptySeries = new List<int>(); // число фиктивных серий присутствующих на начале ленты j
            List<Tape> list = new List<Tape>(); // список лент
            for (int i = 0; i < countOfFiles; i++)
            {
                list.Add(new Tape());
                fibonacciDistribution.Add(1);
                emptySeries.Add(1);
            }

            fibonacciDistribution[fibonacciDistribution.Count - 1] = 0;
            emptySeries[emptySeries.Count - 1] = 0;
            int level = 1;
            int a = 0; // просто вспомогательное число?
            int P = countOfFiles - 1; // количество файлов - 1
            int j = 0; // текущий обрабатываемый файл

            HorizontalDistribution(list, P, j, emptySeries, fibonacciDistribution, a, ref level);
            PrintResult(list, P, emptySeries);
            MergePhase(list, P, emptySeries, fibonacciDistribution, level);
        }

        /// <summary>
        /// Опускаемся на один уровень и делаем сдвиг массивов вправо.
        /// </summary>
        private static void LevelDown(List<Tape> list, int P, List<int> emptySeries, ref int level)
        {
            level--;
            var tmp = list[P];
            for (int i = P; i > 0; i--)
            {
                list[i] = list[i - 1];
            }

            list[0] = tmp;
            var tmp1 = emptySeries[P];
            for (int i = P; i > 0; i--)
            {
                emptySeries[i] = emptySeries[i - 1];
            }

            emptySeries[0] = tmp1;
        }

        private static void MergePhase(List<Tape> list, int P, List<int> emptySeries, List<int> fibonacciDistribution,
            int level)
        {
            int k = 0;
            

            for (int i = 1; i < P; i++)
            {
                list[i].StartRead();
            }

            while (level != 0) // пока не дойдем до 0 уровня
            {
                list[0].StartRead();
                list[P].StartWrite();

                while (list[P - 1].CountOfSeries != 0)
                {
                    bool flag = true;
                    for (int i = 0; i < P; i++)
                    {
                        if (emptySeries[i] <= 0) // проверяем везде ли находятся пустые серии
                            flag = false;
                    }

                    if (flag) // если везде есть пустые серии, то сливаем их в одну серию в выходной файл
                    {
                        emptySeries[P]++;
                        for (int i = 0; i < P; i++)
                        {
                            emptySeries[
                                i]--; // таким образом если у нас везде есть пустые серии, то считаем что они в начале файла и считываем их
                            list[i].CountOfSeries--;
                        }

                        list[P].CountOfSeries++;
                    }
                    else
                    {
                        var mergeList = new List<Tape>();
                        for (int i = 0; i < P; i++)
                        {
                            if (emptySeries[i] == 0) // если в файле у нас нет пустых серий, то считываем обычную серию
                            {
                                mergeList.Add(list[i]);
                            }
                            else // если в файле есть пустые серии, то считываем сначала их
                            {
                                list[i].CountOfSeries--;
                                emptySeries[i]--;
                            }
                        }

                        list[P].Merge(mergeList);
                    }
                }
                
                PrintResult(list, P, emptySeries);
                
                list[P].CloseWriter();
                list[P - 1].CloseReader();
                int series = 0;

                for (int i = 0; i < P; i++)
                {
                    series += list[i].SizeOfSeries;
                }

                list[P].SizeOfSeries = series; // присваиваем новый размер серии выходному файлу
                LevelDown(list, P, emptySeries, ref level);
            }

            for (int i = 1; i < P; i++)
            {
                list[i].CloseReader();
            }
        }


        private static void HorizontalDistribution(List<Tape> list, int P, int j, List<int> emptySeries,
            List<int> fibonacciDistribution, int a,
            ref int level)
        {
            while (!list[P].Eof) // считываем файл до конца
            {
                //Запись
                list[P].StartRead("f" + P + ".txt");
                for (int i = 0; i < P; i++)
                {
                    list[i].StartWrite("f" + i + ".txt");
                }

                while (!list[P].Eof)
                {
                    list[j].StartCopy(list[P]);
                    emptySeries[j]--;
                    if (!list[P].Eof)
                    {
                        if (emptySeries[j] < emptySeries[j + 1])
                        {
                            j++;
                        }
                        else
                        {
                            if (emptySeries[j] == 0)
                            {
                                level++;
                                a = fibonacciDistribution[0];
                                for (int k = 0; k < P; k++)
                                {
                                    emptySeries[k] = a + fibonacciDistribution[k + 1] - fibonacciDistribution[k];
                                    fibonacciDistribution[k] = a + fibonacciDistribution[k + 1];
                                }

                                j = 0;
                            }
                            else
                            {
                                j = 0;
                            }
                        }
                    }
                }

                list[P].CloseReader();
                for (int i = 0; i < P; i++)
                {
                    list[i].CloseWriter();
                }

                int tmp = 0;
                foreach (var sequence in list)
                {
                    sequence.CountOfSeries = fibonacciDistribution[tmp++]; // распределение количества серий по фибоначчи
                }
            }
        }

        public static void PrintResult(List<Tape> list, int P,  List<int> emptySeries)
        {
            Dictionary<Tape, int> printMap = new Dictionary<Tape, int>();
            for (int i = 0; i <= P; i++)
            {
                printMap.Add(list[i], emptySeries[i]);
            }

            var Map = printMap.OrderBy(x => x.Key.Filename);
            

            foreach (var i in Map)
            {
                Console.Write(i.Key.Filename + "\t\t");
            }
            
            Console.WriteLine();
            
            foreach (var i in Map)
            {
                Console.Write(i.Key.CountOfSeries + "(" + i.Value + ")" + "\t\t");
            }

            Console.WriteLine();
        }

        private static void Menu()
        {
            while (true)
            {
                int countOfFiles;
                Console.WriteLine("Введите количество файлов участвующих в сортировке (минимум 3 файла).");
                while (!int.TryParse(Console.ReadLine(), out countOfFiles) || countOfFiles < 3)
                {

                    Console.WriteLine("Неверный ввод. Введите минимум 3 файла.");
                }

                int count;
                Console.WriteLine("Введите количество начальных элементов в входном файле.");
                while (!int.TryParse(Console.ReadLine(), out count) || count <= 0)
                {
                    Console.WriteLine("Неверный ввод. Повторите.");
                }
                FillFile(count, countOfFiles - 1 );
                PolyPhase(countOfFiles);
                Console.WriteLine("Выйти? (y)");
                if (Console.ReadLine() == "y")
                    break;
            }
        }

        private static void FillFile(int count, int fileName)
        {
            StreamWriter file = new StreamWriter("f" + fileName + ".txt");
            Random rnd = new Random();
            for (int i = 0; i < count; i++)
            {
                file.WriteLine(rnd.Next(-999999, 999999));
            }
            file.Close();
        }
        
        
        
        public static void Main()
        {
            Menu();
        }
    }
}