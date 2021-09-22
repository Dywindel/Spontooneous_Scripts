using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

// Persistance stores functions for saving and loading data into the game
// Like text, riftData and Save games

public static class Persistance
{
    // Persistance data path can only be gathered from the main thread. I'll do that here with a simple
    // Variable and class and call it when the game starts up
    // This is perform by the Sc_SH script
    public static string persistanceDataPath = "";
    public static void Get_PersistanceDataPath()
    {
        persistanceDataPath = Application.persistentDataPath;
    }

    // Save the game data
    public static void Save_GameData(int saveFileID, GameData pGameData)
    {
        // For different savefile ID's
        string gameData_Path = Path_SaveGame(saveFileID);

        // Create the json string
        string gameData_AsString = JsonUtility.ToJson(pGameData);
        
        // Save the string to a file
        File.WriteAllText(gameData_Path, gameData_AsString);

        /*
        // Grab a binary formatter
        BinaryFormatter formatter = new BinaryFormatter();
        // Data exchanges are handled using a filestream
        FileStream stream = new FileStream(path, FileMode.Create);

        // We use a try block, just in case something goes wrong
        try
        {    
            // Convert the data into binary and write it to the file.
            formatter.Serialize(stream, pGameData);
        }
        finally
        {
            stream.Close();
        }*/
    }

    // Load the data
    public static GameData Load_GameData(int saveFileID)
    {
        // For different savefile ID's
        string gameData_Path = Path_SaveGame(saveFileID);

        // Check the file exists
        if (File.Exists(gameData_Path))
        {
            // Grab the json string by reading the text file
            string gameData_AsString = File.ReadAllText(gameData_Path);

            // Convert the string back into the original GameData class
            GameData gameData_ToLoad = JsonUtility.FromJson<GameData>(gameData_AsString);

            // Check this game data makes sense

            return gameData_ToLoad;

            /*
            // Grab a binary formatter
            BinaryFormatter formatter = new BinaryFormatter();
            // Data exchanges are handled using a filestream
            FileStream stream = new FileStream(path, FileMode.Open);

            try
            {
                // Deseriealize the file
                GameData pass_MasterData = formatter.Deserialize(stream) as GameData;
                return pass_MasterData;
            }
            finally
            {
                stream.Close();
            }*/
        }
        else
        {
            Debug.Log("Save file not found in " + gameData_Path);

            return null;
        }
    }

    // Function for grabbing the save path
    public static string Path_SaveGame(int saveFileID)
    {
        string path = "/NA.spnt";
        switch(saveFileID)
        {
            default:
            case 0:
                path = persistanceDataPath + "/MagentaSave.spnt";
                break;
            case 1:
                path = persistanceDataPath + "/OrangeSave.spnt";
                break;
            case 2:
                path = persistanceDataPath + "/PinkSave.spnt";
                break;
            case 3:
                path = persistanceDataPath + "/CyanSave.spnt";
                break;
        }

        return path;
    }

    // For PlayerPref data, which is used to store the items that appear on the New Game screen
    public static void Save_GameData_Prefs(GameData_Prefs pGameData_Prefs)
    {
        string gameData_Pref_Path = persistanceDataPath + "/SaveData_Prefs.spnt";

        // Create the json string
        string gameData_Pref_AsString = JsonUtility.ToJson(pGameData_Prefs);
        
        // Save the string to a file
        File.WriteAllText(gameData_Pref_Path, gameData_Pref_AsString);

        /*
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);

        try
        {
            formatter.Serialize(stream, pGameData_Prefs);
        }
        finally
        {
            stream.Close();
        }*/
    }

