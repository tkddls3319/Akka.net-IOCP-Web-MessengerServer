using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.Server
{
    public class MessageCustomCommand<T>
    {
        public T Item1 { get; }
        public MessageCustomCommand(T t1)
        {
            Item1 = t1;
        }
    }
    public class MessageCustomCommand<T1, T2>
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public MessageCustomCommand(T1 t1, T2 t2)
        {
            Item1 = t1;
            Item2 = t2;
        }
    }
    public class MessageCustomCommand<T1, T2, T3>
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public T3 Item3 { get; }
        public MessageCustomCommand(T1 t1, T2 t2, T3 t3)
        {
            Item1 = t1;
            Item2 = t2;
            Item3 = t3;
        }
    }
    public class MessageCustomCommand<T1, T2, T3, T4>
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public T3 Item3 { get; }
        public T4 Item4 { get; }
        public MessageCustomCommand(T1 t1, T2 t2, T3 t3, T4 t4)
        {
            Item1 = t1;
            Item2 = t2;
            Item3 = t3;
            Item4 = t4;
        }
    }

    public class MessageCustomQuery<T>
    {
        public T Item1 { get; }
        public MessageCustomQuery(T t1)
        {
            Item1 = t1;
        }
    }
    public class MessageCustomQuery<T1, T2>
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public MessageCustomQuery(T1 t1, T2 t2)
        {
            Item1 = t1;
            Item2 = t2;
        }
    }
    public class MessageCustomQuery<T1, T2, T3>
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public T3 Item3 { get; }
        public MessageCustomQuery(T1 t1, T2 t2, T3 t3)
        {
            Item1 = t1;
            Item2 = t2;
            Item3 = t3;
        }
    }
    public class MessageCustomQuery<T1, T2, T3, T4>
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public T3 Item3 { get; }
        public T4 Item4 { get; }
        public MessageCustomQuery(T1 t1, T2 t2, T3 t3, T4 t4)
        {
            Item1 = t1;
            Item2 = t2;
            Item3 = t3;
            Item4 = t4;
        }
    }

    public class MessageCustomResponse <T>
    {
        public T Item1 { get; }
        public MessageCustomResponse (T t1)
        {
            Item1 = t1;
        }
    }
    public class MessageCustomResponse <T1, T2>
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public MessageCustomResponse (T1 t1, T2 t2)
        {
            Item1 = t1;
            Item2 = t2;
        }
    }
    public class MessageCustomResponse <T1, T2, T3>
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public T3 Item3 { get; }
        public MessageCustomResponse (T1 t1, T2 t2, T3 t3)
        {
            Item1 = t1;
            Item2 = t2;
            Item3 = t3;
        }
    }
    public class MessageCustomResponse <T1, T2, T3, T4>
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public T3 Item3 { get; }
        public T4 Item4 { get; }
        public MessageCustomResponse (T1 t1, T2 t2, T3 t3, T4 t4)
        {
            Item1 = t1;
            Item2 = t2;
            Item3 = t3;
            Item4 = t4;
        }
    }
}
