using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
Based on the Save Load System by Trevor Mock
https://github.com/trevermock/save-load-system
*/

/// <summary>
/// The <see cref="IDataPersistence"/> interface is the interface for classes that implement saving and loading for <see cref="DataPersistenceManager"/>.
/// </summary>
public interface IDataPersistence
{
    /// <summary>
    /// Saves data into a serializable form to the <see cref="GameData"/> object.
    /// </summary>
    /// <param name="gameData">The <see cref="GameData"/> being saved to.</param>
    void SaveData(GameData gameData);

    /// <summary>
    /// Loads data from a <see cref="GameData"/> object.
    /// </summary>
    /// <param name="gameData">The <see cref="GameData"/> being loaded.</param>
    void LoadData(GameData gameData);
}
