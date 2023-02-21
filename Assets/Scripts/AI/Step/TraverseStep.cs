/// <summary>
/// The <see cref="TraverseStep"/> class inherits from <see cref="WalkStep"/> for passing through a <see cref="ConnectingNode"/>.
/// </summary>
public class TraverseStep : WalkStep
{
    readonly ConnectingNode _connection;
    readonly Room _newRoom;
    readonly Room _oldRoom;

    /// <summary>
    /// Initializes a new instance of the <see cref="TraverseStep"/> class.
    /// </summary>
    /// <param name="start">The starting <see cref="RoomNode"/> of the <see cref="AdventurerPawn"/>.</param>
    /// <param name="connection">The <see cref="ConnectingNode"/> for the <see cref="AdventurerPawn"/> to traverse.</param>
    /// <param name="pawn">The <see cref="AdventurerPawn"/> performing the step.</param>
    /// <param name="step">The previous <see cref="TaskStep"/> the <see cref="AdventurerPawn"/> was performing.</param>
    public TraverseStep(RoomNode start, ConnectingNode connection, AdventurerPawn pawn, TaskStep step) : base(connection.GetOppositeRoomNode(start).SurfacePosition, pawn, step)
    {
        _connection = connection;
        _oldRoom = start.Room;
        _newRoom = connection.GetConnectedRoom(_oldRoom);

    }

    /// <inheritdoc/>
    protected override void Finish()
    {
        _oldRoom.ExitRoom(_pawn as AdventurerPawn);
        _newRoom.EnterRoom(_pawn as AdventurerPawn);
    }
}
