using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class MessageCustom<T>
    {
        public T Item1 { get; }

        public MessageCustom(T t1)
        {
            Item1 = t1;
        }
    }
    public class MessageCustom<T1, T2>
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }

        public MessageCustom(T1 t1, T2 t2)
        {
            Item1 = t1;
            Item2 = t2;
        }
    }
    public class MessageCustom<T1, T2, T3>
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public T3 Item3 { get; }
        public MessageCustom(T1 t1, T2 t2, T3 t3)
        {
            Item1 = t1;
            Item2 = t2;
            Item3 = t3;
        }
    }
    public class MessageCustom<T1, T2, T3, T4>
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public T3 Item3 { get; }
        public T4 Item4 { get; }

        public MessageCustom(T1 t1, T2 t2, T3 t3, T4 t4)
        {
            Item1 = t1;
            Item2 = t2;
            Item3 = t3;
            Item4 = t4;
        }
    }
}
