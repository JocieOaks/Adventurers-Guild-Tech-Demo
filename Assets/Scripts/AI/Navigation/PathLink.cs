using Assets.Scripts.AI.Actor;
using Assets.Scripts.Map.Node;

namespace Assets.Scripts.AI.Navigation
{
    /// <summary>
    /// The <see cref="PathLink"/> class form the links of a linked list that describes the path a <see cref="AI.Actor.Pawn"/> navigates through.
    /// </summary>
    public class PathLink
    {
        /// <value>The previous <see cref="PathLink"/> in the path.</value>
        public PathLink Previous { get; private set; }

        /// <value>The next <see cref="PathLink"/> in the path.</value>
        public PathLink Next { get; private set; }

        /// <value>The <see cref="AI.Actor.Pawn"/> navigating through the path.</value>
        public Pawn Pawn { get; }

        /// <value>The number of steps from the initial origin for the <see cref="AI.Actor.Pawn"/> to travel before reaching this <see cref="PathLink"/>.</value>
        public int Step { get; }

        /// <value>The <see cref="INode"/> being traversed during this step of the path.</value>
        public ConnectingNode Node { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PathLink"/> class.
        /// </summary>
        /// <param name="node">The <see cref="INode"/> being traversed.</param>
        /// <param name="previous">The previous <see cref="PathLink"/> in the path.</param>
        public PathLink(ConnectingNode node, PathLink previous)
        {
            Node = node;
            Pawn = previous.Pawn;
            Step = previous.Step + 1;
            Previous = previous;
            previous.Next = this;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PathLink"/> class that is the root of a new path.
        /// </summary>
        /// <param name="node">The <see cref="INode"/> in which the path is starting.</param>
        /// <param name="pawn">The <see cref="AI.Actor.Pawn"/> traversing the path.</param>
        public PathLink(ConnectingNode node, Pawn pawn)
        {
            Node = node;
            Pawn = pawn;
            Step = 0;
        }

    }
}
