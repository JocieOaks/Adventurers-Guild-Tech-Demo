using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DataPersistenceManager : MonoBehaviour
{
    public static bool SAVE = true;
    public static bool LOAD = true;
    [Header("File Storage Config")]
    [SerializeField] private string fileName;

    private GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    public List<IDataPersistence> NonMonoDataPersistenceObjects { get; } = new List<IDataPersistence>();
    private FileDataHandler dataHandler;

    public static DataPersistenceManager instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one Data Persistence Manager in the scene.");
        }
        instance = this;
    }

    private void Start()
    {
        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        dataPersistenceObjects = FindAllDataPersistenceObjects();
        StartCoroutine(LoadGame());
    }

    public void NewGame()
    {
        gameData = dataHandler.LoadNew();
    }

    public IEnumerator LoadGame()
    {
        yield return new WaitUntil(() => Graphics.Ready == true);
        // load any saved data from a file using the data handler
        gameData = dataHandler.Load();

        GameManager.Instance.Quests = dataHandler.LoadQuestData();

        // if no data can be loaded, initialize to a new game
        if (gameData == null)
        {
            Debug.Log("No data was found. Initializing data to defaults.");
            NewGame();
        }

        gameData.Names = dataHandler.LoadNames();

        if (LOAD)
        { // push the loaded data to all other scripts that need it
            foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
            {
                dataPersistenceObj.LoadData(gameData);
            }

            foreach (IDataPersistence dataPersistenceObj in NonMonoDataPersistenceObjects)
            {
                dataPersistenceObj.LoadData(gameData);
            }
        }
    }

    public void SaveGame()
    {
        // pass the data to other scripts so they can update it
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(gameData);
        }

        foreach (IDataPersistence dataPersistenceObj in NonMonoDataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(gameData);
        }

        // save that data to a file using the data handler
        if (SAVE)
            dataHandler.Save(gameData);
    }

    private void OnApplicationQuit()
    {
        //SaveGame();
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>()
            .OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistenceObjects);
    }
}
