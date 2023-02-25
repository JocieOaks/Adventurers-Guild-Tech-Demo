using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/* 
Based on the Save Load System by Trevor Mock
https://github.com/trevermock/save-load-system
*/

/// <summary>
/// The DataPersistenceManager is a singleton class that controls the loading and saving of persistent save data.
/// </summary>
public class DataPersistenceManager : MonoBehaviour
{
    const bool LOAD = true;
    const bool SAVE = true;
    FileDataHandler dataHandler;

    List<IDataPersistence> dataPersistenceObjects;

    [Header("File Storage Config")]
    [SerializeField] private string fileName;

    GameData gameData;

    /// <value>Gives a reference to the <see cref="DataPersistenceManager"/> singleton.</value>
    public static DataPersistenceManager Instance { get; private set; }

    /// <value>A list of objects that have persistent data but do not inherit from MonoBehaviour and thus cannot be access by <see cref="FindAllDataPersistenceObjects"/>.</value>
    public List<IDataPersistence> NonMonoDataPersistenceObjects { get; } = new List<IDataPersistence>();

    /// <summary>
    /// Called whenever a save file is loaded into the game.
    /// </summary>
    /// <returns>Returns a <see cref="WaitUntil"/> object to wait for <see cref="Graphics"/> to be ready.</returns>
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
        { 
            // push the loaded data to all other scripts that need it
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

    /// <summary>
    /// Loads a new game from the starting save file.
    /// </summary>
    public void NewGame()
    {
        gameData = dataHandler.LoadNew();
    }

    /// <summary>
    /// Save's the game data to an external file.
    /// </summary>
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

    /// <inheritdoc/>
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one Data Persistence Manager in the scene.");
        }
        Instance = this;
    }

    /// <summary>
    /// Find's all MonoBehaviour objects in the current scene that have the IDataPersistence interface.
    /// </summary>
    /// <returns></returns>
    List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>()
            .OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistenceObjects);
    }

    /// <inheritdoc/>
    private void Start()
    {
        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        dataPersistenceObjects = FindAllDataPersistenceObjects();
        StartCoroutine(LoadGame());
    }
}
