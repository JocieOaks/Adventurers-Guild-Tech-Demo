using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts.Utility
{
    /// <summary>
    /// The <see cref="PriorityQueue{T1, T2}"/> class is a collection that sorts items by priority and then pops the item with the highest priority when dequeued.
    /// </summary>
    /// <typeparam name="T1">The type of the elements being placed in the queue.</typeparam>
    /// <typeparam name="T2">The type of the element's priority. Must implement <see cref="System.IComparable"/>.</typeparam>
    public class PriorityQueue<T1, T2> where T2 : System.IComparable
    {
        private readonly PairingHeap<T1, T2> _heap;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriorityQueue{T1, T2}"/> class.
        /// </summary>
        /// <param name="comparer">An <see cref="IComparer"/> used to determine the order of priority for entries into the <see cref="PriorityQueue{T1, T2}"/>.</param>
        public PriorityQueue(IComparer comparer)
        {
            _heap = new PairingHeap<T1, T2>(comparer);
        }

        /// <value>The number of elements in the <see cref="PriorityQueue{T1, T2}"/>.</value>
        public int Count { get; private set; }

        /// <value>Returns true if the <see cref="PriorityQueue{T1, T2}"/> has no elements.</value>
        public bool Empty => EqualityComparer<T1>.Default.Equals(_heap.RootElement(), default);

        /// <summary>
        /// Change the priority of a particular heap node.
        /// </summary>
        /// <param name="node">The node being modified, as an <see cref="IReference"/>.</param>
        /// <param name="priority">The new priority for the node.</param>
        public void ChangePriority(IReference node, T2 priority)
        {
            if (!_heap.ChangePriority(node, priority))
                Count++;
        }

        /// <summary>
        /// Removes all the elements in the <see cref="PriorityQueue{T1, T2}"/>.
        /// </summary>
        public void Clear()
        {
            _heap.Clear();
            Count = 0;
        }

        /// <summary>
        /// Gives the element with the best priority and then removes it from the <see cref="PriorityQueue{T1, T2}"/>.
        /// </summary>
        /// <returns>Returns the element with the best priority.</returns>
        public T1 Pop()
        {
            Count--;
            return _heap.Pop();
        }

        /// <summary>
        /// Add a new element to the <see cref="PriorityQueue{T1, T2}"/>.
        /// </summary>
        /// <param name="element">The element being added to the <see cref="PriorityQueue{T1, T2}"/>.</param>
        /// <param name="priority">The priority associated with <c>element</c>.</param>
        /// <returns>Returns an <see cref="IReference"/> to the entry in the <see cref="PriorityQueue{T1, T2}"/>. Used to make changes in priority to the entry without needing to search the queue.</returns>
        public IReference Push(T1 element, T2 priority)
        {
            Count++;
            return _heap.Insert(element, priority);
        }

        /// <value>The priority of the entry with the highest priority in the <see cref="PriorityQueue{T1, T2}"/>.</value>
        public T2 TopPriority => _heap.RootPriority;

        /// <summary>
        /// The <see cref="MaxComparer"/> class is an <see cref="IComparer"/> where a greater value when using <see cref="IComparer.Compare(object, object)"/> has higher priority.
        /// </summary>
        public class MaxComparer : IComparer
        {
            public static MaxComparer Instance { get; } = new();

            /// <inheritdoc/>
            public int Compare(object x, object y)
            {
                return (((T2)x)!).CompareTo((T2)y);
            }
        }

        /// <summary>
        /// The <see cref="MinComparer"/> class is an <see cref="IComparer"/> where a lesser value when using <see cref="IComparer.Compare(object, object)"/> has higher priority.
        /// </summary>
        public class MinComparer : IComparer
        {
            public static MinComparer Instance { get; } = new();

            /// <inheritdoc/>
            public int Compare(object x, object y)
            {
                return (((T2)y)!).CompareTo((T2)x);
            }
        }
    }
}
