using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AlgoLab5
{
    public static class Program
    {
        public static void Sort()
        {
            string f1 = "f0.txt";
            string f2 = "f1.txt";
            string f3 = "f2.txt";
            bool end = false;

            Sequence s1 = new Sequence();
            Sequence s2 = new Sequence();
            Sequence s3 = new Sequence();
            List<Sequence> list = new List<Sequence>() {s1, s2, s3};

            do
            {
                DistributionPhase(list);
                MergePhase(list);
            } while (s1.CountOfSeries != 1);
        }

        public static void DistributionPhase(List<Sequence> list)
        {
            string f1 = "f0.txt";
            string f2 = "f1.txt";
            string f3 = "f2.txt";
            
            
            list[0].StartRead(f1);   
            list[1].StartWrite(f2);
            list[2].StartWrite(f3);
            list[1].SizeOfSeries =  list[0].SizeOfSeries;
            list[2].SizeOfSeries =   list[0].SizeOfSeries;
            while (!  list[0].Eof)
            {
                list[1].StartCopy(  list[0]);
                list[2].StartCopy(  list[0]);
            }
            
            list[0].CloseReader();
            list[1].CloseWriter();
            list[2].CloseWriter();
        }

        public static void MergePhase(List<Sequence> list)
        {
            string f1 = "f0.txt";
            string f2 = "f1.txt";
            string f3 = "f2.txt";
            bool end = false;

           
            
            list[0].StartWrite(f1);   
            list[1].StartRead(f2);
            list[2].StartRead(f3);
            
            while (!list[1].Eof)
            {
                list[0].Merge(new List<Sequence>(){ list[1],list[2]});
            }

            list[0].SizeOfSeries *= 2;
            list[0].CloseWriter();
            list[1].CloseReader();
            list[2].CloseReader();
        }
        

        public static void Main()
        {
           Sort();
        }
    }
}