/*

    Json파일을 읽어서 동적으로 object를 add하는 class

*/
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class LoaderPerspective
{   
    public string _Name;
    public Vector2Int _GridDim;
    public Vector2 _Margin;
    public Vector2 _InnerMargin;
    public List<Loader.LoaderObject> _Objects;
    public List<Loader.sub> _Subs; 

    //UI
    protected Camera mCamera;
    protected Transform mCanvas;
    protected LoaderButtonOnClickCallBack mButtonCallBack;
    //생성한 object 저장용
    public Dictionary<string, List<GameObject>> mObjects = new Dictionary<string, List<GameObject>>();
    protected HashSet<string> mDrawGridSubSet = new HashSet<string>();
    protected Positions mPos = new Positions();

    private static readonly Lazy<LoaderPerspective> hInstance =
        new Lazy<LoaderPerspective>(() => new LoaderPerspective());
 
    public static LoaderPerspective Instance
    {
        get {
            return hInstance.Value;
        } 
    }
    protected LoaderPerspective()
    {
    }
    protected void Init(LoaderPerspective obj)
    {
        Instance._Name = obj._Name;
        Instance._GridDim = new Vector2Int(obj._GridDim.x, obj._GridDim.y);
        Instance._Margin = new Vector2(obj._Margin.x, obj._Margin.y);
        Instance._InnerMargin = new Vector2(obj._InnerMargin.x, obj._InnerMargin.y);
        Instance._Objects = new List<Loader.LoaderObject>(obj._Objects);
        Instance._Subs = new List<Loader.sub>(obj._Subs);
    }
    public void SetUI(Camera camera, ref Transform canvas, LoaderButtonOnClickCallBack buttonCallBack)
    {
        mCamera = camera;
        mCanvas = canvas;
        mButtonCallBack = buttonCallBack;
    }
    public bool LoadJsonFile(string path)
    {
        LoaderPerspective obj = Json.LoadJsonFileFromResources<LoaderPerspective>(path);
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
        Vector2 min = mPos.GetWorldPointMin(0, 0, true);
        Vector2 max = mPos.GetWorldPointMax(0, 0, true);
        Vector2 fullSize = mPos.GetSize(min, max);

        List<Vector2> points = mPos.GetGridPoints(_Margin, _GridDim, min, max);
        Vector2 fullGridSize = mPos.GetGridSize(fullSize, _InnerMargin, _GridDim.x, _GridDim.y);
        mObjects["BG"] = new List<GameObject>();

        for(int n = 0; n < _Objects.Count; n++)
        {
            int idx = (_Objects[n].position.y *  _GridDim.x) + _Objects[n].position.x;
            if(idx > points.Count)
                throw new Exception("Invalid index. check _GridDim & _Objects position");
            
            Loader.sub s = new Loader.sub();
            CreateObject(null, _Objects[n], GetPosition(points[idx], fullGridSize, _Objects[n].pivot), fullGridSize, cb, cbPost, _Objects[n], s);
        }

        //_subs
        for(int i = 0; i < _Subs.Count; i++)
        {
            Loader.sub s = _Subs[i];
            List<Vector2> minMax = mPos.GetGridMinMax(fullGridSize, fullSize, min, _Margin, _InnerMargin, _GridDim, s._From, s._To);
            points = mPos.GetGridPoints(s._Margin, s._GridDim, minMax[0], minMax[1]);
            Vector2 gridSize = mPos.GetGridSize(minMax[0], minMax[1], s._Margin, s._InnerMargin, s._GridDim);
            GameObject layer;
            List<float> bgColor;
            //bg color
            if(s._BGColor.Count > 0)
            {
                bgColor = s._BGColor;
            }
            else
            {
                bgColor = new List<float>();
                for(int n = 0; n < 4; n++) bgColor.Add(0.0f);
            }
            layer = CreatePanel(s._Name, minMax, bgColor, s._Margin);
            mObjects["BG"].Add(layer);

            for(int n = 0; n < s._Objects.Count; n++)
            {
                Loader.LoaderObject node = s._Objects[n];
                int idx = (node.position.y *  s._GridDim.x) + node.position.x;
                if(idx > points.Count)
                    throw new Exception("Invalid index. check _GridDim & _Objects position. " + s._Name);
                
                CreateObject(layer, node, GetPosition(points[idx], gridSize, node.pivot), gridSize, cb, cbPost, node, s);
            }
            //Draw Line
            if(mDrawGridSubSet.Contains(s._Name) == true || s._DrawGridLine == true)
            {
                mPos.DrawGrid(s._Margin, s._GridDim, minMax[0], minMax[1], mCanvas);
            }
        }
    }
    private void CreateObject(GameObject layer, Loader.LoaderObject node, Vector2 point, Vector2 gridSize, LoaderCallBack cb, LoaderPostCallBack cbPost
        , Loader.LoaderObject loaderObj, Loader.sub subObj)
    {
        GameObject obj;
        string layerName;
        if(layer != null)
            layerName = layer.name;
        else
            layerName = _Name;

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
                obj.transform.position = new Vector3(point.x, point.y, 0);//mCamera.WorldToScreenPoint(new Vector3(point.x, point.y, 0));
                if(layer == null)
                    obj.transform.SetParent(mCanvas);
                else
                    obj.transform.SetParent(layer.transform);
                
                //resizing
                if(node.prefab != null && node.prefab.Length > 0)
                {
                    //span 처리
                    if(subObj._InnerMargin.x > 0 || subObj._InnerMargin.y > 0)
                    {
                        gridSize.x = (gridSize.x * (loaderObj.span.x + 1)) + (subObj._InnerMargin.x * 2.0f * loaderObj.span.x);
                        gridSize.y = (gridSize.y * (loaderObj.span.y + 1)) + (subObj._InnerMargin.y * 2.0f * loaderObj.span.y);
                    }
                    Vector3 gridScreenSize = new Vector3(point.x + gridSize.x, point.y + gridSize.y, 0);//mCamera.WorldToScreenPoint(new Vector3(point.x + gridSize.x, point.y + gridSize.y, 0));
                    gridScreenSize.x -= obj.transform.position.x;
                    gridScreenSize.y -= obj.transform.position.y;
                   
                    obj.GetComponent<RectTransform>().sizeDelta = new Vector2(gridScreenSize.x, gridScreenSize.y);
                }
                //onclick
                Button btn = obj.GetComponent<Button>();
                if(btn != null)
                {
                    btn.onClick.AddListener(() => {mButtonCallBack(obj);});
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
    GameObject CreatePanel(string name, List<Vector2> minMax, List<float> color, Vector2 margin)
    {
        GameObject panel = new GameObject(name);
        panel.AddComponent<CanvasRenderer>();
        Image i = panel.AddComponent<Image>();
        Color c = new Color(color[0], color[1], color[2], color[3]);
        i.color = c;
        //panel = UnityEngine.Object.Instantiate(panel);

        Vector2 center = new Vector2();
        center.x = minMax[0].x + ((minMax[1].x - minMax[0].x) * 0.5f);
        center.y = minMax[0].y + ((minMax[1].y - minMax[0].y) * 0.5f);

        panel.transform.position = new Vector3(center.x, center.y, 0);//mCamera.WorldToScreenPoint(new Vector3(center.x, center.y, 0));

        Vector3 minPosition = new Vector3(minMax[0].x + margin.x, minMax[0].y + margin.y, 0); // mCamera.WorldToScreenPoint(new Vector3(minMax[0].x + margin.x, minMax[0].y + margin.y, 0));
        Vector3 maxPosition = new Vector3(minMax[1].x - margin.x, minMax[1].y - margin.y, 0); // mCamera.WorldToScreenPoint(new Vector3(minMax[1].x - margin.x, minMax[1].y - margin.y, 0));

        panel.GetComponent<RectTransform>().sizeDelta = new Vector2(maxPosition.x - minPosition.x, maxPosition.y - minPosition.y);
        panel.transform.SetParent(mCanvas);
        return panel;
    }

    /*
    Instantiate 된 object여야 함.
    이미 생성한 object의 위치만 잡아주는 기능  
    */
    public void CreateScrollViewItems(List<GameObject> objects, 
                                float margin, 
                                LoaderButtonOnClickCallBack onClickCallBack,
                                GameObject scrollview, 
                                bool isHorizon= true, 
                                string viewportName = "Viewport",
                                string contentName = "Content")
    {
        int count = objects.Count;
        if(count == 0)
            return;

        Vector2 scrollviewSize = scrollview.GetComponent<RectTransform>().sizeDelta;

        GameObject content = scrollview.transform.Find(viewportName).transform.Find(contentName).gameObject;
       
        Vector2 objectSize = objects[0].GetComponent<RectTransform>().sizeDelta;

        if(isHorizon)
        {
            objectSize.x += margin;
            content.GetComponent<RectTransform>().sizeDelta = new Vector2(objectSize.x * count + (margin * 2), scrollviewSize.y);
        }
        else
        {
            objectSize.y += margin;
            content.GetComponent<RectTransform>().sizeDelta = new Vector2(scrollviewSize.x, objectSize.y * count + (margin * 2));
        }
        
        for(int n = 0; n < count; n++)
        {
            Vector3 pos;
            if(isHorizon)
            {
                pos = new Vector3(n * objectSize.x + (objectSize.x / 2) + margin, 0, 0);
            }
            else
            {
                pos = new Vector3(0, n * objectSize.y + (objectSize.y / 2) + margin, 0);
            }

            objects[n].transform.SetParent(content.transform);
            objects[n].transform.position = pos;

            Button btn = objects[n].GetComponentInChildren<Button>();
            if(btn != null)
            {
                GameObject o = objects[n];
                btn.onClick.AddListener(() => {onClickCallBack(o);});
            }
            
        }
    }
    public GameObject CreateByPrefab(string prefab, Transform parent, Vector2 size, Vector3 position)
    {
        GameObject obj = Resources.Load<GameObject>(prefab);
        obj = GameObject.Instantiate(obj, position, Quaternion.identity);
        obj.GetComponent<RectTransform>().sizeDelta = size;
        obj.transform.SetParent(parent);

        return obj;
    }
}