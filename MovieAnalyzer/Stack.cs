using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieAnalyzer
{
    /// <summary>
    /// 
    /// Implementation of Stack Data Structure, that is used in Graph class
    /// 
    /// </summary>
    class Stack<T>
    {
        private LinkedList<T> list;
        public Stack()
        {
            list = new LinkedList<T>();
        }

        public void Push(T item)
        {
            list.AddLast(item);
        }

        public T Pop()
        {
            if (this.IsEmpty)
                throw new StackEmptyException("The stack is empty, cannot pop an item!");

            T item = list.Last.Value;

            list.RemoveLast();

            return item;
        }

        public bool IsEmpty
        { get {
                return list.Last == null;
            }
        }
        

        public class StackEmptyException : ApplicationException
        {
            public StackEmptyException(string msg) : base(msg)
            {

            }
        }
        
    }
}
