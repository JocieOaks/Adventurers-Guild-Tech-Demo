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
        BuildFunctions.CreatePoint = Wall.CreateWall;
        BuildFunctions.CheckPoint = Wall.CheckObject;
        BuildFunctions.HighlightPoint = Wall.PlaceHighlight;
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
        BuildFunctions.CreatePoint = Stair.CreateStair;
        BuildFunctions.CheckPoint = Stair.CheckObject;
        BuildFunctions.HighlightPoint = Stair.PlaceHighlight;
        GUI.Instance.CloseObjects();
    }

    public void FloorOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Area;
        BuildFunctions.CreatePoint = Floor.CreateFloor;
        BuildFunctions.HighlightPoint = Floor.PlaceHighlight;
        BuildFunctions.CheckPoint = Floor.CheckObject;
        GUI.Instance.CloseObjects();
    }

    public void ObjectOnClick()
    {
        GUI.OpenCloseObjects();
    }

    public void BedOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Point;
        BuildFunctions.CreatePoint = Bed.CreateBed;
        BuildFunctions.CheckPoint = Bed.CheckObject;
        BuildFunctions.HighlightPoint = Bed.PlaceHighlight;
    }

    public void ChairOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Point;
        BuildFunctions.Direction = Direction.West;
        BuildFunctions.CreatePoint = Chair.CreateChair;
        BuildFunctions.CheckPoint = Chair.CheckObject;
        BuildFunctions.HighlightPoint = Chair.PlaceHighlight;
    }

    public void StoolOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Point;
        BuildFunctions.CreatePoint = Stool.CreateStool;
        BuildFunctions.CheckPoint = Stool.CheckObject;
        BuildFunctions.HighlightPoint = Stool.PlaceHighlight;
    }

    public void TableRoundOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Point;
        BuildFunctions.CreatePoint = TableRound.CreateTableRound;
        BuildFunctions.CheckPoint = TableRound.CheckObject;
        BuildFunctions.HighlightPoint = TableRound.PlaceHighlight;
    }

    public void TableSquareOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Point;
        BuildFunctions.CreatePoint = TableSquare.CreateTableSquare;
        BuildFunctions.CheckPoint = TableSquare.CheckObject;
        BuildFunctions.HighlightPoint = TableSquare.PlaceHighlight;
    }

    public void BarOnClick()
    {
        BuildFunctions.BuildMode = BuildMode.Line;
        BuildFunctions.CreatePoint = Bar.CreateBar;
        BuildFunctions.CheckPoint = Bar.CheckObject;
        BuildFunctions.HighlightPoint = Bar.PlaceHighlight;
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
