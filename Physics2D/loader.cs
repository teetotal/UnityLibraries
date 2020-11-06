/*

    Json파일을 읽어서 동적으로 object를 add하는 class

*/
using System;
using System.Collections.Generic;
using UnityEngine;

public delegate GameObject LoaderCallBack(string name, string tag, Vector2 position, Vector2 size);
[Serializable]
public class Loader
{
    [Serializable]
    public struct LoaderObject
    {
        public Vector2Int position;
        public string name;
        public string tag;
    };
    public Vector2Int _GridDim;
    public Vector2 _Margin;
    public Vector2 _InnerMargin;
    public List<LoaderObject> _Objects = new List<LoaderObject>();
    protected List<GameObject> mObjList = new List<GameObject>(); //생성한 object 저장용
    protected Positions mPos = new Positions();

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
        Instance._InnerMargin = new Vector2(obj._InnerMargin.x, obj._InnerMargin.y);
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

    public void DrawGrid()
    {
        mPos.DrawGrid(_Margin, _GridDim);
    }

    public void AddComponents(LoaderCallBack cb)
    {   
        List<Vector2> points = mPos.GetGridPoints(_Margin, _GridDim);
        Vector2 gridSize = mPos.GetGridSize(_Margin, _InnerMargin, _GridDim);

        for(int n = 0; n < _Objects.Count; n++)
        {
            int idx = (_Objects[n].position.y *  _GridDim.x) + _Objects[n].position.x;
            if(idx > points.Count)
                throw new Exception("Invalid index. check _GridDim & _Objects position");

            GameObject obj = cb(_Objects[n].name, _Objects[n].tag, points[idx], gridSize);
            mObjList.Add(UnityEngine.Object.Instantiate(obj));
        }
    }
}