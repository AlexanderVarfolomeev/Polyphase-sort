using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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
        
        public static void PolyPhase(int countOfFiles)
        {
            List<int> A = new List<int>();
            List<int> D = new List<int>();
            List<Sequence> list = new List<Sequence>(); //= new List<Sequence>() {a1, a2, a0};
            for (int i = 0; i < countOfFiles; i++)
            {
                list.Add(new Sequence());
                A.Add(1);
                D.Add(1);
            }

            A[A.Count - 1] = 0;
            D[D.Count - 1] = 0;
            int T = countOfFiles;
            int L = 1;
            int a = 0;
            int P = countOfFiles - 1;
            int j = 0;


            while (!list[P].Eof)
            {
                //Запись
                list[P].StartRead("f" + P + ".txt");
                for (int i = 0; i < P; i++)
                {
                    list[i].StartWrite("f" + i + ".txt");
                }


                while (!list[P].Eof)
                {
                    list[j].StartCopy(list[P]); //list[j].StartCopy(a0);
                    D[j]--;
                    if (!list[P].Eof)
                    {
                        if (D[j] < D[j + 1])
                        {
                            j++;
                        }
                        else
                        {
                            if (D[j] == 0)
                            {
                                L++;
                                a = A[0];
                                for (int k = 0; k < P; k++)
                                {
                                    D[k] = a + A[k + 1] - A[k];
                                    A[k] = a + A[k + 1];
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

                //Слияние
            }

            #region Костыльная_часть
            for (int i = 0; i < P; i++)
            {
                list[i].StartRead();
                List<int> number = new List<int>();
                while (!list[i].Reader.EndOfStream)
                {
                    number.Add(int.Parse(list[i].Reader.ReadLine()));
                }

                list[i].countOfEmptyElements = list[i].CountOfSeries - number.Count;
                list[i].CloseReader();
            }
            #endregion

            
            int k1 = 0;
            foreach (var sequence in list)
            {
                //sequence.CountOfSeries += D[k1++];
                sequence.CountOfSeries = A[k1++];
            }            
            while(L != 0)
            {
                
                
                list[P].StartWrite();
                for (int i = 0; i < P; i++)
                {
                    list[i].StartRead();
                }

                while (list[P - 1].CountOfSeries != 0 /* && D[P - 1] == 0*/)
                {
                    bool flag = true;
                    for (int i = 0; i < P; i++)
                    {
                        if (D[i] <= 0) //ПРОВЕРЯЕМ ВЕЗДЕ ЛИ ЕСТЬ ПУСТЫЕ СЕРИИ
                            flag = false;
                    }

                    if (flag)
                    {
                        D[P]++;
                        for (int i = 0; i < P; i++)
                        {
                            D[i]--; //ТАКИМ ОБРАЗОМ ЕСЛИ У НАС ВЕЗДЕ ЕСТЬ ПУСТЫЕ СЕРИИ ТО СНАЧАЛА СЧИТЫВАеМ ИХ
                            list[i].CountOfSeries--;//////
                            list[i].countOfEmptyElements -= list[i].SizeOfSeries;
                            list[P].countOfEmptyElements += list[i].SizeOfSeries;
                        }

                        list[P].CountOfSeries++;
                        
                    }
                    else
                    {
                        var mergeList = new List<Sequence>();
                        for (int i = 0; i < P; i++)
                        {
                            if (D[i] == 0)
                            {
                                mergeList.Add(list[i]);
                            }
                            else
                            {
                                list[i].CountOfSeries--;
                                list[i].countOfEmptyElements -= list[i].SizeOfSeries;
                                list[P].countOfEmptyElements += list[i].SizeOfSeries;
                                D[i]--; //ТАКИМ ОБРАЗОМ МЫ СЧИТАЕМ ЧТО ПУСТЫЕ СЕРИИ НАХОДЯТСЯ В НАЧАЛЕ
                            }
                        }

                        list[P].Merge(mergeList);
                    }
                }

                list[P].CloseWriter();
                int series = 0;
                for (int i = 0; i < P; i++)
                {
                    list[i].CloseReader();
                    series += list[i].SizeOfSeries;
                }

                list[P].SizeOfSeries = series;
                DeleteEmptySeries(list.Take(list.Count - 1).ToList(), D);

                /////Опущение
                L--;
                var tmp = list[P];
                for (int i = P; i > 0; i--)
                {
                    list[i] = list[i - 1];
                }

                list[0] = tmp;

                var tmp1 = D[P];
                for (int i = P; i > 0; i--)
                {
                    D[i] = D[i - 1];
                }

                D[0] = tmp1;


                Console.WriteLine();
            }

        }

        public static void DeleteEmptySeries(List<Sequence> list,  List<int> D)
        {
            
            int j = 0;
            foreach (var sequence in list)
            {
                StreamReader file1 = new StreamReader(sequence.Filename);

                List<int> arr = new List<int>();
                while (!file1.EndOfStream)
                {
                    arr.Add(int.Parse(file1.ReadLine()));
                } //СЪУЯЛИ ТУТ 8 ЭЛЕМОВ ДОЛЖНО БЫТЬ 3

                file1.Close();

                StreamWriter streamWriter = new StreamWriter(sequence.Filename);
                int countNeed = sequence.SizeOfSeries * sequence.CountOfSeries;
                arr.Reverse();
                arr = arr.Take(countNeed).ToList();
                arr.Reverse();
                foreach (var i in arr)
                {
                    streamWriter.WriteLine(i);
                }

                j++;
                streamWriter.Close();
            }
        }


        public static void Main()
        {
            PolyPhase(3);
            //Select();
            //   PolyPhaseSort(3);
        }
    }
}