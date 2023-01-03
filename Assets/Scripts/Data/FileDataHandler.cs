using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class FileDataHandler
{
    string _fileDirectory;
    string _fileName;

    public FileDataHandler(string directory, string filename)
    {
        _fileDirectory = directory;
        _fileName = filename;
    }

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
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
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
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
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
