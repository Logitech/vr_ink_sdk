Shader "Logitech/Highlight"
{
    Properties
    {
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth("Outline Width", float) = 1.05
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    struct appdata
    {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
    };

    struct v2f
    {
        float4 pos : POSITION;
        float4 color : COLOR;
        float3 normal : NORMAL;
    };

    float4 _OutlineColor;
    float _OutlineWidth;

    v2f vert(appdata v)
    {
        v.vertex.xyz *= _OutlineWidth;

        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.color = _OutlineColor;
        o.normal = v.normal;
        return o;
    }

    ENDCG

    SubShader
    {
        Tags { "Queue" = "Transparent" }

        // Render Outline in stencil
        Pass
        {
            Stencil {
                Ref 1
                Comp always
                Pass replace
            }

            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            half4 frag(v2f i) : COLOR
            {
                return i.color;
            }
            ENDCG
        }

        // Normal Render
        Pass
        {
            Stencil {
                Ref 1
                Comp Always
                Pass DecrSat
            }
        }

        // Render Outline
        Pass
        {
            Stencil {
                Ref 1
                Comp Equal
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            half4 frag(v2f i) : COLOR
            {
                return i.color;
            }
            ENDCG
        }
    }
}
