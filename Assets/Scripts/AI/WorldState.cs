using UnityEngine;

public struct WorldState
{
    public Conversation Conversation;
    public Task PreviousTask;
    public ActorProfile PrimaryActor;
    float? _distance;
    public WorldState(Actor actor)
    {
        PrimaryActor = actor.Stats;
        Conversation = actor.Pawn.Social.Conversation;
        _distance = null;
        PreviousTask = null;
    }

    public float ConversationDistance
    {
        set
        {
            _distance = value;
        }
        get
        {
            if (_distance == null)
            {
                if (Conversation != null)
                    _distance = Vector3.Distance(Conversation.Nexus, PrimaryActor.Position);
                else
                    return 0;
            }
            return _distance.Value;
        }
    }
}
//Timescale: 1 FrameTick == 10 seconds.
//Time value is measured in frame ticks.
