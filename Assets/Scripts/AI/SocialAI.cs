using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Specialized;
using System.Linq;

public enum SpeechType
{
    Greet,
    Comment,
    Insult,
    Flirt,
    Goodbye
}


public class SocialAI
{
    Pawn _pawn;
    ListDictionary lastTimeSpokenTo = new ListDictionary();
    public bool Silenced { get; set; } = false;

    public Conversation? Conversation { get; private set; } = null;


    public SocialAI(Pawn pawn)
    {
        _pawn = pawn;
        GameManager.Ticked += OnTicked;
    }

    public void EnterRoom(Room room)
    {
        foreach(Pawn pawn in GetNearbyPawns())
        {
            if (CanGreet(pawn))
                Speak(pawn, SpeechType.Greet);
        }
    }

    bool CanSpeak()
    {
        return !_pawn.IsSpeaking && _pawn.CurrentAction.CanSpeak && !Silenced;
    }

    bool CanSpeakTo(Pawn pawn)
    {
        return CanSpeak() && pawn.CurrentAction.CanListen && pawn != _pawn;
    }

    bool CanGreet(Pawn pawn)
    {
        return CanSpeakTo(pawn) && !greetLockout && (lastTimeSpokenTo[pawn] == null || GameManager.Instance.Tick - (int)lastTimeSpokenTo[pawn] > 150);
    }

    IEnumerable GetNearbyPawns(int radius = 10)
    {
        if (_pawn.CurrentRoom is Layer)
        {
            Vector3Int position = _pawn.WorldPosition;

            foreach (Pawn pawn in _pawn.CurrentRoom.Occupants)
            {
                Vector3Int relPosition = position - pawn.WorldPosition;
                if (pawn != _pawn &&
                    Mathf.Abs(relPosition.x) < radius &&
                    Mathf.Abs(relPosition.y) < radius)
                    yield return pawn;
            }
        }
        else
        {
            foreach(Pawn pawn in _pawn.CurrentRoom.Occupants)
                if(pawn != _pawn)
                    yield return pawn;
        }
    }

    void OnTicked()
    {
        if(_pawn.CurrentRoom is Layer && CanSpeak())
        {
            foreach (Pawn pawn in GetNearbyPawns())
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
                foreach (Pawn pawn in GetNearbyPawns(8))
                {
                    if (CanSpeakTo(pawn) && Vector3.Dot(Map.DirToVector(_pawn.Direction), pawn.WorldPosition - _pawn.WorldPosition) > 0)
                        Speak(pawn, SpeechType.Comment);
                }
            }
        }
        else
        {
            int random = Random.Range(0, 10);
            if(random == 0 && CanSpeak())
            {
                foreach(Pawn pawn in Conversation.Pawns)
                {
                    if (CanSpeakTo(pawn))
                        Speak(pawn, SpeechType.Comment);
                }
            }
        }
    }

    void StartConversation(Pawn pawn)
    {
        Debug.Log("Starting Conversation Between " + pawn.Actor.Stats.Name + " and " + _pawn.Actor.Stats.Name);
        Conversation = new Conversation(_pawn, pawn);
        pawn.Social.Conversation = Conversation;

        _pawn.OverrideTask(new ApproachTask());
        pawn.OverrideTask(new ApproachTask());
    }

    public void EndConversation()
    {
        Conversation?.Leave(_pawn);
        Conversation = null;
    }

    void Speak(Pawn pawn, SpeechType type)
    {
        pawn.Social.Hear(_pawn, type);
        lastTimeSpokenTo[pawn] = GameManager.Instance.Tick;
        _pawn.StartCoroutine(_pawn.Say(type));
    }

    void Hear(Pawn pawn, SpeechType type)
    {
        int random;
        switch (type)
        {
            case SpeechType.Greet:
                if(CanGreet(pawn))
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

    bool greetLockout = false;

    IEnumerator Respond(Pawn pawn, SpeechType type, float delay)
    {
        if (type == SpeechType.Greet)
            greetLockout = true;
        yield return new WaitForSeconds(delay);
        if (CanSpeak())
        {
            if(_pawn.CurrentRoom.IsInRoom(pawn))
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
}
