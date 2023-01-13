﻿using UnityEngine;
/// <summary>
/// Base class for locations on a <see cref="Map"/>.
/// </summary>
public interface INode : IWorldPosition
{
    /// <summary>
    /// Determines if the <see cref="INode"/> can be passed through by a navigating <see cref="Pawn"/>.
    /// </summary>
    public bool Traversible{ get; set;}
}
