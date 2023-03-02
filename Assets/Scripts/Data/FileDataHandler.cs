using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

/* 
Based on the Save Load System by Trevor Mock
https://github.com/trevermock/save-load-system
*/

/// <summary>
/// The <see cref="FileDataHandler"/> class controls the controls the interface between the game and the save files.
/// </summary>
public class FileDataHandler
{
    readonly string _fileDirectory;
    readonly string _fileName;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileDataHandler"/> class.
    /// </summary>
    /// <param name="directory">The PATH to the directory that contains the game's save files.</param>
    /// <param name="filename">The name of the game's save file.</param>
    public FileDataHandler(string directory, string filename)
    {
        _fileDirectory = directory;
        _fileName = filename;
    }

    /// <summary>
    /// Pulls data from a save file to create a new <see cref="GameData"/> object.
    /// </summary>
    /// <returns>The new <see cref="GameData"/> taken from a save file.</returns>
    public GameData Load()
    {
        // use Path.Combine to account for different OS's having different path separators
        string fullPath = Path.Combine(_fileDirectory, _fileName);
        GameData loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                // load the serialized data from the file
                string dataToLoad = "";
                using (FileStream stream = new(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                // deserialize the data from Json back into the C# object
                loadedData = JsonConvert.DeserializeObject<GameData>(dataToLoad);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error occured when trying to load data from file: " + fullPath + "\n" + e);
            }
        }
        return loadedData;
    }

    /// <summary>
    /// Loads the list of names used to randomly generate <see cref="Actor"/> from the names.txt file in Resources.
    /// </summary>
    /// <returns>Returns the list of <see cref="Actor"/> names.</returns>
    public List<string> LoadNames()
    {
        List<string> loadedData = null;

        try
        {
            // load the serialized data from the file
            var dataToLoad = Resources.Load<TextAsset>("names");

            // deserialize the data from Json back into the C# object
            loadedData = JsonConvert.DeserializeObject<List<string>>(dataToLoad.text);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error occured when trying to load data from file: quest_data" + "\n" + e);
        }

        return loadedData;
    }

    /// <summary>
    /// Loads a new game from the start_save.txt file in Resources.
    /// </summary>
    /// <returns>Returns a new <see cref="GameData"/> based on the starting save.</returns>
    public GameData LoadNew()
    {
        GameData loadedData = null;
        try
        {
            // load the serialized data from the file
            var dataToLoad = Resources.Load<TextAsset>("start_save");

            // deserialize the data from Json back into the C# object
            loadedData = JsonConvert.DeserializeObject<GameData>(dataToLoad.text);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error occured when trying to load data from file: start_save" + "\n" + e);
        }
        
        return loadedData;
    }

    /// <summary>
    /// Returns the data to be used for <see cref="Quest"/>s from the quest_data.txt file in Resources.
    /// </summary>
    /// <returns>Returns the list of <see cref="QuestData"/> to be used to build the game's <see cref="Quest"/>s.</returns>
    public List<QuestData> LoadQuestData()
    {
        List<QuestData> loadedData = null;

        try
        {
            // load the serialized data from the file
            var dataToLoad = Resources.Load<TextAsset>("quest_data");

            // deserialize the data from Json back into the C# object
            loadedData = JsonConvert.DeserializeObject<List<QuestData>>(dataToLoad.text);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error occured when trying to load data from file: quest_data" + "\n" + e);
        }
        
        return loadedData;
    }

    /// <summary>
    /// Saves the current state of the game to a save file.
    /// </summary>
    /// <param name="gameData">A <see cref="GameData"/> object that contains all the data for the current state of the game.</param>
    public void Save(GameData gameData)
    {
        // use Path.Combine to account for different OS's having different path separators
        string fullPath = Path.Combine(_fileDirectory, _fileName);
        try
        {
            // create the directory the file will be written to if it doesn't already exist
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // serialize the C# game data object into Json
            string dataToStore = JsonConvert.SerializeObject(gameData, Formatting.Indented);

            // write the serialized data to the file
            using (FileStream stream = new(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
        }
    }
}
