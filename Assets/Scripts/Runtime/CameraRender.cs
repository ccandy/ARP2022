using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraRender
{
   ScriptableRenderContext context;
   Camera camera;
   const string bufferName = "CameraRender";
   private CommandBuffer cmd;
   private CullingResults _cullingResults;
   private static ShaderTagId shaderTagId = new ShaderTagId("SRPDefaultUnlit");

   private static ShaderTagId[] unsupportShaderTagId =
   {
      new ShaderTagId("Always"),
      new ShaderTagId("ForwardBase"),
      new ShaderTagId("PrepassBase"),
      new ShaderTagId("Vertex"),
      new ShaderTagId("VertexLM"),
      new ShaderTagId("VertexLMU"),
      new ShaderTagId("VertexLMRGBM"),
   };

   private Material _errMaterial;
   
   public void Render(ScriptableRenderContext context, Camera camera)
   {
      this.context = context;
      this.camera = camera;

      cmd = new CommandBuffer()
      {
         name = bufferName
      };

      PrepareSceneForSceneWindow(this.camera);
      
      if (!Cull())
      {
         return;
      }
      Setup();
      DrawVisibleGeometry(this.camera);
      DrawUnsupportedGeometry(this.camera);
      DrawGizmos();
      Submit();
   }

   bool Cull()
   {
      ScriptableCullingParameters p;

      if (camera.TryGetCullingParameters(out p))
      {
         _cullingResults = context.Cull(ref p);
         return true;
      }
      return false;
   }

   void Setup()
   {
      context.SetupCameraProperties(camera);
      cmd.ClearRenderTarget(true, true, Color.clear);
      BeginSample();
   }
   

   void Submit()
   {
      EndSample();
      context.Submit();
   }

   void DrawVisibleGeometry(Camera camera)
   {
      DrawOpaqueGeometry(camera);
      context.DrawSkybox(camera);
      DrawTransparentGeometry(camera);
   }


   void DrawOpaqueGeometry(Camera camera)
   {
      var sortingSettings = new SortingSettings(camera)
      {
         criteria = SortingCriteria.CommonOpaque
      };
      var drawingSettings = new DrawingSettings(shaderTagId, sortingSettings);
      var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
      context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
   }

   void DrawTransparentGeometry(Camera camera)
   {
      var sortingSettings = new SortingSettings(camera)
      {
         criteria = SortingCriteria.CommonTransparent
      };
      
      var drawingSettings = new DrawingSettings(shaderTagId, sortingSettings);
      var filteringSettings = new FilteringSettings(RenderQueueRange.transparent);
      context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
   }

   void DrawUnsupportedGeometry(Camera camera)
   {

      if (_errMaterial == null)
      {
         _errMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
      }
      
      var sortingSettings = new SortingSettings(camera);
      var filteringSettings = FilteringSettings.defaultValue;
      var drawingSettings = new DrawingSettings(unsupportShaderTagId[0], sortingSettings)
      {
         overrideMaterial = _errMaterial
      };

      for (int i = 0; i < unsupportShaderTagId.Length; i++)
      {
         var shaderTagId = unsupportShaderTagId[i];
         drawingSettings.SetShaderPassName(i,shaderTagId);
      }
      
      context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
   }
   
   void BeginSample()
   {
      cmd.BeginSample(bufferName);
      ExecuteCommandBuffer(cmd);
   }

   void EndSample()
   {
      cmd.EndSample(bufferName);
      ExecuteCommandBuffer(cmd);
   }

   void ExecuteCommandBuffer(CommandBuffer commandBuffer)
   {
      context.ExecuteCommandBuffer(cmd);
      cmd.Clear();
   }

   void DrawGizmos()
   {
      if (Handles.ShouldRenderGizmos())
      {
         context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
         context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
      }
   }

   private void PrepareSceneForSceneWindow(Camera camera)
   {
      if (camera.cameraType == CameraType.SceneView)
      {
         ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
      }
   }
   
}
