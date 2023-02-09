/// <summary>
/// The <see cref="IDirected"/> interface is for objects that face a specific cardinal direction.
/// </summary>
public interface IDirected
{
    /// <value>The <see cref="global::Direction"/> being faced.</value>
    Direction Direction { get; }
}