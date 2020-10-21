using System;
using UnityEngine;

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
}