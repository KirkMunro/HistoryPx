using System.Collections.Generic;

namespace HistoryPx
{
    public class QueuedDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        Queue<TKey> queue;

        int capacity;
        public int Capacity
        {
            get
            {
                return capacity;
            }
            set
            {
                int itemsToRemove = value - capacity;
                while (itemsToRemove > 0)
                {
                    Remove(queue.Dequeue());
                    itemsToRemove--;
                }
                capacity = value;
            }
        }

        public QueuedDictionary(int capacity) : base(capacity)
        {
            this.capacity = capacity;
            queue = new Queue<TKey>(capacity);
        }

        new public TValue this[TKey key]
        {
            get
            {
                return base[key];
            }
            set
            {
                Add(key, value);
            }
        }

        new public void Add(TKey key, TValue value)
        {
            if (queue.Count == Capacity)
            {
                base.Remove(queue.Dequeue());
            }
            base.Add(key, value);
            queue.Enqueue(key);
        }

        new public void Clear()
        {
            base.Clear();
            queue.Clear();
        }

        new public bool Remove(TKey key)
        {
            if (Comparer.Equals(queue.Peek(), key))
            {
                return base.Remove(queue.Dequeue());
            }

            bool result = base.Remove(key);
            if (result)
            {
                Queue<TKey> newQueue = new Queue<TKey>(capacity);
                while (queue.Count > 0)
                {
                    TKey item = queue.Dequeue();
                    if (!base.Comparer.Equals(item, key))
                    {
                        newQueue.Enqueue(item);
                    }
                }
                queue = newQueue;
            }
            return result;
        }
    }
}
