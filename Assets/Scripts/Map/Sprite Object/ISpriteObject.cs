using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The <see cref="ISpriteObject"/> interface is the interface that corrseponds to the <see cref="SpriteObject"/> base class. 
/// It is used for other interfaces that extend <see cref="SpriteObject"/> such as <see cref="IInteractable"/>.
/// </summary>
public interface ISpriteObject : IDataPersistence, IWorldPosition
{
    /// <value>he 3D dimensions of the <see cref="SpriteObject"/> in terms of <see cref="Map"/> coordinates. 
    /// Normally should be equivalent to <see cref="ObjectDimensions"/> but can be publicly accessed without knowing the <see cref="SpriteObject"/>'s type.</value>
    Vector3Int Dimensions { get; }

    /// <value>The pixels blocked by the <see cref="SpriteObject"/>'s sprites as an array of bools. Used by <see cref="Pawn"/> to construct a <see cref="SpriteMask"/> for all objects in front of it.</value>
    IEnumerable<bool[,]> GetMaskPixels { get; }

    /// <value>Gives the offset for <see cref="GetMaskPixels"/> for <see cref="Pawn"/> when constructing a <see cref="SpriteMask"/> composed of multiple copies of the same <see cref="UnityEngine.Sprite"/>.</value>
    Vector3 OffsetVector { get; }

    /// <value>Gives the <see cref="UnityEngine.SpriteRenderer"/> for the forward most sprite of the <see cref="SpriteObject"/>.</value>
    SpriteRenderer SpriteRenderer { get; }

    /// <summary>
    /// Destroy's the <see cref="SpriteObject"/> and all of it's <see cref="UnityEngine.SpriteRenderer"/>s.
    /// </summary>
    void Destroy();

    /// <summary>
    /// Sets the <see cref="SpriteObject"/> to a specific highlight color.
    /// </summary>
    /// <param name="color">The color to set the <see cref="UnityEngine.SpriteRenderer"/>s.</param>
    void Highlight(Color color);
}