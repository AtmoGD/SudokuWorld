using System;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
public static class DataManager
{
    public static void SaveData<T>(T _data, string _path)
    {
        try
        {
            Serialize<T>(_data, _path);
        }
        catch (Exception e)
        {
            Debug.LogError("CAN'T SAVE DATA! - REASON: " + e.Message);
            DeleteData(_path);
        }
    }

    public static T LoadData<T>(string _path)
    {
        try
        {
            return Deserialize<T>(_path);
        }
        catch (Exception e)
        {
            Debug.Log("CAN'T LOAD DATA! - REASON: " + e.Message);
            return default;
        }
    }

    public static bool DeleteData(string _path)
    {
        if (File.Exists(_path))
        {
            File.Delete(_path);
            return true;
        }
        return false;
    }

    private static void Serialize<T>(T _data, string _path)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(_path, FileMode.Create);

        formatter.Serialize(stream, _data);
        stream.Close();
    }

    private static T Deserialize<T>(string _path)
    {
        if (File.Exists(_path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(_path, FileMode.Open);

            T data = (T)formatter.Deserialize(stream);
            stream.Close();
            return data;
        }
        else
        {
            Debug.Log("FILE DOESN'T EXISTS ON GIVEN PATH!");
            return default;
        }
    }

}
