using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIInputs : MonoBehaviour
{
    static GUIInputs Instance;

    static GUI _gui;

    static GUI GUI
    {
        get
        {
            if (_gui == null)
                _gui = GUI.Instance;
            return _gui;
        }
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    public void WallOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Line;
        BuildFunctions.CreatePoint = WallSprite.CreateWall;
        BuildFunctions.CheckPoint = WallSprite.CheckObject;
        BuildFunctions.HighlightPoint = WallSprite.PlaceHighlight;
        GUI.Instance.CloseObjects();
    }

    public void DoorOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Door;
        GUI.Instance.CloseObjects();
    }

    public void StairOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Area;
        BuildFunctions.CreatePoint = StairSprite.CreateStair;
        BuildFunctions.CheckPoint = StairSprite.CheckObject;
        BuildFunctions.HighlightPoint = StairSprite.PlaceHighlight;
        GUI.Instance.CloseObjects();
    }

    public void FloorOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Area;
        BuildFunctions.CreatePoint = FloorSprite.CreateFloor;
        BuildFunctions.HighlightPoint = FloorSprite.PlaceHighlight;
        BuildFunctions.CheckPoint = FloorSprite.CheckObject;
        GUI.Instance.CloseObjects();
    }

    public void ObjectOnClick()
    {
        GUI.OpenCloseObjects();
    }

    public void BedOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Point;
        BuildFunctions.CreatePoint = BedSprite.CreateBed;
        BuildFunctions.CheckPoint = BedSprite.CheckObject;
        BuildFunctions.HighlightPoint = BedSprite.PlaceHighlight;
    }

    public void ChairOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Point;
        BuildFunctions.Direction = Direction.West;
        BuildFunctions.CreatePoint = ChairSprite.CreateChair;
        BuildFunctions.CheckPoint = ChairSprite.CheckObject;
        BuildFunctions.HighlightPoint = ChairSprite.PlaceHighlight;
    }

    public void StoolOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Point;
        BuildFunctions.CreatePoint = StoolSprite.CreateStool;
        BuildFunctions.CheckPoint = StoolSprite.CheckObject;
        BuildFunctions.HighlightPoint = StoolSprite.PlaceHighlight;
    }

    public void TableRoundOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Point;
        BuildFunctions.CreatePoint = TableRoundSprite.CreateTableRound;
        BuildFunctions.CheckPoint = TableRoundSprite.CheckObject;
        BuildFunctions.HighlightPoint = TableRoundSprite.PlaceHighlight;
    }

    public void TableSquareOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Point;
        BuildFunctions.CreatePoint = TableSquareSprite.CreateTableSquare;
        BuildFunctions.CheckPoint = TableSquareSprite.CheckObject;
        BuildFunctions.HighlightPoint = TableSquareSprite.PlaceHighlight;
    }

    public void BarOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Line;
        BuildFunctions.CreatePoint = BarSprite.CreateBar;
        BuildFunctions.CheckPoint = BarSprite.CheckObject;
        BuildFunctions.HighlightPoint = BarSprite.PlaceHighlight;
    }

    public void DemolishOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Demolish;
        GUI.Instance.CloseObjects();
    }

    public void UpOnClick()
    {
        GameManager.Instance.ChangeLevel(true);
    }

    public void DownOnClick()
    {
        GameManager.Instance.ChangeLevel(false);
    }

    public void SaveOnClick()
    {
        DataPersistenceManager.instance.SaveGame();
    }

    public void QuestsOnClick()
    {
        GUI.OpenCloseQuests();
    }

    public void HiresOnClick()
    {
        GUI.OpenCloseHires();
    }

    public void SelectQuestOnClick(int id)
    {
        GUI.SelectQuest(id);
    }

    public void AdventurerSelectCloseOnClick()
    {
        GUI.CloseAdventurerSelect();
    }

    public void StartQuestOnClick()
    {
        GUI.StartQuest();
    }
}
