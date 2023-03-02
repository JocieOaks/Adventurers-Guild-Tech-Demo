using System.Collections;
using UnityEngine;
using System.Collections.Specialized;

/// <summary>
/// The kinds of speech for when a <see cref="Pawn"/> is conversing with another <see cref="Pawn"/>.
/// </summary>
public enum SpeechType
{
    Greet,
    Comment,
    Insult,
    Flirt,
    Goodbye
}

/// <summary>
/// The <see cref="SocialAI"/> class controls a <see cref="AdventurerPawn"/>'s social behavior. It runs parralel and independent of the <see cref="Planner"/>'s 
/// <see cref="Task"/> based behaviors to allow <see cref="AdventurerPawn"/>s to talk while perform other actions.
/// </summary>
public class SocialAI
{
    readonly AdventurerPawn _pawn;
    bool greetLockout = false;
    readonly ListDictionary lastTimeSpokenTo = new();

    /// <summary>
    /// Initializes a new instance of <see cref="SocialAI"/>.
    /// </summary>
    /// <param name="pawn">The <see cref="AdventurerPawn"/> controlled by <see cref="SocialAI"/>.</param>
    public SocialAI(AdventurerPawn pawn)
    {
        _pawn = pawn;
        GameManager.Ticked += OnTicked;
    }

    /// <value>The current <see cref="Conversation"/> the <see cref="AdventurerPawn"/> is engaged in, if any.</value>
    public Conversation Conversation { get; private set; } = null;

    /// <value>When set to true, disables a <see cref="AdventurerPawn"/>'s ability to speak. Most likely temporary. Used to stop <see cref="AdventurerPawn"/>s from entering a <see cref="Conversation"/>.</value>
    public bool Silenced { get; set; } = false;

    /// <summary>
    /// Sets the <see cref="AdventurerPawn"/> to leave a <see cref="Conversation"/> if it is in one.
    /// </summary>
    public void EndConversation()
    {
        Conversation?.Leave(_pawn);
        Conversation = null;
    }

    /// <summary>
    /// Called whenever the <see cref="AdventurerPawn"/> enters a new <see cref="Room"/>, to check if the should greet the current occupants of the room.
    /// </summary>
    /// <param name="room">The <see cref="Room"/> the <see cref="AdventurerPawn"/> has enetered.</param>
    public void EnterRoom(Room room)
    { 
        foreach (AdventurerPawn pawn in GetNearbyPawns())
        {
            if (CanGreet(pawn))
                Speak(pawn, SpeechType.Greet);
        }
    }

    /// <summary>
    /// Checks if the <see cref="AdventurerPawn"/> has last spoken to the given <see cref="AdventurerPawn"/> long enough ago to justify performing a greeting.
    /// </summary>
    /// <param name="pawn">The <see cref="AdventurerPawn"/> being checked.</param>
    /// <returns>Returns true if the <see cref="AdventurerPawn"/> can be greeted.</returns>
    bool CanGreet(AdventurerPawn pawn)
    {
        return CanSpeakTo(pawn) && !greetLockout && (lastTimeSpokenTo[pawn] == null || GameManager.Instance.Tick - (int)lastTimeSpokenTo[pawn] > 150);
    }

    /// <summary>
    /// Checks if the <see cref="AdventurerPawn"/> can speak based on whether they are currently speaking or if their <see cref="TaskAction"/> allows speaking.
    /// </summary>
    /// <returns>Returns true if the <see cref="AdventurerPawn"/> can speak.</returns>
    bool CanSpeak()
    {
        return !_pawn.IsSpeaking && _pawn.CurrentAction.CanSpeak; //&& !Silenced;
    }

    /// <summary>
    /// Checks if the <see cref="AdventurerPawn"/> can speak to a given <see cref="AdventurerPawn"/>.
    /// </summary>
    /// <param name="pawn">The <see cref="AdventurerPawn"/> to be spoken to.</param>
    /// <returns>True if the <see cref="AdventurerPawn"/> can be spoken to.</returns>
    bool CanSpeakTo(AdventurerPawn pawn)
    {
        return CanSpeak() && pawn.CurrentAction.CanListen && pawn != _pawn;
    }

    /// <summary>
    /// Finds nearby <see cref="AdventurerPawn"/> within speaking range of the <see cref="AdventurerPawn"/>.
    /// </summary>
    /// <param name="radius">The maximum distance a <see cref="AdventurerPawn"/> can be from the <see cref="AdventurerPawn"/>.</param>
    /// <returns>The <see cref="AdventurerPawn"/>'s near to the <see cref="AdventurerPawn"/>.</returns>
    IEnumerable GetNearbyPawns(int radius = 10)
    {
        if (_pawn.Room is Layer)
        {
            Vector3Int position = _pawn.WorldPosition;

            foreach (Pawn pawn in _pawn.Room.Occupants)
            {
                Vector3Int relPosition = position - pawn.WorldPosition;
                if (pawn != _pawn &&
                    pawn is AdventurerPawn &&
                    Mathf.Abs(relPosition.x) < radius &&
                    Mathf.Abs(relPosition.y) < radius)
                    yield return pawn;
            }
        }
        else
        {

            foreach (Pawn pawn in _pawn.Room.Occupants)
                if (pawn != _pawn && pawn is AdventurerPawn)
                    yield return pawn;
        }
    }

