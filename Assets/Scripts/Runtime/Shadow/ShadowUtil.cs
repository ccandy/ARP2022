using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public static class ShadowUtil
{
   public static int GetSplit(int tileCount)
   {
      int dim  = (int)Mathf.Ceil(Mathf.Sqrt(tileCount));
      int size = Mathf.NextPowerOfTwo(dim);
      
      return size;
   }

   public static Matrix4x4 GetWorldToShadowMatrix(Matrix4x4 viewMat, Matrix4x4 projMat, int split,
      Vector2 offset)
   {
      Matrix4x4 m = viewMat * projMat;
      
      if (SystemInfo.usesReversedZBuffer) {
         m.m20 = -m.m20;
         m.m21 = -m.m21;
         m.m22 = -m.m22;
         m.m23 = -m.m23;
      }
      float scale = 1f / split;
      m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
      m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
      m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
      m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
      m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
      m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
      m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
      m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
      m.m20 = 0.5f * (m.m20 + m.m30);
      m.m21 = 0.5f * (m.m21 + m.m31);
      m.m22 = 0.5f * (m.m22 + m.m32);
      m.m23 = 0.5f * (m.m23 + m.m33);
      return m;
   }

   public static Vector2 GetViewOffset(int index, int split)
   {
      return new Vector2(index % split, index / split);
   }

   public static void SetViewPort(ref ScriptableRenderContext context, CommandBuffer cmd,Vector2 offset, float tileSize)
   {
      cmd.SetViewport(new Rect(
         offset.x * tileSize, offset.y * tileSize, tileSize, tileSize
      ));
      
      ExecuteCommandBuffer(ref context, cmd);
   }

   public static void ExecuteCommandBuffer(ref ScriptableRenderContext context , CommandBuffer cmd)
   {
      context.ExecuteCommandBuffer(cmd);
      cmd.Clear();
   }

   public static void SetShadowBias(ref ScriptableRenderContext context, CommandBuffer cmd, float bias)
   {
      cmd.SetGlobalDepthBias(0f,bias);
      ExecuteCommandBuffer(ref context, cmd);
   }
   
}
