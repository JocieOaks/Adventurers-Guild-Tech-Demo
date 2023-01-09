public interface IOccupied : IInteractable
{
    public Actor Occupant { get; set; }
    public bool Occupied => Occupant != null;
    public void Enter(Pawn pawn);

    public void Exit(Pawn pawn);
}
