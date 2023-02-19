/// <summary>
/// The <see cref="TraverseStep"/> class inherits from <see cref="WalkStep"/> for passing through a <see cref="ConnectingNode"/>.
/// </summary>
public class TraverseStep : WalkStep
{
    ConnectingNode _connection;
    Room _newRoom;
    Room _oldRoom;

    /// <summary>
    /// Initializes a new instance of the <see cref="TraverseStep"/> class.
    /// </summary>
    /// <param name="start">The starting <see cref="RoomNode"/> of the <see cref="Pawn"/>.</param>
    /// <param name="connection">The <see cref="ConnectingNode"/> for the <see cref="Pawn"/> to traverse.</param>
    /// <param name="pawn">The <see cref="Pawn"/> performing the step.</param>
    /// <param name="step">The previous <see cref="TaskStep"/> the <see cref="Pawn"/> was performing.</param>
    public TraverseStep(RoomNode start, ConnectingNode connection, Pawn pawn, TaskStep step) : base(connection.GetRoomNode(start).SurfacePosition, pawn, step)
    {
        _connection = connection;
        _oldRoom = start.Room;
        _newRoom = connection.GetConnectedRoom(_oldRoom);

    }

    /// <inheritdoc/>
    protected override void Finish()
    {
        _oldRoom.ExitRoom(_pawn);
        _newRoom.EnterRoom(_pawn);
    }
}
