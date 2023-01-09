
public delegate Task GetPayoffDelegate(WorldState worldState);
public interface ISetup
{
    public GetPayoffDelegate Payoff => ConstructPayoff;

    public Task ConstructPayoff(WorldState worldState);
}
