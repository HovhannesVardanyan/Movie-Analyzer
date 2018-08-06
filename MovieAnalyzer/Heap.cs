using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieAnalyzer
{
    /// <summary>
    /// 
    /// This is the implementation of Heap Data Structure, that is used for Movie Analyzer
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class Heap<T> 
    {
        private List<T> register = new List<T>();
        public int Count => register.Count;
        public readonly Func<T, T, bool> condition;
        public bool isEmpty { get => register.Count == 0; }
        public T Root {
            get {
                if (register.Count == 0)
                    throw new Exception("The heap is empty!");
                return register[0];
            } }
        public Heap(Func<T,T,bool> condition)
        {
            this.condition = condition;   
        }
        public T Extract()
        {
            if (register.Count == 0)
                throw new Exception("The heap is empty");
            T ans = register[0];
            Swap(0, register.Count - 1);

            register.RemoveAt(register.Count - 1);

            int i = 0;
            while (2 * i + 2 < register.Count)
            {
                if (!condition(register[i], register[2 * i + 1]) || !condition(register[i], register[2 * i + 2]))
                    i = Swap(i, 2 * i + 1, 2 * i + 2);
                else
                    break;
            }
            if (2 * i + 1 < register.Count && !condition(register[i], register[2 * i + 1]))
                Swap(i,2*i + 1);

            return ans;
        }
        private void Add(T value)
        {
            register.Add(value);
            if (value is IID v)
                v.ID = register.Count - 1;
        }
        public void Insert(T item)
        {
            this.Add(item);
            int i = register.Count - 1;

            int j = (i - 1) / 2;
            while (j != i)
            {
                if (!condition(register[j], register[i]))
                    Swap(j, i);
                i = j;
                j = (i - 1) / 2;
                
            }
        }
        private void Swap(int i, int j)
        {
            T temp = register[i];
            register[i] = register[j];
            register[j] = temp;
            if (register[i] is IID ri)
                ri.ID = i;

            if (register[j] is IID rj)
                rj.ID = j;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            int height = (int)(Math.Log10(register.Count) / Math.Log10(2));
            int width = 1;
            
            for (int h = 0; h <= height; h++)
            {
                for (int i = width - 1; i < width * 2 - 1 && i < register.Count; i++)
                    sb.Append(register[i] + "  ");
                sb.AppendLine();
                width *= 2;
            }
            return sb.ToString();
        }
        private int Swap(int elem, int c1, int c2)
        {
            int c = c1;
            if (condition(register[c1], register[c2]))
                Swap(c1, elem);
            else
            {
                Swap(c2, elem);
                c = c2;
            }
            return c;
        }
        public static T[] Sort(T[] arr,Func<T,T,bool> condition)
        {
            if (arr.Length <= 1)
                return arr;
            if (!(arr[0] is IComparable<T>))
            {
                throw new ArgumentException("The array items should be Comparable!");
            }
            Heap<T> heap = new Heap<T>(condition);
            foreach (var item in arr)
            {
                heap.Insert(item);
            }
            T[] ans = new T[arr.Length];

            for (int i = 0; i < ans.Length; i++)
                ans[i] = heap.Extract();

            return ans;
        }
        public static int Median(int[] arr)
        {
            if (arr.Length == 0)
                throw new Exception("Empty array !");
            Heap<int> heap = new Heap<int>((a,b) => a>b);

            foreach (var item in arr)
                heap.Insert(item);
            int ans = arr[0];
            for (int i = 0; i <= arr.Length / 2; i++)
            {
                ans = heap.Extract();
            }
            return ans;
        }
        public static double[] RunningMedians(int[] arr)
        {
            Heap<int> left = new Heap<int>((a,b) => a > b);
            Heap<int> right = new Heap<int>((a,b) => a < b);

            double[] ans = new double[arr.Length];

            for (int i = 0; i < arr.Length; i++)
            {
                InsertNumber(arr[i], left, right);
                Balance(left,right);
                ans[i] = ExtractMedian(left, right);
            }

            return ans;
        }
        private static void Balance(Heap<int> left, Heap<int> right)
        {
            if (Math.Abs(left.Count - right.Count) > 1)
            {
                var bigger = (left.Count > right.Count) ? left : right;
                var smaller = (left.Count < right.Count) ? left : right;
                smaller.Insert(bigger.Extract());
            }
        }
        private static void InsertNumber(int num, Heap<int> left, Heap<int> right)
        {
            if (left.Count == 0 || left.Root > num)
                left.Insert(num);
            else
                right.Insert(num);
        }
        private static double ExtractMedian(Heap<int> left, Heap<int> right)
        {
            if (left.Count == right.Count)
                return ((double)left.Root + right.Root) / 2;

                return (left.Count > right.Count) ? left.Root : right.Root;
        }
        public void Delete(int i)
        {
            Swap(i, register.Count - 1);
            register.RemoveAt(register.Count - 1);

            while (2 * i + 2 < register.Count)
            {
                if (!condition(register[i], register[2 * i + 1]) || !condition(register[i], register[2 * i + 2]))
                    i = Swap(i, 2 * i + 1, 2 * i + 2);
                else
                    break;
            }
            if (2 * i + 1 < register.Count && !condition(register[i], register[2 * i + 1]))
                Swap(i, 2 * i + 1);

        }

    }
    interface IID
    {
        int ID { get; set; }
    }
}
