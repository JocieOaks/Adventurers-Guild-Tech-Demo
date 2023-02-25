using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;

// This is a very quick and simple implementation. Need to actually implement a binary heap or the like, or find a library that has a better implementationg.

/// <summary>
/// The <see cref="PriorityQueue{T1, T2}"/> class is a collection that sorts items by priority and then pops the item with the highest priority when dequeued.
/// </summary>
/// <typeparam name="T1">The type of the elements being placed in the queue.</typeparam>
/// <typeparam name="T2">The type of the element's priority. Must implement <see cref="System.IComparable"/>.</typeparam>
public class PriorityQueue<T1, T2> where T2 : System.IComparable
{

    static readonly ProfilerMarker s_PopMarker = new("PriorityQueue.Pop");

    static readonly ProfilerMarker s_PushMarker = new("PriorityQueue.Push");

    readonly List<(T1 element, T2 priority)> _elements = new();
    readonly IComparer _getBestPriority;
    /// <summary>
    /// Initializes a new instance of the <see cref="PriorityQueue{T1, T2}"/> class.
    /// </summary>
    /// <param name="max"> Sets whether an element has greater priority when T2 is greater or lesser.</param>
    public PriorityQueue(bool max)
    {
        if (max)
        {
            _getBestPriority = new MaxComparer();
        }
        else
        {
            _getBestPriority = new MinComparer();
        }
    }

    /// <value>The number of elements in the <see cref="PriorityQueue{T1, T2}"/>.</value>
    public int Count => _elements.Count();

    /// <value>Returns true if the <see cref="PriorityQueue{T1, T2}"/> has no elements.</value>
    public bool Empty => Count <= 0;

    /// <summary>
    /// Removes all the elements in the <see cref="PriorityQueue{T1, T2}"/>.
    /// </summary>
    public void Clear()
    {
        _elements.Clear();
    }

    /// <summary>
    /// Gives the element with the smallest priority and then removes it from the <see cref="PriorityQueue{T1, T2}"/>.
    /// </summary>
    /// <returns>Returns the element with the smallest priority.</returns>
    public T1 PopMin()
    {
        using (s_PopMarker.Auto())
        {
            (T1, T2) element = _elements.Find(x => EqualityComparer<T2>.Default.Equals(x.priority, _elements.Min(x => x.priority)));
            _elements.Remove(element);
            return element.Item1;
        }
    }

    /// <summary>
    /// Gives the element with the largest priority and then removes it from the <see cref="PriorityQueue{T1, T2}"/>.
    /// </summary>
    /// <returns>Returns the element with the largest priority.</returns>
    public T1 PopMax()
    {
        using (s_PopMarker.Auto())
        {
            (T1, T2) element = _elements.Find(x => EqualityComparer<T2>.Default.Equals(x.priority, _elements.Max(x => x.priority)));
            _elements.Remove(element);
            return element.Item1;
        }
    }

    /// <summary>
    /// Add a new element to the <see cref="PriorityQueue{T1, T2}"/>.
    /// </summary>
    /// <param name="element">The element being added to the <see cref="PriorityQueue{T1, T2}"/>.</param>
    /// <param name="priority">The priority associated with <c>element</c>.</param>
    /// <param name="replace">When true, if the <see cref="PriorityQueue{T1, T2}"/> alreadt has <c>element</c> in the queue, it will only keep the element with the highest priority.</param>
    public void Push(T1 element, T2 priority, bool replace = false)
    {
        using (s_PushMarker.Auto())
        {
            if (replace)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (EqualityComparer<T1>.Default.Equals(_elements[i].element, element))
                    {
                        if (_getBestPriority.Compare(_elements[i], (element, priority)) > 0)
                        {
                            _elements[i] = (element, priority);
                        }
                        return;
                    }
                }

            }

            _elements.Add((element, priority));

        }
    }

    /// <summary>
    /// The <see cref="MaxComparer"/> class is an <see cref="IComparer"/> where a greater value when using <see cref="IComparer.Compare(object, object)"/> has higher priority.
    /// </summary>
    class MaxComparer : IComparer
    { 
        /// <inheritdoc/>
        public int Compare(object x, object y)
        {
            return (((T1 element, T2 priority))x).priority.CompareTo((((T1 element, T2 priority))y).priority);
        }
    }

    /// <summary>
    /// The <see cref="MinComparer"/> class is an <see cref="IComparer"/> where a lesser value when using <see cref="IComparer.Compare(object, object)"/> has higher priority.
    /// </summary>
    class MinComparer : IComparer
    {
        /// <inheritdoc/>
        public int Compare(object x, object y)
        {
            return (((T1 element, T2 priority))y).priority.CompareTo((((T1 element, T2 priority))x).priority);
        }
    }
}
