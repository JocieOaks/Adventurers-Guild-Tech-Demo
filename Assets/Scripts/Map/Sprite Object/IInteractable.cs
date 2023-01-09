using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    public Vector3Int WorldPosition { get; }

    public List<RoomNode> GetInteractionPoints();
}
