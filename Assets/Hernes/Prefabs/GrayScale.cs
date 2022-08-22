using UnityEngine;

using UnityEngine.Rendering;

using UnityEngine.Rendering.HighDefinition;

using System;
using System.Collections.Generic;

[Serializable, VolumeComponentMenu("Post-processing/Custom/GrayScale")]

public sealed class GrayScale : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    

    [Tooltip("Controls the intensity of the effect.")]
    public ColorParameter color = new ColorParameter(Color.black);

    [Tooltip("Controls the initial intensity of the effect.")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(1, 0, 1);

    [Tooltip("Controls the initial intensity of the effect.")]
    public ClampedFloatParameter minFade = new ClampedFloatParameter(0, 0, 2);

    [Tooltip("Controls the initial intensity of the effect.")]
    public ClampedFloatParameter maxFade = new ClampedFloatParameter(0, 0, 2);
    Material m_Material;

    public bool IsActive() => m_Material != null && intensity.value > 0;

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    public override void Setup()

    {

        if (Shader.Find("Hidden/Shader/GrayScale") != null)
            m_Material = new Material(Shader.Find("Hidden/Shader/GrayScale"));
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        
        if (m_Material == null)
        {
            return;
        }
        m_Material.SetFloat("_intensity", intensity.value);
        m_Material.SetFloat("_minFade", minFade.value);
        m_Material.SetFloat("_maxFade", maxFade.value);
        m_Material.SetColor("_Color", color.value);
        m_Material.SetTexture("_InputTexture", source);

        HDUtils.DrawFullScreen(cmd, m_Material, destination);

    }

    public override void Cleanup() => CoreUtils.Destroy(m_Material);

}