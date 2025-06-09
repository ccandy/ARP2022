using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowUtil
{
   public static int GetSplit(int tileCount)
   {
      int dim  = (int)Mathf.Ceil(Mathf.Sqrt(tileCount));
      int size = Mathf.NextPowerOfTwo(dim);
      
      return size;
   }
}
