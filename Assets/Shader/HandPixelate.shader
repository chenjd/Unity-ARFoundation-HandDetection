Shader "Unlit/HandPixelate"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DetectionTex ("DetectionTexture", 2D) = "white" {}

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _DetectionTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 PixelateOperation(sampler2D tex, half2 uv, half scale, half ratio)
			{
				half PixelSize = 1.0 / scale;
				half coordX=PixelSize * ceil(uv.x / PixelSize);
				half coordY = (ratio * PixelSize)* ceil(uv.y / PixelSize / ratio);
				half2 coord = half2(coordX,coordY);
				return half4(tex2D(tex, coord).xyzw);
			}

            fixed4 frag (v2f i) : SV_Target
            {
                float2 dUV = i.uv;
                dUV.y = 1 - dUV.y;
                fixed4 detectedCol = tex2D(_DetectionTex, dUV);
                if(detectedCol.r == 1)
                {
                    return PixelateOperation(_MainTex, i.uv, 60, 1);
                }
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
