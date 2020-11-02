using System.IO;
using System.Text;
using UnityEngine;

public class Json
{
    const string _res = "Assets/Resources/";
    public static string ToJson(object json)
    {
        return JsonUtility.ToJson(json);
    }

    public static T ToObject<T>(string json)
    {
        return JsonUtility.FromJson<T>(json);
    }

    public static T LoadJsonFile<T>(string path)
    {
        FileStream f = new FileStream(_res + path, FileMode.Open);
        byte[] buffer = new byte[f.Length];
        f.Read(buffer, 0, buffer.Length);
        f.Close();

        string json = Encoding.UTF8.GetString(buffer);
        return ToObject<T>(json);
    }

    public static void SaveJsonFile(string path, object json)
    {
        string j = ToJson(json);
        FileStream f = new FileStream(_res + path, FileMode.Create);
        byte[] buffer = Encoding.UTF8.GetBytes(j);
        f.Write(buffer, 0, buffer.Length);
        f.Close();
    }
}