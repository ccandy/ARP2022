using System.Collections;
using System.Collections.Generic;
using ARP.Constant;
using UnityEngine;
using UnityEngine.Rendering;

namespace ARP.Render
{   public class DirectionalLightRender : LightRenderInterface
    {
        
        private int directionalLightCount;
        private LightData[] _directionalLightDatas = new LightData[LightConstants.MAX_DIRECTIONAL_LIGHTS];
        public void ConfigurelLightData(VisibleLight visibleLight)
        {
            if (visibleLight == null)
            {
                return;
            }
            
            LightData directionalLightData = new LightData();
            
            directionalLightData.LightAxis                      = -visibleLight.localToWorldMatrix.GetColumn(2);
            directionalLightData.LightColor                     = visibleLight.finalColor;
            directionalLightData.LightAtten                     = 1;
            directionalLightData.RenderLayerMask                = visibleLight.light.renderingLayerMask - 1;
            
            _directionalLightDatas[directionalLightCount]       = directionalLightData;
            directionalLightCount++;
        }
        public void Init()
        {
            directionalLightCount = 0;
        }
        
        public void CleanUp()
        {
            System.Array.Clear(_directionalLightDatas, 0, _directionalLightDatas.Length);
        }

        public void SendToGPU(ref ScriptableRenderContext context, CommandBuffer cmd)
        {

            if (directionalLightCount <= 0)
            {
                return;
            }
            
            Vector4[] dirLightColor = new Vector4[directionalLightCount];
            Vector4[] dirLightDir   = new Vector4[directionalLightCount];
            Vector4[] dirLightData  = new Vector4[directionalLightCount];
            
            for (int i = 0; i < directionalLightCount; ++i)
            {
                dirLightColor[i] = _directionalLightDatas[i].LightColor;
                dirLightDir[i]      = _directionalLightDatas[i].LightAxis;

                Vector4 lightData   = new Vector4();
                lightData.x         = _directionalLightDatas[i].LightAtten;
                lightData.y         = _directionalLightDatas[i].RenderLayerMask;
                
                dirLightData[i]     = lightData;
            }
            
            cmd.SetGlobalVectorArray(LightConstants.DirectionalLightsColorId,dirLightColor);
            cmd.SetGlobalVectorArray(LightConstants.DirectionalLightsDirId, dirLightDir);
            cmd.SetGlobalInt(LightConstants.DirectonalLightAccountId, directionalLightCount);
            cmd.SetGlobalVectorArray(LightConstants.DirectionalLightsDataId, dirLightData);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }

        public void Render(ref ScriptableRenderContext context, ref CullingResults cullingResults,
            ref ShadowGlobalData shadowGlobalData)
        {
            
        }
    }
}
    
