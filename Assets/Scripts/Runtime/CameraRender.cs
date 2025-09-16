using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using ARP.Constant;

namespace ARP.Render
{
   public class CameraRender
   {
      ScriptableRenderContext context;
      Camera _camera;
      const string bufferName = "CameraRender";
      private CommandBuffer cmd;
      private CullingResults _cullingResults;

      private static ShaderTagId[] supportShaderTagId =
      {
         new ShaderTagId("SRPDefaultUnlit"),
         new ShaderTagId("ARPLit"),
      };
      private string camBufferName;
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
      private LightRender _lightRender = new LightRender();
      public void Render(ScriptableRenderContext context, Camera camera, ShadowGlobalData shadowGlobalData)
      {
         this.context = context;
         _camera = camera;

         camBufferName = camera.name + " " + bufferName;

         cmd = new CommandBuffer()
         {
            name = camBufferName
         };

         PrepareSceneForSceneWindow(this._camera);
         float shadowDistance = shadowGlobalData.ShadowDistance;
         if (!Cull(shadowDistance))
         {
            return;
         }

         SetupKeyWords(ref shadowGlobalData);
         _lightRender.SetupLightData(context, ref _cullingResults, ref shadowGlobalData);
         _lightRender.Render(ref context, ref _cullingResults,ref shadowGlobalData);
         
         Setup();
         DrawVisibleGeometry(this._camera);
         DrawUnsupportedGeometry(this._camera);
         DrawGizmos();
         Submit();
      }

      bool Cull(float distance)
      {
         ScriptableCullingParameters p;
        
         
         if (_camera.TryGetCullingParameters(out p))
         {
            p.shadowDistance  = distance;
            _cullingResults   = context.Cull(ref p);
            return true;
         }
         return false;
      }

      private void SetupKeyWords(ref ShadowGlobalData shadowGlobalData)
      {
         string[] softshadowKeyworkds  = ShadowKeywords.DirectionalSoftShadowKeyword;
         int enableSoftKeywords        = (int)shadowGlobalData.FilterMode;
         string enabledSoftKeywords    = softshadowKeyworkds[enableSoftKeywords];
         
         cmd.EnableShaderKeyword(enabledSoftKeywords);
         ExecuteCommandBuffer(cmd);
         
      }
      
      void Setup()
      {
         context.SetupCameraProperties(_camera);
         CameraClearFlags flags = _camera.clearFlags;
         cmd.ClearRenderTarget(flags <= CameraClearFlags.Depth, 
            flags == CameraClearFlags.Color, 
            flags == CameraClearFlags.Color ? _camera.backgroundColor.linear : Color.clear);
         BeginSample(camBufferName);
      }
      

      void Submit()
      {
         EndSample(camBufferName);
         context.Submit();
      }

      void DrawVisibleGeometry(Camera camera)
      {
         var sortingSettings = new SortingSettings(camera);
         var drawingSettings = new DrawingSettings(supportShaderTagId[0], sortingSettings);
         for (int i = 1; i < supportShaderTagId.Length; i++)
         {
            var shaderTagId = supportShaderTagId[i];
            drawingSettings.SetShaderPassName(i,shaderTagId);
         }
         
         DrawOpaqueGeometry(camera, ref drawingSettings);
         context.DrawSkybox(camera);
         DrawTransparentGeometry(camera, ref drawingSettings);
      }


      void DrawOpaqueGeometry(Camera camera, ref DrawingSettings drawingSettings)
      {
         
         
         var sortingSettings = new SortingSettings(camera)
         {
            criteria = SortingCriteria.CommonOpaque
         };
         drawingSettings.sortingSettings = sortingSettings;
         var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
         context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
      }

      void DrawTransparentGeometry(Camera camera,ref DrawingSettings drawingSettings)
      {
         var sortingSettings = new SortingSettings(camera)
         {
            criteria = SortingCriteria.CommonTransparent
         };
         drawingSettings.sortingSettings = sortingSettings;
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

         for (int i = 1; i < unsupportShaderTagId.Length; i++)
         {
            var shaderTagId = unsupportShaderTagId[i];
            drawingSettings.SetShaderPassName(i,shaderTagId);
         }
         
         context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
      }
      
      void BeginSample(string bufferName)
      {
         cmd.BeginSample(bufferName);
         ExecuteCommandBuffer(cmd);
      }

      void EndSample(string bufferName)
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
            context.DrawGizmos(_camera, GizmoSubset.PreImageEffects);
            context.DrawGizmos(_camera, GizmoSubset.PostImageEffects);
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
}
   
