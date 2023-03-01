using UnityEngine;
using System;

/// <summary>
/// Demarks the type of <see cref="SpriteObject"/>s being built, so that <see cref="GameManager"/> now how to handle <see cref="BuildFunctions"/>.
/// </summary>
public enum BuildMode
{
    /// <summary>Nothing is currently being built.</summary>
    None,
    /// <summary>The <see cref="SpriteObject"/> being built consists of a single object.</summary>
    Point,
    /// <summary>The <see cref="SpriteObject"/> being built is a <see cref="LinearSpriteObject"/> in which multiple objects appear in a line.</summary>
    Line,
    /// <summary>The <see cref="SpriteObject"/> being built is a <see cref="AreaSpriteObject"/> in which multiple objects appear across a broad area.</summary>
    Area,
    /// <summary>Special <see cref="BuildMode"/> for placing a door into a <see cref="WallSprite"/>.</summary>
    Door,
    /// <summary><see cref="SpriteObject"/>s are to be destroyed instead of created.</summary>
    Demolish
}

/// <summary>
/// The <see cref="BuildFunctions"/> class is a static class containing references to methods used in building.
/// To change what is being built, the <see cref="CheckSpriteObject"/>, <see cref="CreateSpriteObject"/>, and <see cref="HighlightSpriteObject"/> are set to reference
/// the corresponding static methods of the <see cref="SpriteObject"/> being created.
/// </summary>
public static class BuildFunctions
{
    /// <value>Gives the type of object being built, so that <see cref="GameManager"/> knows how to use the player input.</value>
    public static BuildMode BuildMode { get; set; }

    /// <value>Gives reference to the corresponding <see cref="SpriteObject"/> method to determine 
    /// if the given <see cref="Map"/> coordinates are a valid position for the <see cref="SpriteObject"/>.</value>
    public static Func<Vector3Int,bool> CheckSpriteObject { get; set; }

    /// <value>Gives reference to the corresponding <see cref="SpriteObject"/> method to create a new instance of the <see cref="SpriteObject"/>.</value>
    public static Action<Vector3Int> CreateSpriteObject { get; set; }

    /// <value>The <see cref="global::Direction"/> any newly built <see cref="SpriteObject"/>s will face.</value>
    public static Direction Direction { get; set; }

    /// <value>Gives reference to the corresponding <see cref="SpriteObject"/> method to place a highlight of the <see cref="SpriteObject"/> to be built.</value>
    public static Action<SpriteRenderer, Vector3Int> HighlightSpriteObject { get; set; }
}
