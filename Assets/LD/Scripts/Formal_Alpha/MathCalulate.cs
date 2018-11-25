using System.Collections.Generic;
using UnityEngine;

public struct Rectangle
{
    public float minX;
    public float minY;
    public float maxX;
    public float maxY;
}

public class MathCalulate
{
    public static float GetHalfValue(float value)
    {
        if(value >= 0)
        {
            return (int)value + 0.5f;
        }
        else
        {
            return (int)value - 0.5f;
        }       
    }

    public static float GetWholeValue(float value)
    {
        if (value >= 0)
        {
            if (value >= (int)value + 0.5f)
            {
                value = (int)value + 1;
            }
            else
            {
                value = (int)value;
            }
        }
        else
        {
            if (value <= (int)value - 0.5f)
            {
                value = (int)value - 1;
            }
            else
            {
                value = (int)value;
            }
        }
        return value;
    }

    public static Vector2 GetHalfVector2(Vector2 pos)
    {
        float x = GetHalfValue(pos.x);
        float y = GetHalfValue(pos.y);
        return new Vector2(x, y);

    }

    public static Vector2 GetNearestWholeUnit(Vector2 pos)
    {
        return new Vector2(GetWholeValue(pos.x), GetWholeValue(pos.y));
        
    }

    public static Rectangle? ConvergenceRectangle(Rectangle rect1, Rectangle rect2)
    {
        float minX = rect1.minX > rect2.minX ? rect1.minX : rect2.minX;
        float minY = rect1.minY > rect2.minY ? rect1.minY : rect2.minY;
        float maxY = rect1.maxY < rect2.maxY ? rect1.maxY : rect2.maxY;
        float maxX = rect1.maxX < rect2.maxX ? rect1.maxX : rect2.maxX;
        if (minX < maxX && minY < maxY)
        {
            Rectangle rect;
            rect.minX = minX;
            rect.minY = minY;
            rect.maxX = maxX;
            rect.maxY = maxY;
            return rect;
        }
        return null;
    }

    public static List<Rectangle> GetColliderSoldierArea(Rectangle rect, Rectangle convergenceRect)
    {
        List<Rectangle> rectSoldiers = new List<Rectangle>();
        Rectangle rectSoldier;
        if (convergenceRect.minX > rect.minX)
        {
            rectSoldier.minX = rect.minX; rectSoldier.maxX = convergenceRect.minX;
            rectSoldier.minY = rect.minY; rectSoldier.maxY = rect.maxY;
            rectSoldiers.Add(rectSoldier);
        }
        if (convergenceRect.maxX < rect.maxX)
        {
            rectSoldier.minX = convergenceRect.maxX; rectSoldier.maxX = rect.maxX;
            rectSoldier.minY = rect.minY; rectSoldier.maxY = rect.maxY;
            rectSoldiers.Add(rectSoldier);
        }
        if (convergenceRect.maxY < rect.maxY)
        {
            rectSoldier.minX = convergenceRect.minX; rectSoldier.maxX = convergenceRect.maxX;
            rectSoldier.minY = convergenceRect.maxY; rectSoldier.maxY = rect.maxY;
            rectSoldiers.Add(rectSoldier);
        }
        if(convergenceRect.minY > rect.minY)
        {
            rectSoldier.minX = convergenceRect.minX; rectSoldier.maxX = convergenceRect.maxX;
            rectSoldier.minY = rect.minY; rectSoldier.maxY = convergenceRect.minY;
            rectSoldiers.Add(rectSoldier);
        }

        return rectSoldiers;
    }

    public static bool ifRectCanIgnore(Rectangle rect)
    {
        if (rect.maxX - rect.minX < 0.3f || rect.maxY - rect.minY < 0.3f)
        {
            return true;
        }
        return false;
    }

    //获得两个矩形在某个方向上的距离向量（用于两个矩形相互靠近即将相交，默认是矩形2靠近矩形1）
    public static Vector2 GetOffoset(Rectangle rect1, Rectangle rect2,Vector2 dir)
    {
        //以下是数学运算
        float distance = (rect2.maxX + rect2.minX) / 2 - (rect1.minX + rect1.maxX) / 2;
        float dx = distance >= 0 ? distance : -distance;
        distance = (rect2.maxY + rect2.minY) / 2 - (rect1.minY + rect1.maxY) / 2;
        float dy = distance >= 0 ? distance : -distance;
        float halfWidth1 = (rect1.maxX - rect1.minX) / 2;
        float halfHeight1 = (rect1.maxY - rect1.minY) / 2;
        float halfWidth2 = (rect2.maxX - rect2.minX) / 2;
        float halfHeight2 = (rect2.maxY - rect2.minY) / 2;

        float judgeX = dx - halfWidth1 - halfWidth2;      
        if (judgeX > 0 && dir.x != 0)
        {
            float dirX = dir.x > 0 ? dir.x : -dir.x;
            float k = judgeX / dirX;
            return dir * k;
        }

        float judgeY = dy - halfHeight1 - halfHeight2;
        if (judgeY > 0 && dir.y != 0)
        {
            float dirY = dir.y > 0 ? dir.y : -dir.y;
            float k = judgeY / dirY;
            return dir * k;
        }

        return Vector2.zero;
    }


}



