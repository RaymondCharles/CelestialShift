using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
public static class SaveSystem
{
    public static void SavePlayer(FirstPersonController player, DayNightCycle time)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/player.data";
        FileStream stream = new FileStream(path,FileMode.Create);

        PlayerData data = new PlayerData(player, time);

        formatter.Serialize(stream,data);
        stream.Close();

    }

    public static PlayerData LoadPlayer()
    {
        string path = Application.persistentDataPath + "/player.data";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            PlayerData data = formatter.Deserialize(stream) as PlayerData;
            stream.Close();
            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }

    }
    public static bool SaveExists()
    {
        string path = Application.persistentDataPath + "/player.data";
        bool exists = System.IO.File.Exists(path);
        Debug.Log("Save exists? " + exists + " Path: " + path);
        return exists;
    }




}
