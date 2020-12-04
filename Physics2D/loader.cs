/*

    Json파일을 읽어서 동적으로 object를 add하는 class

*/
using System;
using System.Collections.Generic;
using UnityEngine;

public delegate GameObject LoaderCallBack(string layerName,string name, string tag, Vector2 position, Vector2 size);
public delegate void LoaderPostCallBack(GameObject obj, string layerName);

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

    [Serializable]
    public struct sub
    {
        public string _Name;
        public Vector2Int _From;
        public Vector2Int _To;
        public Vector2Int _GridDim;
        public Vector2 _Margin;
        public Vector2 _InnerMargin;
        public List<LoaderObject> _Objects; // = new List<LoaderObject>();
    }

    public string _Name;
    public Vector2Int _GridDim;
    public Vector2 _Margin;
    public Vector2 _InnerMargin;
    public List<LoaderObject> _Objects; // = new List<LoaderObject>();
    public List<sub> _Subs; // = new List<sub>();
    protected List<GameObject> mObjList = new List<GameObject>(); //생성한 object 저장용
    protected HashSet<string> mDrawGridSubSet = new HashSet<string>();
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
        Instance._Name = obj._Name;
        Instance._GridDim = new Vector2Int(obj._GridDim.x, obj._GridDim.y);
        Instance._Margin = new Vector2(obj._Margin.x, obj._Margin.y);
        Instance._InnerMargin = new Vector2(obj._InnerMargin.x, obj._InnerMargin.y);
        Instance._Objects = new List<LoaderObject>(obj._Objects);
        Instance._Subs = new List<sub>(obj._Subs);
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
    public void AddSubDrawGrid(string nameOfSub)
    {
        mDrawGridSubSet.Add(nameOfSub);
    }

    public void AddComponents(LoaderCallBack cb, LoaderPostCallBack cbPost = null)
    {   
        List<Vector2> points = mPos.GetGridPoints(_Margin, _GridDim);
        Vector2 gridSize = mPos.GetGridSize(_Margin, _InnerMargin, _GridDim);

        for(int n = 0; n < _Objects.Count; n++)
        {
            int idx = (_Objects[n].position.y *  _GridDim.x) + _Objects[n].position.x;
            if(idx > points.Count)
                throw new Exception("Invalid index. check _GridDim & _Objects position");

            GameObject obj = cb(_Name, _Objects[n].name, _Objects[n].tag, points[idx], gridSize);
            if(obj != null)
                mObjList.Add(UnityEngine.Object.Instantiate(obj));
        }

        //_subs 처리 해야됨
        for(int i = 0; i < _Subs.Count; i++)
        {
            sub s = _Subs[i];
            List<Vector2> minMax = mPos.GetGridMinMax(_Margin, _InnerMargin, _GridDim, s._From, s._To);
            points = mPos.GetGridPoints(s._Margin, s._GridDim, minMax[0], minMax[1]);
            gridSize = mPos.GetGridSize(minMax[0], minMax[1], s._InnerMargin, s._GridDim);

            if(mDrawGridSubSet.Contains(s._Name) == true)
            {
                mPos.DrawGrid(s._Margin, s._GridDim, minMax[0], minMax[1]);
            }

            for(int n = 0; n < s._Objects.Count; n++)
            {
                int idx = (s._Objects[n].position.y *  s._GridDim.x) + s._Objects[n].position.x;
                if(idx > points.Count)
                    throw new Exception("Invalid index. check _GridDim & _Objects position. " + s._Name);
                
                GameObject obj = cb(s._Name, s._Objects[n].name, s._Objects[n].tag, points[idx], gridSize);
                if(obj != null)
                {
                    GameObject instance = UnityEngine.Object.Instantiate(obj);
                    mObjList.Add(instance);
                    if(cbPost != null)
                    {
                        cbPost(instance, s._Name);
                    }
                }    
            }
        }
    }
}