    public static GameData_Prefs Load_GameData_Prefs()
    {
        // The blank GameData_Prefs we will fill
        GameData_Prefs gameData_Prefs_ToLoad = new GameData_Prefs();

        // For different savefile ID's
        string gameData_Pref_Path = persistanceDataPath + "/SaveData_Prefs.spnt";

        // Check the file exists
        if (File.Exists(gameData_Pref_Path))
        {
            // Grab the json string by reading the text file
            string gameData_Pref_AsString = File.ReadAllText(gameData_Pref_Path);

            // Convert the string back into the original GameData class
            Debug.Log(gameData_Pref_AsString);
            gameData_Prefs_ToLoad = JsonUtility.FromJson<GameData_Prefs>(gameData_Pref_AsString);

            return gameData_Prefs_ToLoad;

            /*
            // Grab a binary formatter
            BinaryFormatter formatter = new BinaryFormatter();
            // Data exchanges are handled using a filestream
            FileStream stream = new FileStream(path, FileMode.Open);

            try
            {
                // Deseriealize the file
                GameData_Prefs pass_MasterData = formatter.Deserialize(stream) as GameData_Prefs;
                return pass_MasterData;
            }
            finally
            {
                stream.Close();
            }
            */
        }
        else
        {
            Debug.Log("Save file not found in " + gameData_Pref_Path);
            return null;
        }
    }

    // These are Unity Editor specific scripts
    #if UNITY_EDITOR

    // Save a value, just to a single file, for generating unique puzzle IDs
    public static void Save_UniqueID(UniqueID uniqueID)
    {
        // The file location is the same regardless
        string persistanceDataPath = Application.dataPath + "\\Levels\\";
        string filePath = persistanceDataPath + "UniqueIDs.unq";
        
        // Grab a binary formatter
        BinaryFormatter formatter = new BinaryFormatter();
        // Data exchanges are handled using a filestream
        FileStream stream = new FileStream(filePath, FileMode.Create);

        // We use a try block, just in case something goes wrong
        try
        {
            // Convert the data into binary and write it to the file.
            formatter.Serialize(stream, uniqueID);
        }
        finally
        {
            stream.Close();
        }
    }

    // Load the unique ID value
    public static UniqueID Load_UniqueID()
    {
        // default(T) returns null or 0 or an empty list depending on what T was specified
        // It basically means generic null
        UniqueID uniqueID = default(UniqueID);

        // The file location is the same regardless
        string persistanceDataPath = Application.dataPath + "\\Levels\\";
        string filePath = persistanceDataPath + "UniqueIDs.unq";

        // Grab a binary formatter
        BinaryFormatter formatter = new BinaryFormatter();
        // Data exchanges are handled using a filestream
        FileStream stream = new FileStream(filePath, FileMode.Open);

        try
        {
            // Deseriealize the file
            uniqueID = (UniqueID)formatter.Deserialize(stream);
        }
        finally
        {
            stream.Close();
        }

        // We return the data, if it exists
        return uniqueID;
    }

    // Save a file for any serializable data type T
    public static void Save_Rift<T>(T data)
    {
        // Now, the user doesn't pick puzzle filenames
        // Instead, each puzzle is given a unique ID as a name
        string fileFolder = Application.dataPath + "\\Levels\\Levels_SO\\";
        UniqueID uniqueID = Persistance.Load_UniqueID();
        string fileName = "PuzzleSO_" + uniqueID.uniqueIDTotal + ".spn";
        string filePath = fileFolder + fileName;
        
        // Grab a binary formatter
        BinaryFormatter formatter = new BinaryFormatter();
        // Data exchanges are handled using a filestream
        FileStream stream = new FileStream(filePath, FileMode.Create);

        // We use a try block, just in case something goes wrong
        try
        {
            // Convert the data into binary and write it to the file.
            formatter.Serialize(stream, data);
        }
        finally
        {
            stream.Close();
        }
    }

    // Load a file of any serializable data type T
    public static T Load_Rift<T>(string folderPath)
    {
        // default(T) returns null or 0 or an empty list depending on what T was specified
        // It basically means generic null
        T data = default(T);

        // Open the file inspector
        string persistanceDataPath = Application.persistentDataPath + folderPath;
        string filePath = EditorUtility.OpenFilePanel("Blank", persistanceDataPath, "spn");

        // If we don't select a file, we don't clear anything in the level
        if (filePath != "")
        {
            // Grab a binary formatter
            BinaryFormatter formatter = new BinaryFormatter();
            // Data exchanges are handled using a filestream
            FileStream stream = new FileStream(filePath, FileMode.Open);

            try
            {
                // Deseriealize the file
                data = (T)formatter.Deserialize(stream);
            }
            finally
            {
                stream.Close();
            }
        }

        // We return the data, if it exists
        return data;
    }

    #endif
}
