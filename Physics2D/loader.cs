/*

    Json파일을 읽어서 동적으로 object를 add하는 class

*/
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate GameObject LoaderCallBack(string layerName,string name, string tag, Vector2 position, Vector2 size);
public delegate void LoaderPostCallBack(GameObject obj, string layerName);

[Serializable]
public class Loader
{
    [Serializable]
    public struct LoaderObject
    {
        public Vector2Int position;
        public Vector2 pivot;
        public string name;
        public string tag;
        public string prefab;
        public bool ui;
        public string text;
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
        public List<LoaderObject> _Objects;
    }

    public string _Name;
    public Vector2Int _GridDim;
    public Vector2 _Margin;
    public Vector2 _InnerMargin;
    public List<LoaderObject> _Objects;
    public List<sub> _Subs; 

    //UI
    protected Camera mCamera;
    protected Canvas mCanvas;
    //생성한 object 저장용
    protected Dictionary<string, List<GameObject>> mObjects = new Dictionary<string, List<GameObject>>();
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
    public void SetUI(Camera camera, ref Canvas canvas)
    {
        mCamera = camera;
        mCanvas = canvas;
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
    public GameObject GetObject(string name)
    {
        if(mObjects.ContainsKey(name))
        {
            return mObjects[name][0];
        }
        return null;
    }
    public List<GameObject> GetObjects(string name)
    {
        if(mObjects.ContainsKey(name))
        {
            return mObjects[name];
        }
        return null;
    }
    private Vector2 GetPosition(Vector2 position, Vector2 gridSize,Vector2 pivot)
    {
        if(pivot == null)
        {
            return position;
        }

        float diffX = gridSize.x * pivot.x;
        float diffY = gridSize.y * pivot.y;

        return new Vector2(position.x + diffX, position.y + diffY);
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
            
            CreateObject(_Name, _Objects[n], GetPosition(points[idx], gridSize, _Objects[n].pivot), gridSize, cb, cbPost);
        }

        //_subs
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
                LoaderObject node = s._Objects[n];
                int idx = (node.position.y *  s._GridDim.x) + node.position.x;
                if(idx > points.Count)
                    throw new Exception("Invalid index. check _GridDim & _Objects position. " + s._Name);
                
                CreateObject(s._Name, node, GetPosition(points[idx], gridSize, node.pivot), gridSize, cb, cbPost);
            }
        }
    }
    private void CreateObject(string layerName,LoaderObject node, Vector2 point, Vector2 gridSize, LoaderCallBack cb, LoaderPostCallBack cbPost)
    {
        GameObject obj;

        //prefab   
        if(node.prefab != null && node.prefab.Length > 0)
        {
            obj = Resources.Load<GameObject>(node.prefab);
            if(node.ui == false)
            {
                mPos.SetGameObjectSize(ref obj, gridSize.x, gridSize.y);
                obj.transform.position = new Vector3(point.x, point.y, 0);
            }
        }
        else
        {
            obj = cb(layerName, node.name, node.tag, point, gridSize);
        }
        
        if(obj != null)
        {
            obj = UnityEngine.Object.Instantiate(obj);
            obj.tag = node.tag;
            obj.name = node.name;   
            //ui
            if(node.ui == true)
            {
                obj.transform.position = mCamera.WorldToScreenPoint(new Vector3(point.x, point.y, 0));
                obj.transform.SetParent(mCanvas.transform);
                
                //resizing
                if(node.prefab != null && node.prefab.Length > 0)
                {
                    Vector3 gridScreenSize = mCamera.WorldToScreenPoint(new Vector3(point.x + gridSize.x, point.y + gridSize.y, 0));
                    gridScreenSize.x -= obj.transform.position.x;
                    gridScreenSize.y -= obj.transform.position.y;

                    obj.GetComponent<RectTransform>().sizeDelta = new Vector2(gridScreenSize.x, gridScreenSize.y);
                }
            }
            //text
            if(node.text != null)
            {
                Text txt = obj.GetComponent<Text>();
                if(txt == null)
                {
                    txt = obj.GetComponentInChildren<Text>();
                }
                if(txt != null)
                {
                    txt.text = node.text;
                }
            }

            if(mObjects.ContainsKey(node.name) == false)
            {
                mObjects[node.name] = new List<GameObject>();
            }
            mObjects[node.name].Add(obj);

            if(cbPost != null)
            {
                cbPost(obj, layerName);
            }
        }   
    }
}