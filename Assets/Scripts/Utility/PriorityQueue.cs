using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;

public class PriorityQueue<T1, T2> where T2 : System.IComparable
{

    static readonly ProfilerMarker s_PopMarker = new("PriorityQueue.Pop");

    static readonly ProfilerMarker s_PushMarker = new("PriorityQueue.Push");

    //(T1 element, T2 priority)[] _elements = new (T1, T2)[1000];
    readonly List<(T1 element, T2 priority)> _elements = new();
    readonly IComparer _getBestPriority;
    /// <summary>
    /// Generates a new <see cref="PriorityQueue{T1, T2}"/>
    /// </summary>
    /// <param name="max">True if an element has greater priority when T2 is greater, false if an element has greater priority when T2 is lesser.</param>
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

    public int Count => _elements.Count();
    public List<(T1 element, T2 priority)> Elements => _elements;
    //{ get; private set; } = 0;

    public bool Empty => Count <= 0;

    public void Clear()
    {
        //Count = 0;
        _elements.Clear();
    }

    public T1 Pop()
    {
        using (s_PopMarker.Auto())
        {
            /*Array.Sort(_elements, _getBestPriority);
            Count--;
            T1 element = _elements[Count].element;
            _elements[Count] = default;
            return element;*/
            (T1, T2) element = _elements.Find(x => EqualityComparer<T2>.Default.Equals(x.priority, _elements.Min(x => x.priority)));
            _elements.Remove(element);
            return element.Item1;
        }
    }

    public T1 PopMax()
    {
        using (s_PopMarker.Auto())
        {
            /*Array.Sort(_elements, _getBestPriority);
            Count--;
            T1 element = _elements[Count].element;
            _elements[Count] = default;
            return element;*/
            (T1, T2) element = _elements.Find(x => EqualityComparer<T2>.Default.Equals(x.priority, _elements.Max(x => x.priority)));
            _elements.Remove(element);
            return element.Item1;
        }
    }

    public void Push(T1 element, T2 priority, bool replace = false)
    {
        using (s_PushMarker.Auto())
        {
            if (replace)
            {
                /*for (int i = 0; i < Count; i++)
                {
                    if (EqualityComparer<T1>.Default.Equals(_elements[i].element, element))
                    {
                        if (_getBestPriority.Compare(_elements[i], (element, priority)) > 0)
                        {
                            _elements[i].priority = priority;
                        }
                        return;
                    }
                }*/
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

            //_elements[Count] = (element, priority);
            //Count++;
        }
    }

    class MaxComparer : IComparer
    { 
        public int Compare(object x, object y)
        {
            return (((T1 element, T2 priority))x).priority.CompareTo((((T1 element, T2 priority))y).priority);
        }
    }

    class MinComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            return (((T1 element, T2 priority))y).priority.CompareTo((((T1 element, T2 priority))x).priority);
        }
    }
}
//Timescale: 1 FrameTick == 10 seconds.
//Time value is measured in frame ticks.
