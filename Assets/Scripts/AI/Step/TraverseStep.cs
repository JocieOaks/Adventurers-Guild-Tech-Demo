public class TraverseStep : WalkStep
{
    ConnectionNode _connection;
    Room _oldRoom;
    Room _newRoom;

    public TraverseStep(RoomNode start, ConnectionNode connection, Pawn pawn, TaskStep step) : base(connection.GetRoomNode(start).WorldPosition, pawn, step)
    {
        _connection = connection;
        _oldRoom = start.Room;
        _newRoom = connection.GetConnectedRoom(_oldRoom);

    }

    protected override void Finish()
    {
        _oldRoom.ExitRoom(_pawn);
        _newRoom.EnterRoom(_pawn);
    }
}
