using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.IO;
using System.Linq;
using System.Runtime;

namespace AlgoLab5
{
    public class Tape
    {
        private int CurElem { get; set; } //указатель на текущий элемент
        public bool Eof { get; set; } //конец файла
        private string Filename { get; set; }
        public int SizeOfSeries { get; set; } = 1;
        public int CountOfSeries { get; set; } = 0;
        private StreamReader Reader { get; set; }
        private StreamWriter Writer { get; set; }


        private void ReadNext() //чтение след элемента
        {
            Eof = Reader.EndOfStream;
            if (!Eof)
                CurElem = Int32.Parse(Reader.ReadLine() ?? throw new Exception());
        }

        public void StartRead(string path) //начать чтение
        {
            Filename = path;
            //CountOfSeries = 0;
            Reader = new StreamReader(path);
            ReadNext();
        }

        public void StartWrite(string path) //начать запись
        {
            Filename = path;
            Writer = new StreamWriter(path);
        }

        public void StartRead() //начать чтение
        {
            Reader = new StreamReader(Filename);
            ReadNext();
        }

        public void StartWrite() //начать запись
        {
            Writer = new StreamWriter(Filename);
        }

        private void Copy(Tape other) //копировать в this след элемент из другого файла
        {
            CurElem = other.CurElem;
            Writer.WriteLine(CurElem);
            other.ReadNext();
        }

        public void StartCopy(Tape other) // копировать в this серию из другого файла
        {
            for (int i = 0; i < other.SizeOfSeries; i++)
            {
                if (!other.Eof)
                    Copy(other);
            }

            CountOfSeries++;
        }

        public void Merge(List<Tape> list)
        {
            List<int> numbers = new List<int>();
            foreach (var sequence in list)
            {
                for (int i = 0; i < sequence.SizeOfSeries; i++)
                {
                    if (!sequence.Eof)
                    {
                        numbers.Add(sequence.CurElem);
                        sequence.ReadNext();
                    }
                }

                sequence.CountOfSeries--;
            }

            numbers.Sort();
            foreach (var number in numbers)
            {
                Writer.WriteLine(number);
            }

            CountOfSeries++;
        }

        public void CloseReader()
        {
            Reader.Close();
        }

        public void CloseWriter()
        {
            Writer.Close();
        }
    }
}