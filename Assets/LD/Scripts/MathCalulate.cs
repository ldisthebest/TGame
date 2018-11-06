
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
            if (elem[min] > target && min != 0) //特殊情况
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
        return HalfFind(elem, 0, elem.Length-1, target);
    }


}



