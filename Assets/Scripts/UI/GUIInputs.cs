using UnityEngine;

/// <summary>
/// The <see cref="GUIInputs"/> class is a singleton that exposes certain functions to be called whenever a UI button is clicked, then calls the corresponding functions.
/// </summary>
public class GUIInputs : MonoBehaviour
{
    static GUI _gui;
    static GUIInputs _instance;

    /// <value>A quick reference to the <see cref="GUI"/> singleton.</value>
    static GUI GUI
    {
        get
        {
            if (_gui == null)
                _gui = GUI.Instance;
            return _gui;
        }
    }

    /// <summary>
    /// Called when the adventurer select panel is closed.
    /// </summary>
    public void AdventurerSelectCloseOnClick()
    {
        GUI.CloseAdventurerSelect();
    }

    /// <summary>
    /// Called when the bar object is selected from the objects build menu.
    /// </summary>
    public void BarOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Line;
        BuildFunctions.CreateSpriteObject = BarSprite.CreateBar;
        BuildFunctions.CheckSpriteObject = BarSprite.CheckObject;
        BuildFunctions.HighlightSpriteObject = BarSprite.PlaceHighlight;
    }

    /// <summary>
    /// Called when the bed object is selected from the objects build menu.
    /// </summary>
    public void BedOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Point;
        BuildFunctions.CreateSpriteObject = BedSprite.CreateBed;
        BuildFunctions.CheckSpriteObject = BedSprite.CheckObject;
        BuildFunctions.HighlightSpriteObject = BedSprite.PlaceHighlight;
    }

    /// <summary>
    /// Called when the chair object is selected from the objects build menu.
    /// </summary>
    public void ChairOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Point;
        BuildFunctions.Direction = Direction.West;
        BuildFunctions.CreateSpriteObject = ChairSprite.CreateChair;
        BuildFunctions.CheckSpriteObject = ChairSprite.CheckObject;
        BuildFunctions.HighlightSpriteObject = ChairSprite.PlaceHighlight;
    }

    /// <summary>
    /// Called when demolish is selected from the build menu.
    /// </summary>
    public void DemolishOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Demolish;
        GUI.Instance.CloseObjects();
    }

    /// <summary>
    /// Called when door is selected from the build menu.
    /// </summary>
    public void DoorOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Door;
        GUI.Instance.CloseObjects();
    }

    /// <summary>
    /// Called when the down button is clicked.
    /// </summary>
    public void DownOnClick()
    {
        GameManager.Instance.ChangeLevel(false);
    }

    /// <summary>
    /// Called when floor is selected from the build menu.
    /// </summary>
    public void FloorOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Area;
        BuildFunctions.CreateSpriteObject = FloorSprite.CreateFloor;
        BuildFunctions.HighlightSpriteObject = FloorSprite.PlaceHighlight;
        BuildFunctions.CheckSpriteObject = FloorSprite.CheckObject;
        GUI.Instance.CloseObjects();
    }

    /// <summary>
    /// Called when the adventurers button is clicked.
    /// </summary>
    public void HiresOnClick()
    {
        GUI.OpenCloseHires();
    }

    /// <summary>
    /// Called when object is selected from the build menu.
    /// </summary>
    public void ObjectOnClick()
    {
        GUI.OpenCloseObjects();
    }

    /// <summary>
    /// Called when quests is selected from the play menu.
    /// </summary>
    public void QuestsOnClick()
    {
        GUI.OpenCloseQuests();
    }

    /// <summary>
    /// Called when the save button is clicked.
    /// </summary>
    public void SaveOnClick()
    {
        DataPersistenceManager.Instance.SaveGame();
    }

    /// <summary>
    /// Called when a quest is selected in the quest menu.
    /// </summary>
    /// <param name="id">The id of the chosen quest.</param>
    public void SelectQuestOnClick(int id)
    {
        GUI.SelectQuest(id);
    }

    /// <summary>
    /// Called when stair is selected from the build menu.
    /// </summary>
    public void StairOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Area;
        BuildFunctions.CreateSpriteObject = StairSprite.CreateStair;
        BuildFunctions.CheckSpriteObject = StairSprite.CheckObject;
        BuildFunctions.HighlightSpriteObject = StairSprite.PlaceHighlight;
        GUI.Instance.CloseObjects();
    }

    /// <summary>
    /// Called when start is clicked in the quest menu.
    /// </summary>
    public void StartQuestOnClick()
    {
        GUI.StartQuest();
    }

    /// <summary>
    /// Called when the stool object is selected from the objects build menu.
    /// </summary>
    public void StoolOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Point;
        BuildFunctions.CreateSpriteObject = StoolSprite.CreateStool;
        BuildFunctions.CheckSpriteObject = StoolSprite.CheckObject;
        BuildFunctions.HighlightSpriteObject = StoolSprite.PlaceHighlight;
    }

    /// <summary>
    /// Called when the round table object is selected from the objects build menu.
    /// </summary>
    public void TableRoundOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Point;
        BuildFunctions.CreateSpriteObject = TableRoundSprite.CreateTableRound;
        BuildFunctions.CheckSpriteObject = TableRoundSprite.CheckObject;
        BuildFunctions.HighlightSpriteObject = TableRoundSprite.PlaceHighlight;
    }

    /// <summary>
    /// Called when the square table object is selected from the objects build menu.
    /// </summary>
    public void TableSquareOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Point;
        BuildFunctions.CreateSpriteObject = TableSquareSprite.CreateTableSquare;
        BuildFunctions.CheckSpriteObject = TableSquareSprite.CheckObject;
        BuildFunctions.HighlightSpriteObject = TableSquareSprite.PlaceHighlight;
    }

    /// <summary>
    /// Called when the up button is clicked.
    /// </summary>
    public void UpOnClick()
    {
        GameManager.Instance.ChangeLevel(true);
    }

    /// <summary>
    /// Called when wall is selected from the build menu.
    /// </summary>
    public void WallOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Line;
        BuildFunctions.CreateSpriteObject = WallSprite.CreateWall;
        BuildFunctions.CheckSpriteObject = WallSprite.CheckObject;
        BuildFunctions.HighlightSpriteObject = WallSprite.PlaceHighlight;
        GUI.Instance.CloseObjects();
    }

    /// <inheritdoc/>
    void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(this);
    }
}
