using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Positions
{
    public Vector2 GetWorldPointMin(float marginX = 0.0f, float marginY = 0.0f)
    {
        Vector2 p = Camera.main.ViewportToWorldPoint(Vector2.zero);
        return new Vector2(p.x + marginX, p.y + marginY);
    }

    public Vector2 GetWorldPointMax(float marginX = 0.0f, float marginY = 0.0f)
    {
        Vector2 p = Camera.main.ViewportToWorldPoint(new Vector2(1, 1));
        return new Vector2(p.x - marginX, p.y - marginY);
    }

    public Vector2 GetWorldPointSize(float marginX = 0.0f, float marginY = 0.0f)
    {
        Vector2 min = GetWorldPointMin(marginX, marginY);
        Vector2 max = GetWorldPointMax(marginX, marginY);
        return new Vector2(Math.Abs(min.x) + Math.Abs(max.x), Math.Abs(min.y) + Math.Abs(max.y));
    }
    public void DrawGrid(Vector2 margin, Vector2Int count)
    {
        DrawGrid(margin.x, margin.y, count.x, count.y);
    }
    public void DrawGrid(float marginX = 0.0f, float marginY = 0.0f, int countX = 5, int countY = 5)
    {
        const float width = 0.02f;
        const string name = "DrawGrid";
        //material이 있어야 색지정 가능
        //lineRenderer.material = new Material (Shader.Find("Particles/Additive"));
        //lineRenderer.startColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        Vector2 min = GetWorldPointMin(marginX, marginY);
        Vector2 max = GetWorldPointMax(marginX, marginY);
        Vector2 size = GetWorldPointSize(marginX, marginY);

        float diffX = size.x / (float)countX;
        float diffY = size.y / (float)countY;

        for(int y = 0; y < countY + 1; y++)
        {
            LineRenderer lineRenderer = new GameObject(name + "_y_" + y.ToString()).AddComponent<LineRenderer>();
            lineRenderer.positionCount = 2; 
            lineRenderer.startWidth = width;

            lineRenderer.SetPosition(0, new Vector3(min.x, min.y + (y * diffY), 0));
            lineRenderer.SetPosition(1, new Vector3(max.x, min.y + (y * diffY), 0));
        }

        for(int x = 0; x < countX + 1; x++)
        {
            LineRenderer lineRenderer = new GameObject(name + "_x_" + x.ToString()).AddComponent<LineRenderer>();
            lineRenderer.positionCount = 2; 
            lineRenderer.startWidth = width;

            lineRenderer.SetPosition(0, new Vector3(min.x + (x * diffX), min.y, 0));
            lineRenderer.SetPosition(1, new Vector3(min.x + (x * diffX), max.y, 0));
            //Debug.Log((x * diffX).ToString() + ", " + min.y.ToString() + " - " + (x * diffX).ToString() + ", " + max.y.ToString());
        }
    }
    public Vector2 GetGridPoint(int x, int y, Vector2 margin, Vector2Int count)
    {
        return GetGridPoint(x, y, margin.x, margin.y, count.x, count.y);
    }
    public Vector2 GetGridPoint(int x, int y, float marginX = 0.0f, float marginY = 0.0f, int countX = 5, int countY = 5)
    {
        Vector2 min = GetWorldPointMin(marginX, marginY);
        Vector2 max = GetWorldPointMax(marginX, marginY);
        Vector2 size = GetWorldPointSize(marginX, marginY);

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
        return GetGridPoints(margin.x, margin.y, count.x, count.y);
    }
    public List<Vector2> GetGridPoints(float marginX = 0.0f, float marginY = 0.0f, int countX = 5, int countY = 5)
    {
        List<Vector2> list = new List<Vector2>(countX * countY);

        Vector2 min = GetWorldPointMin(marginX, marginY);
        Vector2 max = GetWorldPointMax(marginX, marginY);
        Vector2 size = GetWorldPointSize(marginX, marginY);

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
    public Vector2 GetGridSize(Vector2 margin, Vector2Int count)
    {
        return GetGridSize(margin.x, margin.y, count.x, count.y);
    }
    public Vector2 GetGridSize(float marginX = 0.0f, float marginY = 0.0f, int countX = 5, int countY = 5)
    {
        Vector2 size = GetWorldPointSize(marginX, marginY);

        float diffX = size.x / (float)countX;
        float diffY = size.y / (float)countY;

        Vector2 diff = new Vector2(diffX, diffY);

        return diff;
    }

    public Vector3 GetGameObjectSize(GameObject obj)
    {
        return obj.GetComponent<SpriteRenderer>().bounds.size;
    }

    public void SetGameObjectSize(ref GameObject obj, float width, float height)
    {
        Vector3 size = GetGameObjectSize(obj);
        Vector3 scale = obj.transform.localScale;

        float scaleX = width * scale.x / size.x;
        float scaleY = height * scale.y / size.y;

        obj.transform.localScale = new Vector3(scaleX, scaleY, scale.z);
    }
}