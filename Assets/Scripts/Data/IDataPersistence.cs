using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataPersistence
{
    void SaveData(GameData gameData);

    void LoadData(GameData gameData);
}
