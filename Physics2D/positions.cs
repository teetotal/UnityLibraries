using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Positions
{
    public Vector2 GetWorldPointMin(float marginX = 0.0f, float marginY = 0.0f, bool isPerspective = false)
    {
        Vector2 p;
        if (isPerspective)
        {
            Rect safeArea = Screen.safeArea;
            return new Vector2(safeArea.position.x + marginX, safeArea.position.y + marginY);
            //p = Camera.main.ScreenToWorldPoint(new Vector3(safeArea.position.x, Camera.main.pixelHeight - safeArea.position.y, Camera.main.nearClipPlane));
        }
        else
        {
            Rect safeArea = Screen.safeArea;
            float x = (safeArea.position.x) / Screen.width;
            float y = (safeArea.position.y) / Screen.height;
            p = Camera.main.ViewportToWorldPoint(new Vector2(x, y));            
        }
        return new Vector2(p.x + marginX, p.y + marginY);
    }

    public Vector2 GetWorldPointMax(float marginX = 0.0f, float marginY = 0.0f, bool isPerspective = false)
    {
        Vector2 p;
        if (isPerspective)
        {
            Rect safeArea = Screen.safeArea;
            return new Vector2(safeArea.width + safeArea.position.x - marginX, safeArea.height + safeArea.position.y - marginY);
            //p = Camera.main.ScreenToWorldPoint(new Vector3(safeArea.width + safeArea.position.x, Camera.main.pixelHeight - safeArea.height + safeArea.position.y, Camera.main.nearClipPlane));
        }
        else
        {
            Rect safeArea = Screen.safeArea;
            float x = (safeArea.width + safeArea.position.x) / Screen.width;
            float y = (safeArea.height + safeArea.position.y) / Screen.height;

            p = Camera.main.ViewportToWorldPoint(new Vector2(x, y));
        }
           
        return new Vector2(p.x - marginX, p.y - marginY);
    }

    public Vector2 GetWorldPointSize(float marginX = 0.0f, float marginY = 0.0f)
    {
        Vector2 min = GetWorldPointMin(marginX, marginY);
        Vector2 max = GetWorldPointMax(marginX, marginY);
        return GetSize(min, max);
    }
    public Vector2 GetSize(Vector2 min, Vector2 max)
    {
        return new Vector2(Math.Abs(max.x - min.x), Math.Abs(max.y - min.y));
    }
    public void DrawGrid(Vector2 margin, Vector2Int count)
    {
        Vector2 min, max;
        min = new Vector2(0.0f, 0.0f);
        max = new Vector2(0.0f, 0.0f);
        DrawGrid(margin.x, margin.y, count.x, count.y, min, max);
    }
    public void DrawGrid(Vector2 margin, Vector2Int count, Vector2 _min, Vector2 _max, Transform parent)
    {
        DrawGrid(margin.x, margin.y, count.x, count.y, _min, _max, parent);
    }
    protected void DrawGrid(float marginX, float marginY, int countX, int countY, Vector2 _min, Vector2 _max, Transform parent = null)
    {
        const float width = 0.02f;
        const string name = "DrawGrid";
        //material이 있어야 색지정 가능
        //lineRenderer.material = new Material (Shader.Find("Particles/Additive"));
        //lineRenderer.startColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);

        Vector2 min, max;
        if(_min.x != _max.x && _min.y != _max.y)
        {
            min = _min;
            max = _max;
        } 
        else
        {
            min = GetWorldPointMin(marginX, marginY);
            max = GetWorldPointMax(marginX, marginY);
        }

        //Vector2 min = GetWorldPointMin(marginX, marginY);
        //Vector2 max = GetWorldPointMax(marginX, marginY);

        min.x += marginX;
        min.y += marginY;
        max.x -= marginX;
        max.y -= marginY;

        Vector2 size = GetSize(min, max);

        float diffX = size.x / (float)countX;
        float diffY = size.y / (float)countY;

        for(int y = 0; y < countY + 1; y++)
        {
            LineRenderer lineRenderer = new GameObject(name + "_y_" + y.ToString()).AddComponent<LineRenderer>();
            lineRenderer.positionCount = 2; 
            lineRenderer.startWidth = width;

            lineRenderer.SetPosition(0, new Vector3(min.x, min.y + (y * diffY), 0));
            lineRenderer.SetPosition(1, new Vector3(max.x, min.y + (y * diffY), 0));

            if(parent != null)
                lineRenderer.transform.SetParent(parent);
        }

        for(int x = 0; x < countX + 1; x++)
        {
            LineRenderer lineRenderer = new GameObject(name + "_x_" + x.ToString()).AddComponent<LineRenderer>();
            lineRenderer.positionCount = 2; 
            lineRenderer.startWidth = width;

            lineRenderer.SetPosition(0, new Vector3(min.x + (x * diffX), min.y, 0));
            lineRenderer.SetPosition(1, new Vector3(min.x + (x * diffX), max.y, 0));
            //Debug.Log((x * diffX).ToString() + ", " + min.y.ToString() + " - " + (x * diffX).ToString() + ", " + max.y.ToString());
            if(parent != null)
                lineRenderer.transform.SetParent(parent);
        }
    }
    public Vector2 GetGridPoint(int x, int y, Vector2 margin, Vector2Int count)
    {
        Vector2 min = GetWorldPointMin(margin.x, margin.y);
        Vector2 max = GetWorldPointMax(margin.x, margin.y);
        Vector2 size = GetSize(min, max);

        return GetGridPoint(size, min, x, y, margin.x, margin.y, count.x, count.y);
    }
    public Vector2 GetGridPoint(Vector2 size, Vector2 min, int x, int y, float marginX = 0.0f, float marginY = 0.0f, int countX = 5, int countY = 5)
    {        

        float diffX = size.x / (float)countX;
        float diffY = size.y / (float)countY;

        Vector2 pos = new Vector2();
        float xPre = min.x + (x * diffX);
        float xNext = min.x + ((x+1) * diffX);

        float yPre = min.y + (y * diffY);
        float yNext = min.y + ((y+1) * diffY);

        pos.x = xPre + (xNext - xPre) * 0.5f;
        pos.y = yPre + (yNext - yPre) * 0.5f;

        return pos;
    }
    public List<Vector2> GetGridPoints(Vector2 margin, Vector2Int count)
    {
        return GetGridPoints(GetWorldPointMin(margin.x, margin.y), GetWorldPointMax(margin.x, margin.y), margin.x, margin.y, count.x, count.y);
    }
    public List<Vector2> GetGridMinMax(Vector2 margin, Vector2 innerMargin, Vector2Int count, Vector2Int from, Vector2Int to)
    {
        Vector2 gridSize = GetGridSize(margin, innerMargin, count);
        Vector2 fromMin = GetGridPoint(from.x, from.y, margin, count);
        fromMin.x -= gridSize.x / 2.0f;
        fromMin.y -= gridSize.y / 2.0f; 
        Vector2 toMax = GetGridPoint(to.x, to.y, margin, count);
        toMax.x += gridSize.x / 2.0f;
        toMax.y += gridSize.y / 2.0f;

        List<Vector2> list = new List<Vector2>(2);
        list.Add(fromMin);
        list.Add(toMax);

        return list;
    }
    /// <summary>
    /// for perspective
    /// </summary>
    /// <param name="gridSize">전체 grid size</param>
    /// <param name="fullSize">전체 크기</param>
    /// <param name="min"></param>
    /// <param name="margin"></param>
    /// <param name="innerMargin"></param>
    /// <param name="count"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public List<Vector2> GetGridMinMax(Vector2 fullGridSize, Vector2 fullSize, Vector2 min, Vector2 margin, Vector2 innerMargin, Vector2Int count, Vector2Int from, Vector2Int to)
    {
        Vector2 fromMin = GetGridPoint(fullSize, min, from.x, from.y, margin.x, margin.y, count.x, count.y);
        fromMin.x -= fullGridSize.x / 2.0f;
        fromMin.y -= fullGridSize.y / 2.0f;
        Vector2 toMax = GetGridPoint(fullSize, min, to.x, to.y, margin.x, margin.y, count.x, count.y);
        toMax.x += fullGridSize.x / 2.0f;
        toMax.y += fullGridSize.y / 2.0f;

        List<Vector2> list = new List<Vector2>(2);
        list.Add(fromMin);
        list.Add(toMax);

        return list;
    }
    public List<Vector2> GetGridPoints(Vector2 margin, Vector2Int count, Vector2 min, Vector2 max)
    {
        return GetGridPoints(min, max, margin.x, margin.y, count.x, count.y);
    }
    public List<Vector2> GetGridPoints(Vector2 min, Vector2 max, float marginX = 0.0f, float marginY = 0.0f, int countX = 5, int countY = 5)
    {
        List<Vector2> list = new List<Vector2>(countX * countY);

        //Vector2 min = GetWorldPointMin(marginX, marginY);
        //Vector2 max = GetWorldPointMax(marginX, marginY);

        min.x += marginX;
        min.y += marginY;
        max.x -= marginX;
        max.y -= marginY;

        Vector2 size = GetSize(min, max);

        float diffX = size.x / (float)countX;
        float diffY = size.y / (float)countY;

        for(int y = 0; y < countY; y++)
        {
            for(int x = 0; x < countX; x++)
            {
                Vector2 pos = new Vector2();
                float xPre = min.x + (x * diffX);
                float xNext = min.x + ((x+1) * diffX);

                float yPre = min.y + (y * diffY);
                float yNext = min.y + ((y+1) * diffY);

                pos.x = xPre + (xNext - xPre) * 0.5f;
                pos.y = yPre + (yNext - yPre) * 0.5f;

                list.Add(pos);
            }
        }

        return list;
    }
    public Vector2 GetGridSize(Vector2 min, Vector2 max, Vector2 margin, Vector2 innerMargin, Vector2Int count)
    {
        Vector2 size = GetSize(min, max);
        size.x -= margin.x * 2.0f;
        size.y -= margin.y * 2.0f;
        return GetGridSize(size, innerMargin, count.x, count.y);
    }
    public Vector2 GetGridSize(Vector2 margin, Vector2 innerMargin, Vector2Int count)
    {
        return GetGridSize(GetWorldPointSize(margin.x, margin.y), innerMargin, count.x, count.y);
    }
    public Vector2 GetGridSize(Vector2 fullSize, Vector2 innerMargin, int countX = 5, int countY = 5)
    {
        //Vector2 size = GetWorldPointSize(marginX, marginY);

        float diffX = fullSize.x / (float)countX;
        float diffY = fullSize.y / (float)countY;

        Vector2 diff = new Vector2(diffX - (innerMargin.x * 2.0f), diffY - (innerMargin.y * 2.0f));

        return diff;
    }

    public Vector3 GetGameObjectSize(GameObject obj)
    {
        return obj.GetComponent<SpriteRenderer>().bounds.size;
    }

    public void SetGameObjectSize(ref GameObject obj, float width, float height, bool isSameRatio = false, bool fixToLager = false)
    {
        Vector3 size = GetGameObjectSize(obj);
        Vector3 scale = obj.transform.localScale;

        float scaleX = width * scale.x / size.x;
        float scaleY = height * scale.y / size.y;

        if(isSameRatio)
        {
            if(fixToLager == false)
            {
                if(scaleX < scaleY)
                {
                    scaleY = scaleX;
                }
                else
                {
                    scaleX = scaleY;
                }
            }
            else
            {
                if(scaleX < scaleY)
                {
                    scaleX = scaleY;
                }
                else
                {
                    scaleY = scaleX;
                }
            }
            
        } 

        obj.transform.localScale = new Vector3(scaleX, scaleY, scale.z);
    }
}