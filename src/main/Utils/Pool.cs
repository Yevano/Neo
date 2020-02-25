using System;
using System.Collections.Generic;

namespace Neo.Utils {
    public sealed class Pool<E> {
        private readonly Dictionary<int, E> data;
        private readonly Dictionary<E, int> index;
        private int current;

        public Pool() {
            data = new Dictionary<int, E>();
            index = new Dictionary<E, int>();
        }

        public bool Contains(E key) {
            return index.ContainsKey(key);
        }

        public int Add(E key) {
            if (index.ContainsKey(key)) {
                throw new InvalidOperationException();
            }

            var i = current;
            current++;
            index[key] = i;
            data[i] = key;
            return i;
        }

        public int IndexOf(E key) {
            if(!index.ContainsKey(key)) {
                throw new InvalidOperationException();
            }

            return index[key];
        }

        public E[] ToArray() {
            var a = new E[current];
            for (var i = 0; i < a.Length; i++) {
                a[i] = data[i];
            }
            return a;
        }

        public int this[E key] {
            get {
                if (index.ContainsKey(key)) {
                    return index[key];
                } else {
                    return Add(key);
                }
            }
        }

        public E this[int index] {
            get {
                if(data.ContainsKey(index)) {
                    return data[index];
                } else {
                    throw new InvalidOperationException();
                }
            }
        }

        public int Count => current;
    }
}