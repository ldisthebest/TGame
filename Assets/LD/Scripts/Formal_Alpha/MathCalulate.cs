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

    public static void BubbleSort(float[] elem)
    {
        for(int times = 0; times < elem.Length - 1; times++)
        {
            for (int index = 0; index < elem.Length - times -1; index++)
            {
                if (elem[index] > elem[index + 1])
                {
                    float max = elem[index];
                    elem[index] = elem[index + 1];
                    elem[index + 1] = max;
                }
            }
        }
        
    }

    private static int HalfFind(float[] elem,int min ,int max ,float target)
    {
        if(min == max)
        {
            if (elem[min] > target/* && min != 0*/) //特殊情况
                return min - 1;
            else
                return min;
        }
        int half = (min + max) / 2;
        if(target <= elem[half])
        {
            return HalfFind(elem, min, half, target);
        }
        else
        {
            return HalfFind(elem, half+1, max, target);
        }
    }
    public static int GetInsertIndexOfMin(float[] elem,float target)
    {
        if (elem.Length == 0)
        {
            return -1;
        }
        //折半查找的思想,返回介于两者之间较小的元素的下标
        int result =  HalfFind(elem, 0, elem.Length-1, target);
        //if (result+1<elem.Length && elem[result] == elem[result+1])//重合的话
        //{
        //    result = result + 1;    
        //}
        return result;
    }


    //private static void SortRelatedStepByY(List<FlatBoxStep> elem)
    //{
    //    for (int times = 0; times < elem.Count - 1; times++)
    //    {
    //        for (int index = 0; index < elem.Count - times - 1; index++)
    //        {
    //            if (elem[index].maxY > elem[index + 1].maxY)
    //            {
    //                FlatBoxStep max = elem[index];
    //                elem[index] = elem[index + 1];
    //                elem[index + 1] = max;
    //            }
    //        }
    //    }
    //}
    //public static int GetExactlyLessIndex(List<FlatBoxStep> elem,float target)
    //{
    //    SortRelatedStepByY(elem);
    //    int i = 0;
    //    while(i < elem.Count && target >= elem[i].maxY)
    //    {
    //        i++;
    //    }
    //    return i - 1;
    //}

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

        return rectSoldiers;
    }


}



