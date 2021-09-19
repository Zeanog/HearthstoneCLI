using System;
using System.Collections.Generic;

namespace Neo.Utility {
    public class DataStructureLibrary<T> : IDisposable {
        //TODO: Should make this a struct at some point
        public class CheckoutSlip : IDisposable {
            public T Value {
                get;
                protected set;
            }

            public CheckoutSlip(T data)
            {
                Value = data;
            }

            public static implicit operator T(CheckoutSlip slip) => slip.Value;

            public void Dispose()
            {
                Instance.Return(this);
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }

        public readonly static DataStructureLibrary<T> Instance = new DataStructureLibrary<T>();

        protected LinkedList<T> available = new LinkedList<T>();
        protected LinkedList<T> checkedOut = new LinkedList<T>();

        public CheckoutSlip CheckOut(params object[] constructorArgs)
        {
            if (available.Count <= 0)
            {
                T inst = (T)Activator.CreateInstance(typeof(T), constructorArgs);
                System.Diagnostics.Debug.Assert(inst != null);
                available.AddLast(inst);
            }

            LinkedListNode<T> node = available.Last;
            available.Remove(node.Value);

            checkedOut.AddLast(node);

            return new CheckoutSlip(node.Value);
        }

        protected void Return(CheckoutSlip slip)
        {
            LinkedListNode<T> node = checkedOut.Find(slip.Value);
            if (node == null)
            {
                slip = null;
                return;
            }

            checkedOut.Remove(node);
            available.AddLast(node);
        }

        // Hiding the contructor
        protected DataStructureLibrary()
        {
        }

        public void Dispose()
        {
            System.Diagnostics.Debug.Assert(checkedOut.Count <= 0);
            Type tType = typeof(T);
            var methodInfo = tType.GetMethod("Dispose", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (methodInfo != null)
            {
                foreach (var node in available)
                {
                    methodInfo.Invoke(node, null);
                }
            }
        }
    }
}