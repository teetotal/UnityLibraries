using System;
using System.Collections.Generic;
using UnityEngine;

public delegate GameObject LoaderCallBack(int id);
[Serializable]
public class Loader
{
    [Serializable]
    public struct LoaderObject
    {
        public Vector2Int position;
        public int id;
    };
    public Vector2Int _GridDim;
    public Vector2 _Margin;
    public Vector2 _InnerMargin;
    public List<LoaderObject> _Objects = new List<LoaderObject>();

    private static readonly Lazy<Loader> hInstance =
        new Lazy<Loader>(() => new Loader());
 
    public static Loader Instance
    {
        get {
            return hInstance.Value;
        }
        
    }

    protected Loader()
    {
    }

    protected void Init(Loader obj)
    {
        Instance._GridDim = new Vector2Int(obj._GridDim.x, obj._GridDim.y);
        Instance._Margin = new Vector2(obj._Margin.x, obj._Margin.y);
        Instance._Objects = new List<LoaderObject>(obj._Objects);
    }

    public bool LoadJsonFile(string path)
    {
        Loader obj = Json.LoadJsonFile<Loader>(path);
        Init(obj);

        return true;
    }

    public void SaveJsonFile(string path)
    {
        Json.SaveJsonFile(path, Instance);
    }

    public void AddComponents(LoaderCallBack cb)
    {

    }
}