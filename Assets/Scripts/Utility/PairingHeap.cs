using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Utility
{
    /// <summary>
    /// The <see cref="PairingHeap{T1, T2}"/> class is a heap data structure for storing elements for <see cref="PriorityQueue{T1, T2}"/>.
    /// </summary>
    public class PairingHeap<T1, T2> where T2 : System.IComparable
    {

        private readonly IComparer _comparer;
        private Node _root;

        /// <value>The priority of the root entry in the <see cref="PairingHeap{T1, T2}"/>.</value>
        public T2 RootPriority => _root.Priority;

        /// <summary>
        /// Initializes a new instance of the <see cref="PairingHeap{T1, T2}"/> class.
        /// </summary>
        /// <param name="comparer"> Sets whether an element has greater priority when T2 is greater or lesser.</param>
        public PairingHeap(IComparer comparer)
        {
            _comparer = comparer;
        }

        /// <value>The root of the <see cref="PairingHeap{T1, T2}"/>.</value>
        private Node Root
        {
            get
            {
                if (_root != null)
                    while (_root.Parent != null)
                    {
                        _root = _root.Parent;
                    }

                return _root;
            }
            set => _root = value;
        }

        /// <summary>
        /// Change the priority of a particular <see cref="Node"/>. The <see cref="Node"/> may have previously been removed from the 
        /// <see cref="PairingHeap{T1, T2}"/> in which case it is added back into the <see cref="PairingHeap{T1, T2}"/>.
        /// </summary>
        /// <param name="reference">The <see cref="Node"/> being modified, as an <see cref="IReference"/>.</param>
        /// <param name="priority">The new priority for the <see cref="Node"/>.</param>
        public void ChangePriority(IReference reference, T2 priority)
        {
            Node node = (Node)reference;
            node.Parent?.Children.Remove(node);
            node.Parent = null;
            node.Priority = priority;

            Meld(node, Root);
        }

        /// <summary>
        /// Clear the <see cref="PairingHeap{T1, T2}"/> by setting the root to null.
        /// </summary>
        public void Clear()
        {
            Root = null;
        }

        /// <summary>
        /// Finds the element associated with the the root of the <see cref="PairingHeap{T1, T2}"/> which is also the element with the best priority.
        /// </summary>
        /// <returns>Returns the element with the best priority.</returns>
        public T1 RootElement()
        {
            if (Root == null)
                return default;
            return Root.Element;
        }

        /// <summary>
        /// Adds a new <see cref="Node"/> to the <see cref="PairingHeap{T1, T2}"/>.
        /// </summary>
        /// <param name="element">The element being sorted.</param>
        /// <param name="priority">The priority of <c>element</c>.</param>
        /// <returns>Returns the created <see cref="Node"/> as an <see cref="IReference"/> so that the entry can be modified in the future without needing to search the <see cref="PairingHeap{T1, T2}"/>.</returns>
        public IReference Insert(T1 element, T2 priority)
        {
            Node node = new Node(element, priority);
            if (Root == null)
            {
                Root = node;
            }
            else
                Meld(node, Root);

            return node;
        }

        /// <summary>
        /// Removes the best priority <see cref="Node"/> from the <see cref="PairingHeap{T1, T2}"/> and returns its associated element.
        /// </summary>
        /// <returns>The element with the best priority in the <see cref="PairingHeap{T1, T2}"/>.</returns>
        public T1 Pop()
        {
            Node root = Root;
            List<Node> children = root.Children;
            while (children.Count > 1)
            {
                for (int i = 0; i < children.Count - 1; i++)
                {
                    children.Remove(Meld(children[i], children[i + 1]));
                }
            }

            _root = children.FirstOrDefault();
            if (_root != null)
            {
                _root.Parent = null;
            }

            //Called because in some cases the root might be reinserted into the list using ChangePriority
            root.Children.Clear();

            return root.Element;
        }

        /// <summary>
        /// Melds the roots of two separate heaps to create one heap.
        /// </summary>
        /// <param name="node1">The first <see cref="Node"/> being melded.</param>
        /// <param name="node2">The second <see cref="Node"/> being melded.</param>
        /// <returns>Returns the <see cref="Node"/> that is not at the root of the new heap. Used in <see cref="Pop"/> to remove the lower
        /// priority <see cref="Node"/> from the list of possible roots.</returns>
        private Node Meld(Node node1, Node node2)
        {
            if (node1 == node2)
                return node1;
            if (_comparer.Compare(node1.Priority, node2.Priority) > 0)
            {
                node1.Children.Add(node2);
                node2.Parent = node1;
                return node2;
            }
            else
            {
                node2.Children.Add(node1);
                node1.Parent = node2;
                return node1;
            }
        }

        /// <summary>
        /// The <see cref="Node"/> class is a node for the <see cref="PairingHeap{T1, T2}"/> data structure.
        /// </summary>
        private class Node : IReference
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Node"/> class.
            /// </summary>
            /// <param name="element">The object being sorted into the <see cref="PairingHeap{T1, T2}"/>.</param>
            /// <param name="priority">The priority of <c>element</c>.</param>
            public Node(T1 element, T2 priority)
            {
                Element = element;
                Priority = priority;
            }

            /// <value>The child <see cref="Node"/>s of this <see cref="Node"/>.</value>
            public List<Node> Children { get; } = new();

            /// <value>The object of type <see cref="T1"/> being stored in the <see cref="Node"/>.</value>
            public T1 Element { get; }

            /// <value>The parent <see cref="Node"/> in the <see cref="PairingHeap{T1, T2}"/>. 
            /// Will be null if this <see cref="Node"/> is the root.</value>
            public Node Parent { get; set; }

            /// <value>The value of type <see cref="T2"/> determining the <see cref="Node"/>'s priority within the <see cref="PairingHeap{T1, T2}"/>.</value>
            public T2 Priority { get; set; }
        }
    }
}

