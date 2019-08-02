Shader "Logitech/Circle"
{
    Properties
    {
        _ForegroundColor("Foreground Color", Color) = (1,1,1,1)
        _BackgroundColor("Background Color", Color) = (0,0,0,1)
        [Space(10)]
        _ForegroundCutoff("Foreground Cutoff", Range(0,1)) = 0.5
        _BackgroundCutoff("Background Cutoff", Range(0,1)) = 0.5
        [Space(10)]
        _AntiAliasingBorderSize("Anti Aliasing Border Size", float) = 0.005
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _ForegroundColor;
            fixed4 _BackgroundColor;
            half _ForegroundCutoff;
            half _BackgroundCutoff;
            half _AntiAliasingBorderSize;

            struct appdata
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            v2f vert(appdata IN)
            {
                v2f OUT;
                UNITY_INITIALIZE_OUTPUT(v2f, OUT);
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord - fixed2(0.5, 0.5);
                return OUT;
            }

            float calcAlpha(float distance)
            {
                float pwidth = length(float2(ddx(distance), ddy(distance)));
                float alpha = smoothstep(0.5, 0.5 - pwidth * 1.5, distance);
                return alpha;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed x = IN.texcoord.x * 2;
                fixed y = IN.texcoord.y * 2;

                float distance = length(IN.texcoord);
                float radius = 1 - sqrt(x * x + y * y);
                float col;

                // TODO fix incorrect colors using better anti aliasing method
                if (radius > _ForegroundCutoff)
                {
                    if (_ForegroundCutoff > 0)
                    {
                        float t = smoothstep(radius + _AntiAliasingBorderSize, radius - _AntiAliasingBorderSize, _ForegroundCutoff + _AntiAliasingBorderSize);
                        IN.color = lerp(_BackgroundColor, lerp(_BackgroundColor, _ForegroundColor, _ForegroundColor.a), t);
                        IN.color.a = lerp(_BackgroundColor.a, _ForegroundColor.a, t);
                    }
                    else
                    {
                        IN.color = lerp(_BackgroundColor, _ForegroundColor, _ForegroundColor.a);
                    }
                }
                else
                {
                    IN.color = _BackgroundColor;
                }

                return fixed4(IN.color.rgb, IN.color.a * calcAlpha(distance + (_BackgroundCutoff / 2)));
            }

            ENDCG
        }
    }
}
