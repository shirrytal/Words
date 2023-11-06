Shader "Custom/Rounded2D"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Radius ("Radius", Range(0, 1)) = 0.1
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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

            fixed4 _Color;
            float _Radius;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
                 fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                uv = uv * 2.0 - 1.0; // Scale and bias UV to be in [-1, 1]
                float maxDist = length(uv);
                //(x - center_x)² + (y - center_y)² < radius²
               
                float2 center = float2(uv.x/2, uv.y/2);
                float radiSquare = pow(_Radius,2);

                float2 bottomLeft = float2(-1.0,-1.0);
                float2 topLeft = float2(-1.0,1.0);
                float2 bottomRight = float2(1.0,-1.0);
                float2 topRight = float2(1.0,1.0);
                float2 bottomLeftCenter = bottomLeft + float2(0.2,0.2);
                float2 topLeftCenter = topLeft + float2(0.2,-0.2);
                float2 bottomRightCenter = bottomRight + float2(-0.2,0.2);
                float2 topRightCenter = topRight - float2(0.2,0.2);

                bool inCircleBottomLeft = pow(uv.x - bottomLeftCenter.x,2) + pow(uv.y - bottomLeftCenter.y,2) < radiSquare;
                bool inCircleBottomRight = pow(uv.x - bottomRightCenter.x,2) + pow(uv.y - bottomRightCenter.y,2) < radiSquare;
                bool inCircleTopLeft = pow(uv.x - topLeftCenter.x,2) + pow(uv.y - topLeftCenter.y,2) < radiSquare;
                bool inCircleTopRight = pow(uv.x - topRightCenter.x,2) + pow(uv.y - topRightCenter.y,2) < radiSquare;


                bool cutOffTopRight = !inCircleTopRight && uv.x >= 0.8 && uv.y >= 0.8;
                bool cutOffTopLeft = !inCircleTopLeft && uv.x <= -0.8 && uv.y >= 0.8;

                bool cutOffBottomRight = !inCircleBottomRight && uv.x >= 0.8 && uv.y <= -0.8;
                bool cutOffBottomLeft = !inCircleBottomLeft && uv.x <= -0.8 && uv.y <= -0.8;


                if(cutOffTopLeft || cutOffTopRight || cutOffBottomRight || cutOffBottomLeft) discard;
                // Return the color
                return _Color;
            }
            ENDCG
        }
    }
}
