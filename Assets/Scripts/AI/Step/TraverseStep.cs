/// <summary>
/// The <see cref="TraverseStep"/> class inherits from <see cref="WalkStep"/> for passing through a <see cref="ConnectingNode"/>.
/// Currently doesn't really do anything, but will become useful once doors become properly interactable.
/// </summary>
public class TraverseStep : WalkStep
{
    readonly ConnectingNode _connection;
    readonly Room _newRoom;
    readonly Room _oldRoom;

    /// <summary>
    /// Initializes a new instance of the <see cref="TraverseStep"/> class.
    /// </summary>
    /// <param name="start">The starting <see cref="RoomNode"/> of the <see cref="Pawn"/>.</param>
    /// <param name="connection">The <see cref="ConnectingNode"/> for the <see cref="Pawn"/> to traverse.</param>
    /// <param name="pawn">The <see cref="Pawn"/> performing the step.</param>
    /// <param name="step">The previous <see cref="TaskStep"/> the <see cref="Pawn"/> was performing.</param>
    public TraverseStep(RoomNode start, ConnectingNode connection, Pawn pawn, TaskStep step) : base(connection.GetOppositeRoomNode(start).SurfacePosition, pawn, step) {}
}