    /// <summary>
    /// Called when another <see cref="AdventurerPawn"/> speaks within hearing range of the <see cref="AdventurerPawn"/>.
    /// </summary>
    /// <param name="pawn">The <see cref="AdventurerPawn"/> speaking.</param>
    /// <param name="type">The <see cref="SpeechType"/> of the action.</param>
    void Hear(AdventurerPawn pawn, SpeechType type)
    {
        int random;
        switch (type)
        {
            case SpeechType.Greet:
                if (CanGreet(pawn))
                    _pawn.StartCoroutine(Respond(pawn, SpeechType.Greet, Random.Range(0.25f, 1f)));
                else
                {
                    random = Random.Range(0, 6);
                    if (random == 0 && Conversation == null && !pawn.IsInConversation && CanSpeak())
                    {
                        StartConversation(pawn);
                    }
                    else if (random < 3)
                        _pawn.StartCoroutine(Respond(pawn, SpeechType.Comment, Random.Range(0.75f, 2f)));
                }
                break;
            case SpeechType.Comment:
                random = Random.Range(0, 6);
                if (random == 0 && Conversation == null && !pawn.IsInConversation && CanSpeak())
                {
                    StartConversation(pawn);
                }
                else if (random < 3)
                    _pawn.StartCoroutine(Respond(pawn, SpeechType.Comment, Random.Range(0.75f, 2f)));
                break;
        }

        lastTimeSpokenTo[pawn] = GameManager.Instance.Tick;
    }

    /// <summary>
    /// Called each <see cref="GameManager"/> tick. Each tick, the <see cref="AdventurerPawn"/> has a random chance of speaking.
    /// </summary>
    void OnTicked()
    {
        if (_pawn.Room is Layer && CanSpeak())
        {
            foreach (AdventurerPawn pawn in GetNearbyPawns())
            {
                if (CanGreet(pawn))
                    Speak(pawn, SpeechType.Greet);
            }
        }

        if (Conversation == null)
        {
            int random = Random.Range(0, 15);
            if (random == 0 && CanSpeak())
            {

                foreach (AdventurerPawn pawn in GetNearbyPawns(8))
                {
                    if (CanSpeakTo(pawn) && Vector3.Dot(Utility.DirectionToVector(_pawn.Direction), pawn.WorldPosition - _pawn.WorldPosition) > 0)
                        Speak(pawn, SpeechType.Comment);
                }
            }
        }
        else
        {
            int random = Random.Range(0, 10);
            if(random == 0 && CanSpeak())
            {
                foreach(AdventurerPawn pawn in Conversation.Pawns)
                {
                    if (CanSpeakTo(pawn))
                        Speak(pawn, SpeechType.Comment);
                }
            }
        }
    }

    /// <summary>
    /// Called in response to hearing another <see cref="AdventurerPawn"/> speak.
    /// </summary>
    /// <param name="pawn">The <see cref="AdventurerPawn"/> being responded to.</param>
    /// <param name="type">The <see cref="SpeechType"/> of the message being responded to.</param>
    /// <param name="delay">The amount of time to wait before responding, in seconds.</param>
    /// <returns>Returns <see cref="WaitForSeconds"/> objects for the <c>StartCoroutine</c> function, until the delay has expired is ready.</returns>
    IEnumerator Respond(AdventurerPawn pawn, SpeechType type, float delay)
    {
        if (type == SpeechType.Greet)
            greetLockout = true;
        yield return new WaitForSeconds(delay);
        if (CanSpeak())
        {
            if (_pawn.Room.IsInRoom(pawn))
            {
                switch (type)
                {
                    case SpeechType.Greet:
                        Speak(pawn, type);
                        greetLockout = false;
                        break;
                    case SpeechType.Comment:
                        Speak(pawn, type);
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Say something to another <see cref="AdventurerPawn"/>.
    /// </summary>
    /// <param name="pawn">The <see cref="AdventurerPawn"/> being spoken to.</param>
    /// <param name="type">The <see cref="SpeechType"/> of what is being said.</param>
    void Speak(AdventurerPawn pawn, SpeechType type)
    {
        pawn.Social.Hear(_pawn, type);
        lastTimeSpokenTo[pawn] = GameManager.Instance.Tick;
        _pawn.StartCoroutine(_pawn.Say(type));
    }

    /// <summary>
    /// Initialize a new <see cref="Conversation"/> with another <see cref="AdventurerPawn"/>.
    /// </summary>
    /// <param name="pawn">The <see cref="AdventurerPawn"/> with which the conversation is being started.</param>
    void StartConversation(AdventurerPawn pawn)
    {
        Debug.Log("Starting Conversation Between " + pawn.Actor.Stats.Name + " and " + _pawn.Actor.Stats.Name);
        Conversation = new Conversation(_pawn, pawn);
        pawn.Social.Conversation = Conversation;

        _pawn.OverrideTask(new ApproachTask());
        pawn.OverrideTask(new ApproachTask());
    }
}
