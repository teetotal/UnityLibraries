using System.IO;
using System.Text;
using UnityEngine;

public class Json
{
    static string _res = Application.dataPath + "Assets/Resources/";
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
        TextAsset buffer = Resources.Load<TextAsset>(path);
        /*
        FileStream f = new FileStream(_res + path, FileMode.Open);
        byte[] buffer = new byte[f.Length];
        f.Read(buffer, 0, buffer.Length);
        f.Close();
        */
        
        string json = Encoding.UTF8.GetString(buffer.bytes);
        return ToObject<T>(json);
    }

    public static void SaveJsonFile(string path, object json)
    {
        //resources에 저장 못함. 저장하는법 찾아서 고쳐야 함
        string j = ToJson(json);
        FileStream f = new FileStream(_res + path, FileMode.Create);
        byte[] buffer = Encoding.UTF8.GetBytes(j);
        f.Write(buffer, 0, buffer.Length);
        f.Close();
    }
}