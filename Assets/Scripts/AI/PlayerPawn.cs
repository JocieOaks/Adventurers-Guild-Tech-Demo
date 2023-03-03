using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerPawn : Pawn
{
    static readonly List<int> s_humanSkinTones = new() { 9, 10, 11, 12, 13, 14, 15, 16, 20 };
    static readonly List<int> s_naturalHairColors = new() { 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };
    static readonly List<int> s_orcSkinTones = new() { 4, 5, 6, 7, 8, 16 };
    static readonly List<int> s_tieflingSkinTones = new() { 0, 1, 2, 3, 17, 18, 19, 20 };
    static readonly List<int> s_unnaturalHairColors = new() { 0, 1, 2 };
    float _speed = 2.5f;

    /// <inheritdoc/>
    public override RoomNode CurrentNode
    {
        get => _currentNode;
        protected set
        {
            _currentNode = value;
            int level = GameManager.Instance.IsOnLevel(CurrentLevel);
            if (level < 0)
                GameManager.Instance.ChangeLevel(false);
            else if (level > 0)
                GameManager.Instance.ChangeLevel(true);
        }
    }

    /// <inheritdoc/>
    public override float Speed => _speed * CurrentNode.SpeedMultiplier;

    /// <inheritdoc/>
    public override Vector3 WorldPositionNonDiscrete 
    { 
        get => base.WorldPositionNonDiscrete; 
        set {

            Vector3Int vector = Utility.DirectionToVector(Direction);

            if (vector.x != 0 && !CurrentNode.GetNode(vector.x > 0 ? Direction.East : Direction.West).Traversable)
            {
                if ((value.x - WorldPosition.x) * vector.x > 0.25f)
                {
                    value = new Vector3(WorldPosition.x + 0.25f * vector.x, value.y, value.z);
                }
            }
            if (vector.y != 0 && !CurrentNode.GetNode(vector.y > 0 ? Direction.North : Direction.South).Traversable)
            {
                if ((value.y - WorldPosition.y) * vector.y > 0.25f)
                {
                    value = new Vector3(value.x, WorldPosition.y + 0.25f * vector.y, value.z);
                }
            }
            
            base.WorldPositionNonDiscrete = value;
        } 
    }

    /// <inheritdoc/>
    public override string Name => "Player";

    /// <inheritdoc/>
    protected override IEnumerator Startup()
    {
        yield return new WaitUntil(() => GameManager.GameReady);

        CurrentNode = Map.Instance[Utility.SceneCoordinatesToMapCoordinates(transform.position, 0)];

        WorldPositionNonDiscrete = WorldPosition;

        InitializeGameObject();

        Graphics.LevelChanged += OnLevelChange;
        Graphics.UpdatedGraphics += BuildSpriteMask;
        Graphics.LevelChangedLate += BuildSpriteMask;

        

        NativeArray<Color> pixels;

        Race Race = (Race)Random.Range(0, 4);

        ActorAppearance appearance = new(Race);

        JobHandle jobHandle = Graphics.Instance.BuildSprites(appearance, out pixels);

        yield return new WaitUntil(() => jobHandle.IsCompleted);
        jobHandle.Complete();
        _animationSprites = Graphics.Instance.SliceSprites(pixels.ToArray());
        pixels.Dispose();

        CurrentStep = new WaitStep(this, Direction.SouthEast, false);

        InitializeGameObject();

        _ready = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_ready && !GameManager.Instance.Paused && GameManager.Instance.GameMode == GameMode.Play)
        {
            Vector3 movement = Vector3.zero;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                movement += new Vector3Int(1, 1);
            }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                movement += new Vector3Int(-1, -1);
            }
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                movement += new Vector3Int(-1, 1);
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                movement += new Vector3Int(1, -1);
            }

            if(Input.GetKey(KeyCode.LeftShift))
            {
                _speed = 4f;
            }
            else
            {
                _speed = 2.5f;
            }

            Direction direction = movement switch
            {
                Vector3 v when v == new Vector3Int(1, 1) => Direction.NorthEast,
                Vector3 v when v == new Vector3Int(2, 0) => Direction.East,
                Vector3 v when v == new Vector3Int(1, -1) => Direction.SouthEast,
                Vector3 v when v == new Vector3Int(0, -2) => Direction.South,
                Vector3 v when v == new Vector3Int(-1, -1) => Direction.SouthWest,
                Vector3 v when v == new Vector3Int(-2, 0) => Direction.West,
                Vector3 v when v == new Vector3Int(-1, 1) => Direction.NorthWest,
                Vector3 v when v == new Vector3Int(0, 2) => Direction.North,
                _ => Direction.Undirected,
            };

            if(direction == Direction.Undirected)
            {
                if(CurrentStep is not WaitStep)
                {
                    CurrentStep = new WaitStep(this, CurrentStep, false);
                }
            }
            else
            {
                if(CurrentStep is not WalkStep step || step.Direction != direction)
                {
                    CurrentStep = new WalkStep(direction, this, CurrentStep);
                }
            }

            CurrentStep.Perform();
        }
    }

}
