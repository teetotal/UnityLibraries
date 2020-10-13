/*
    Physics Lib.
*/
using System;
public class Point2D
{
    public float mX, mY;    
    public Point2D()
    {

    }
    public Point2D(float x, float y)
    {
        Init(x, y);
    }
    public void Init(float x, float y)
    {
        mX = x;
        mY = y;
    }

    public void Set(float x, float y)
    {
        mX = x;
        mY = y;
    }
    public void Set(Point2D point2D)
    {
        Init(point2D.GetX(), point2D.GetY());
    }
    public void Add(float x, float y)
    {
        mX += x;
        mY += y;
    }

    public float GetX()
    {
        return mX;
    }

    public float GetY()
    {
        return mY;
    }

    public void SetX(float x)
    {
        mX = x;
    }
    public void SetY(float y)
    {
        mY = y;
    }

}

public class Gradient
{
    public float mGradient;
    public bool mUP_DOWN;
    public float mMomentum;
    public void Init(float gradient, bool isUP, float momentum)
    {
        mGradient = gradient;
        mUP_DOWN = isUP;
        mMomentum = momentum;
    }
    public void Set(Gradient g)
    {
        Init(g.mGradient, g.mUP_DOWN, g.mMomentum);
    }

}

public enum SIDE
{
    TOP,
    TOP_LEFT,
    TOP_RIGHT,
    LEFT,
    RIGHT,
    BOTTOM,
    BOTTOM_LEFT,
    BOTTOM_RIGHT,
    MAX
}

public class GradientObject 
{
    private Point2D mPoint;   
    public Gradient mGradient;
    private float mBaseXVal; //한 step당 X의 증가량
    private float mMomentum; //현재 가속도
    private Point2D mMinPosition, mMaxPosition, mSize;
    private Point2D mPrePosition;
    public void Init(float baseX, float x, float y, float gradient, bool isUP, Point2D minPosition, Point2D maxPosition, Point2D size)
    {
        mMomentum = 0.0f;
        mBaseXVal = baseX;
        mPoint = new Point2D();
        mPoint.Init(x, y);
        mGradient = new Gradient();
        mGradient.Init(gradient, isUP, 0);

        mMinPosition = new Point2D();
        mMinPosition.Init(minPosition.GetX(), minPosition.GetY());
        mMaxPosition = new Point2D();
        mMaxPosition.Init(maxPosition.GetX(), maxPosition.GetY());
        mPrePosition = new Point2D(x, y);
        mSize = new Point2D(size.GetX(), size.GetY());
    }

    public void Move(float x, float y)
    {
        mPoint.Set(x, y);
    }

    public Point2D Next(float deltaTime)
    {
        mPrePosition.Set(mPoint);

        float x = 0.0f;
        float y = 0.0f;
        float baseX = mBaseXVal * deltaTime;
        //momentum
        if(mMomentum > 0.0f)
        {
            mMomentum -= mMomentum * deltaTime;
        }
        baseX += baseX * mMomentum;

        if(mGradient.mGradient < 0)
        {
            x += baseX * -1;
        }
        else
        {
            x += baseX;
        }

        if(mGradient.mUP_DOWN) 
        {
            y += baseX * Math.Abs(mGradient.mGradient);
        } else 
        {
            y -= baseX * Math.Abs(mGradient.mGradient);
        }

        mPoint.Add(x, y);
        if(mPoint.GetX() < mMinPosition.GetX())
            mPoint.SetX(mMinPosition.GetX() + (mSize.GetX() * 0.5f)); 
        if(mPoint.GetX() > mMaxPosition.GetX())
            mPoint.SetX(mMaxPosition.GetX() - (mSize.GetX() * 0.5f)); 
        if(mPoint.GetY() < mMinPosition.GetY())
            mPoint.SetY(mMinPosition.GetY() + (mSize.GetY() * 0.5f)); 
        if(mPoint.GetY() > mMaxPosition.GetY())
            mPoint.SetY(mMaxPosition.GetY() - (mSize.GetY() * 0.5f)); 
            
        return mPoint;
    }

    public void Force(Gradient g)
    {        
        mGradient.Set(g);
        mMomentum = g.mMomentum;
    }
    public SIDE GetSideAdjusted(float positionX, float positionY, float sizeX, float sizeY, float contactX, float contactY)
    {
        SIDE side = GetSIDE(positionX, positionY, sizeX, sizeY, contactX, contactY);
        float e = 0.9f;
        float gapX = (mPoint.GetX() - mPrePosition.GetX());
        float gapY = (mPoint.GetY() - mPrePosition.GetY());

        while(side == SIDE.MAX)
        {
            float midX = gapX * e;
            float midY = gapY * e;
            midX = mPrePosition.GetX() + midX;
            midY = mPrePosition.GetY() + midY;

            side = GetSIDE(positionX, positionY, sizeX, sizeY, midX, midY);
            e -= 0.1f;
            //return side;
            if(e < -5.9f) {
                break;
            }
        }
        return side;
    }
    public SIDE GetSIDE(float positionX, float positionY, float sizeX, float sizeY, float contactX, float contactY)
    {
        float minX = positionX - sizeX;
        float minY = positionY - sizeY;

        float maxX = positionX + sizeX;
        float maxY = positionY + sizeY;

        bool top = false, bottom = false, left = false, right = false;
        float marginX = sizeX * 0.5f;
        float marginY = sizeY * 0.5f;
        if(minY <= contactY && minY + marginY >= contactY)
            bottom = true;
        if(maxY >= contactY && maxY - marginY <= contactY)
            top = true;
        if(minX <= contactX && minX + marginX >= contactX)
            left = true;
        if(maxX >= contactX && maxX - marginX <= contactX)
            right = true;

        if(top)
        {
            if(mGradient.mUP_DOWN) //UP이면 top에 맞을 수가 없다
                return SIDE.MAX;

            if(left)
            {
                if(mGradient.mGradient < 0)
                    return SIDE.MAX;
                return SIDE.TOP_LEFT;
            }
            if(right)
            {
                if(mGradient.mGradient > 0)
                    return SIDE.MAX;
                return SIDE.TOP_RIGHT;
            }
            
            return SIDE.TOP;
        }

        if(bottom)
        {
            if(!mGradient.mUP_DOWN) //Down이면 bottom에 맞을 수가 없다
                return SIDE.MAX;

            if(left)
            {
                if(mGradient.mGradient < 0)
                    return SIDE.MAX;
                return SIDE.BOTTOM_LEFT;
            }
            if(right)
            {
                if(mGradient.mGradient > 0)
                    return SIDE.MAX;
                return SIDE.BOTTOM_RIGHT;
            }
            
            return SIDE.BOTTOM;
        }

        if(right)
        {
            if(mGradient.mGradient > 0)
                return SIDE.MAX;

            return SIDE.RIGHT;
        }
        if(left)
        {
            if(mGradient.mGradient < 0)
                return SIDE.MAX;

            return SIDE.LEFT;
        }

        return SIDE.MAX;
    }
}

