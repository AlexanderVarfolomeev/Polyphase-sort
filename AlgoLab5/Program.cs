using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AlgoLab5
{
    public static class Program
    {
        public static void Sort(int n)
        {
            List<Sequence> list = new List<Sequence>(n);
            for (int i = 0; i < n; i++)
            {
                list.Add(new Sequence());
            }
            do
            {
                DistributionPhase(list);
                MergePhase(list);
            } while (list[0].CountOfSeries != 1);
        }

        public static void DistributionPhase(List<Sequence> list)
        {
            string f1 = "f0.txt";
            list[0].StartRead(f1);
            for (int i = 1; i < list.Count; i++)
            {
                list[i].StartWrite("f" + i + ".txt");
                list[i].SizeOfSeries = list[0].SizeOfSeries;
            }
            while (!list[0].Eof)
            {
                for (int i = 1; i < list.Count; i++)
                {
                    list[i].StartCopy(list[0]);
                }
            }
            
            list[0].CloseReader();
            for (int i = 1; i < list.Count; i++)
            {
                list[i].CloseWriter();
            }
        }

        public static void MergePhase(List<Sequence> list)
        {
            string f1 = "f0.txt";
            list[0].StartWrite(f1);
            for (int i = 1; i < list.Count; i++)
            {
                list[i].StartRead("f" + i + ".txt");
            }
            
            while (!list[1].Eof)
            {
                list[0].Merge(list.Skip(1).ToList());
            }

            list[0].SizeOfSeries *= 2;
            list[0].CloseWriter();
            for (int i = 1; i < list.Count; i++)
            {
                list[i].CloseReader();
            }
        }
        

        public static void Main()
        {
           Sort(6);
        }
    }
}