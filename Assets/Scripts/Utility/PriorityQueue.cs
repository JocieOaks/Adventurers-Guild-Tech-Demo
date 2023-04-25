using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using Unity.VisualScripting.ReorderableList.Element_Adder_Menu;
using UnityEditor;

// This is a very quick and simple implementation. Need to actually implement a binary heap or the like, or find a library that has a better implementationg.

/// <summary>
/// The <see cref="PriorityQueue{T1, T2}"/> class is a collection that sorts items by priority and then pops the item with the highest priority when dequeued.
/// </summary>
/// <typeparam name="T1">The type of the elements being placed in the queue.</typeparam>
/// <typeparam name="T2">The type of the element's priority. Must implement <see cref="System.IComparable"/>.</typeparam>
public class PriorityQueue<T1, T2> where T2 : System.IComparable
{

    PairingHeap<T1,T2> _heap;

    /// <summary>
    /// Initializes a new instance of the <see cref="PriorityQueue{T1, T2}"/> class.
    /// </summary>
    /// <param name="max"> Sets whether an element has greater priority when T2 is greater or lesser.</param>
    public PriorityQueue(bool max)
    {
        _heap = new PairingHeap<T1, T2>(max);
    }

    /// <value>The number of elements in the <see cref="PriorityQueue{T1, T2}"/>.</value>
    public int Count { get; private set; }

    /// <value>Returns true if the <see cref="PriorityQueue{T1, T2}"/> has no elements.</value>
    public bool Empty => _heap.RootElement() == null;

    /// <summary>
    /// Change the priority of a particular heap node.
    /// </summary>
    /// <param name="node">The node being modified, as an <see cref="ILockBox"/>.</param>
    /// <param name="priority">The new priority for the node.</param>
    public void ChangePriority(ILockBox node, T2 priority)
    {
        _heap.ChangePriority(node, priority);
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
    public ILockBox Push(T1 element, T2 priority)
    {
        Count++;
        return _heap.Insert(element, priority);
    }
}
