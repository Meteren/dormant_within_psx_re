using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomFogFeature : ScriptableRendererFeature
{
    class CustomFogPass : ScriptableRenderPass
    {
        static readonly string k_tag = "CustomFogFeature";
        RenderTargetIdentifier currentTarget;

        Material fogMaterial;
        FogVolumeProfile fogVolumeProfile;

        static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        static readonly int TempTargetId = Shader.PropertyToID("_TempTargetFog");

        public void Setup(in RenderTargetIdentifier currentTarget) => this.currentTarget = currentTarget; 

        public CustomFogPass(Material fogMaterial, FogVolumeProfile fogVolumeProfile)
        {
            this.fogMaterial = fogMaterial;
            this.fogVolumeProfile = fogVolumeProfile;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {

            if (fogVolumeProfile == null)
            {
                Debug.Log("Volume Profile is null.");
                return;
            } 

            if (!fogVolumeProfile.IsActive())
            {
                Debug.Log("Volume Profile is not active.");
                return;
            }

            SetMaterial(fogVolumeProfile);

            CommandBuffer cb = CommandBufferPool.Get(k_tag);

            Debug.Log($"Fog Material: {fogMaterial}");

            Render(cb, ref renderingData);

            context.ExecuteCommandBuffer(cb);
            CommandBufferPool.Release(cb);

        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            return;
        }

        private void SetMaterial(FogVolumeProfile fogVolumeProfile)
        {
            fogMaterial.SetColor("_FogColor", fogVolumeProfile.fogColor.value);
            fogMaterial.SetFloat("_FogStart", fogVolumeProfile.fogStart.value);
            fogMaterial.SetFloat("_FogEnd", fogVolumeProfile.fogEnd.value);
            fogMaterial.SetFloat("_FogDistance", fogVolumeProfile.fogDistance.value);
            fogMaterial.SetFloat("_FogDensity", fogVolumeProfile.fogDensity.value);

        }

        void Render(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;
            var source = currentTarget;
            int destination = TempTargetId;

            //getting camera width and height 
            var w = cameraData.camera.scaledPixelWidth;
            var h = cameraData.camera.scaledPixelHeight;

            //setting parameters here 
            cameraData.camera.depthTextureMode = cameraData.camera.depthTextureMode | DepthTextureMode.Depth;
           

            int shaderPass = 0;
            cmd.SetGlobalTexture(MainTexId, source);
            cmd.GetTemporaryRT(destination, w, h, 0, FilterMode.Point, RenderTextureFormat.Default);
            cmd.Blit(source, destination);
            cmd.Blit(destination, source, this.fogMaterial, shaderPass);
        }
    }

    CustomFogPass customFogPass;
    private Material fogMaterial;
    private static readonly string shaderPath = "PostEffect/CustomFogShader";
   
    /// <inheritdoc/>
    public override void Create()
    {
        SetMaterialNull();
        //Debug.Log($"[Create] fogMaterial before assignment: {fogMaterial}");

        var shader = Shader.Find(shaderPath);
        var stack = VolumeManager.instance.stack;
        FogVolumeProfile fogVolumeProfile = stack.GetComponent<FogVolumeProfile>();

        if(shader == null)
        {
            Debug.Log("Shader is null");
            return;
        }

        fogMaterial = CoreUtils.CreateEngineMaterial(shader);
        //Debug.Log($"[Create] fogMaterial before assignment: {fogMaterial}");
        if (fogMaterial == null)
        {
            Debug.Log("Material is empty");
            return;
        }

        customFogPass = new CustomFogPass(fogMaterial, fogVolumeProfile)
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
        };

    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (fogMaterial == null)
        {
            Debug.Log("Material is empty");
            return;
        }
      
        renderer.EnqueuePass(customFogPass);
    }
    private void SetMaterialNull() => fogMaterial = null;

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        customFogPass.Setup(renderer.cameraColorTargetHandle);
    }

}


