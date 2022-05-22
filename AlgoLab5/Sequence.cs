using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.IO;
using System.Runtime;

namespace AlgoLab5
{
    public class Sequence
    {
        public int CurElem { get; set; } //указатель на текущий элемент
        public bool Eof { get; set; } //конец файла

        public int SizeOfSeries { get; set; } = 1;
        public int CountOfSeries { get; set; } = 0;
        public StreamReader Reader { get; set; } 
        public StreamWriter Writer { get; set; }
        
        
        public void ReadNext() //чтение след элемента
        {
            Eof = Reader.EndOfStream;
            if (!Eof)
                CurElem = Int32.Parse(Reader.ReadLine() ?? throw new Exception());
        }

        public void StartRead(string path) //начать чтение
        {
            CountOfSeries = 0;
            Reader = new StreamReader(path);
            ReadNext();
        }

        public void StartWrite(string path) //начать запись
        {
            Writer = new StreamWriter(path);
        }

        public void Copy(Sequence other) //копировать в this след элемент из другого файла
        {
            CurElem = other.CurElem;
            Writer.WriteLine(CurElem);
            other.ReadNext();
        }

        public void StartCopy(Sequence other) // копировать в this серию из другого файла
        {
            for (int i = 0; i < other.SizeOfSeries; i++)
            {
                if(!other.Eof)
                    Copy(other);
            }
        }

        public void Merge(List<Sequence> list)
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