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
        BuildFunctions.CreatePoint = BarSprite.CreateBar;
        BuildFunctions.CheckPoint = BarSprite.CheckObject;
        BuildFunctions.HighlightPoint = BarSprite.PlaceHighlight;
    }

    /// <summary>
    /// Called when the bed object is selected from the objects build menu.
    /// </summary>
    public void BedOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Point;
        BuildFunctions.CreatePoint = BedSprite.CreateBed;
        BuildFunctions.CheckPoint = BedSprite.CheckObject;
        BuildFunctions.HighlightPoint = BedSprite.PlaceHighlight;
    }

    /// <summary>
    /// Called when the chair object is selected from the objects build menu.
    /// </summary>
    public void ChairOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Point;
        BuildFunctions.Direction = Direction.West;
        BuildFunctions.CreatePoint = ChairSprite.CreateChair;
        BuildFunctions.CheckPoint = ChairSprite.CheckObject;
        BuildFunctions.HighlightPoint = ChairSprite.PlaceHighlight;
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
        BuildFunctions.CreatePoint = FloorSprite.CreateFloor;
        BuildFunctions.HighlightPoint = FloorSprite.PlaceHighlight;
        BuildFunctions.CheckPoint = FloorSprite.CheckObject;
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
        BuildFunctions.CreatePoint = StairSprite.CreateStair;
        BuildFunctions.CheckPoint = StairSprite.CheckObject;
        BuildFunctions.HighlightPoint = StairSprite.PlaceHighlight;
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
        BuildFunctions.CreatePoint = StoolSprite.CreateStool;
        BuildFunctions.CheckPoint = StoolSprite.CheckObject;
        BuildFunctions.HighlightPoint = StoolSprite.PlaceHighlight;
    }

    /// <summary>
    /// Called when the round table object is selected from the objects build menu.
    /// </summary>
    public void TableRoundOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Point;
        BuildFunctions.CreatePoint = TableRoundSprite.CreateTableRound;
        BuildFunctions.CheckPoint = TableRoundSprite.CheckObject;
        BuildFunctions.HighlightPoint = TableRoundSprite.PlaceHighlight;
    }

    /// <summary>
    /// Called when the square table object is selected from the objects build menu.
    /// </summary>
    public void TableSquareOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Point;
        BuildFunctions.CreatePoint = TableSquareSprite.CreateTableSquare;
        BuildFunctions.CheckPoint = TableSquareSprite.CheckObject;
        BuildFunctions.HighlightPoint = TableSquareSprite.PlaceHighlight;
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
        BuildFunctions.CreatePoint = WallSprite.CreateWall;
        BuildFunctions.CheckPoint = WallSprite.CheckObject;
        BuildFunctions.HighlightPoint = WallSprite.PlaceHighlight;
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
