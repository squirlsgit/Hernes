Shader "Hidden/Shader/GrayScale"

{

    HLSLINCLUDE

    #pragma target 4.5

    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"

    struct Attributes

    {

        uint vertexID : SV_VertexID;

        UNITY_VERTEX_INPUT_INSTANCE_ID

    };

    struct Varyings

    {

        float4 positionCS : SV_POSITION;

        float2 texcoord   : TEXCOORD0;

        UNITY_VERTEX_OUTPUT_STEREO

    };

    Varyings Vert(Attributes input)

    {

        Varyings output;

        UNITY_SETUP_INSTANCE_ID(input);

        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);

        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);

        return output;

    }

    // List of properties to control your post process effect

    float _intensity;
    float _maxFade;
    float _minFade;

    float4 _Color;
    TEXTURE2D_X(_InputTexture);

    float4 CustomPostProcess(Varyings input) : SV_Target

    {

        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        // minFade (0, 1) = 0, maxFade (minFade, 1) = 0, Color (any), minRadius (0, 1) = 0

        float minFade = _minFade;
        float maxFade = max(_minFade, _maxFade);
        float _radius = 1 - _intensity;
        uint2 positionSS = input.texcoord * _ScreenSize.xy;
        float3 baseColor = LOAD_TEXTURE2D_X(_InputTexture, positionSS).xyz;
        float3 outColor = _Color.xyz;
        float2 centeredUV = float2(input.texcoord.x - 0.5, input.texcoord.y - 0.5);
        float dist = (centeredUV.x * centeredUV.x + centeredUV.y * centeredUV.y) / 0.5;
        float relative_dist = 0;
        if (_radius == 0) {
            relative_dist = dist;
        } else if (_radius < 1) {
            relative_dist = (dist - _radius) / (1 - _radius);
        }
        float fade = (maxFade - minFade) * relative_dist + minFade;
        // float fade = lerp(minFade, maxFade, dist);
        if (relative_dist > 0) {
            return float4(lerp(baseColor, outColor, fade ), 1);
        } else {
            return float4(baseColor, 1);
        }

        // return float4(baseColor, 1);


        // return float4(lerp(outColor, Luminance(outColor).xxx, _Intensity), 1);

    }

    ENDHLSL

    SubShader

    {

        Pass

        {

            Name "GrayScale"

            ZWrite Off

            ZTest Always

            Blend Off

            Cull Off

            HLSLPROGRAM

                #pragma fragment CustomPostProcess

                #pragma vertex Vert

            ENDHLSL

        }

    }

    Fallback Off

}