public interface IOccupied : IInteractable
{
    /// <value>The <see cref="Pawn"/> occuping the <see cref="IOccupied"/>. Null if empty.</value>
    public Pawn Occupant { get; set; }
    /// <value>Returns true if the <see cref="IOccupied"/> has an <see cref="Pawn"/> occupying it.</value>
    public bool Occupied => Occupant != null;

    /// <summary>
    /// Called when a given <see cref="Pawn"/> begins occupying the <see cref="IOccupied"/>.
    /// </summary>
    /// <param name="pawn"><see cref="Pawn"/> occupying the <see cref="IOccupied"/>.</param>
    public void Enter(Pawn pawn);

    /// <summary>
    /// Called when a given <see cref="Pawn"/> stops occupying the <see cref="IOccupied"/>. Checks to ensure
    /// that the given <see cref="Pawn"/> is actually occupying the <see cref="IOccupied"/>.
    /// </summary>
    /// <param name="pawn"><see cref="Pawn"/> exiting the <see cref="IOccupied"/>.</param>
    public void Exit(Pawn pawn);
}
