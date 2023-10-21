using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

/* 
Based on the Save Load System by Trevor Mock
https://github.com/trevermock/save-load-system
*/

namespace Assets.Scripts.Data
{
    /// <summary>
    /// The DataPersistenceManager is a singleton class that controls the loading and saving of persistent save data.
    /// </summary>
    public class DataPersistenceManager : MonoBehaviour
    {
        private const bool LOAD = true;
        private const bool SAVE = true;
        private FileDataHandler _dataHandler;

        private List<IDataPersistence> _dataPersistenceObjects;

        [Header("File Storage Config")]
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        [SerializeField] private string _fileName;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

        private GameData _gameData;

        /// <value>Gives a reference to the <see cref="DataPersistenceManager"/> singleton.</value>
        public static DataPersistenceManager Instance { get; private set; }

        /// <value>A list of objects that have persistent data but do not inherit from MonoBehaviour and thus cannot be access by <see cref="FindAllDataPersistenceObjects"/>.</value>
        public List<IDataPersistence> NonMonoDataPersistenceObjects { get; } = new();

        /// <summary>
        /// Called whenever a save file is loaded into the game.
        /// </summary>
        /// <returns>Returns a <see cref="WaitUntil"/> object to wait for <see cref="Graphics"/> to be ready.</returns>
        public IEnumerator LoadGame()
        {
            yield return new WaitUntil(() => Graphics.Ready);

            // load any saved data from a file using the data handler
            _gameData = _dataHandler.Load();

        

            // if no data can be loaded, initialize to a new game
            if (_gameData == null)
            {
                Debug.Log("No data was found. Initializing data to defaults.");
                NewGame();
            }

            _gameData!.Names = _dataHandler.LoadNames();
            _gameData.Quests = _dataHandler.LoadQuestData();

            if (LOAD)
            { 
                // push the loaded data to all other scripts that need it
                foreach (IDataPersistence dataPersistenceObj in _dataPersistenceObjects)
                {
                    dataPersistenceObj.LoadData(_gameData);
                }

                foreach (IDataPersistence dataPersistenceObj in NonMonoDataPersistenceObjects)
                {
                    dataPersistenceObj.LoadData(_gameData);
                }
            }
        }

        /// <summary>
        /// Loads a new game from the starting save file.
        /// </summary>
        public void NewGame()
        {
            _gameData = _dataHandler.LoadNew();
        }

        /// <summary>
        /// Save's the game data to an external file.
        /// </summary>
        public void SaveGame()
        {
            // pass the data to other scripts so they can update it
            foreach (IDataPersistence dataPersistenceObj in _dataPersistenceObjects)
            {
                dataPersistenceObj.SaveData(_gameData);
            }

            foreach (IDataPersistence dataPersistenceObj in NonMonoDataPersistenceObjects)
            {
                dataPersistenceObj.SaveData(_gameData);
            }

            // save that data to a file using the data handler
            if (SAVE)
                _dataHandler.Save(_gameData);
        }

        [UsedImplicitly]
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
        private List<IDataPersistence> FindAllDataPersistenceObjects()
        {
            IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>()
                .OfType<IDataPersistence>();

            return new List<IDataPersistence>(dataPersistenceObjects);
        }

        [UsedImplicitly]
        private void Start()
        {
            _dataHandler = new FileDataHandler(Application.persistentDataPath, _fileName);
            _dataPersistenceObjects = FindAllDataPersistenceObjects();
            StartCoroutine(LoadGame());
        }
    }
}
