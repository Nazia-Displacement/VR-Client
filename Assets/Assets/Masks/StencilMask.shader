Shader "Custom/StencilMask"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        ColorMask 0 // Mask color if you don’t want it visible
        Stencil
        {
            Ref 1
            Comp Always
            Pass Replace
        }
        Pass {}
    }
}