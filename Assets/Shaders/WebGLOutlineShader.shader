Shader "Unlit/WebGLOutlineShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
       [PerRendererData] _OutlineColor("Outline Color", Color) = (0, 0, 0, 0)
       [PerRendererData] _OutlineWidth("Outline Width", Range(0.0, 1.0)) = 0.00
           //_OutlineOffset("Outline Offset", Float2) = (0, 0)
    }
        SubShader
       {
           Tags { "Queue" = "Transparent" }
           LOD 100

               Cull Off
           Blend SrcAlpha OneMinusSrcAlpha

           Pass
           {
               CGPROGRAM
               #pragma vertex vert
               #pragma fragment frag
               // make fog work
               #pragma multi_compile_fog

               #include "UnityCG.cginc"

               struct appdata
               {
                   float4 vertex : POSITION;
                   float2 uv : TEXCOORD0;
                   fixed4 color : Color;
               };

               struct v2f
               {
                   float2 uv : TEXCOORD0;
                   float4 vertex : SV_POSITION;
                   fixed4 color : Color;
               };

               sampler2D _MainTex;
               float4 _MainTex_ST;
               fixed4 _OutlineColor;
               float _OutlineWidth;

               v2f vert(appdata v)
               {
                   v2f o;

                   o.vertex = UnityObjectToClipPos(v.vertex);
                   o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                   o.color = v.color;
                   return o;
               }

               fixed4 Outline(fixed4 og, float2 offset)
               {
                   fixed4 offsetCol = tex2D(_MainTex, float2(offset));

                   fixed4 col = offsetCol;
                   col.a -= og.a;
                   col.a = clamp(col.a, 0, 1);

                   col.rgb = _OutlineColor * col.a;

                   return col;
               }

               fixed4 frag(v2f i) : SV_Target
               {
                   // sample the texture
                   fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                   col.rgb *= col.a;

               fixed4 colLeft = tex2D(_MainTex, float2(i.uv.x + _OutlineWidth, i.uv.y));
               fixed4 colRight = tex2D(_MainTex, float2(i.uv.x - _OutlineWidth, i.uv.y));
               fixed4 colUp = tex2D(_MainTex, float2(i.uv.x, i.uv.y + _OutlineWidth));
               fixed4 colDown = tex2D(_MainTex, float2(i.uv.x, i.uv.y - _OutlineWidth));

               fixed4 colUpLeft = tex2D(_MainTex, float2(i.uv.x + _OutlineWidth, i.uv.y + _OutlineWidth));
               fixed4 colUpRight = tex2D(_MainTex, float2(i.uv.x - _OutlineWidth, i.uv.y + _OutlineWidth));
               fixed4 colDownLeft = tex2D(_MainTex, float2(i.uv.x - _OutlineWidth, i.uv.y - _OutlineWidth));
               fixed4 colDownRight = tex2D(_MainTex, float2(i.uv.x + _OutlineWidth, i.uv.y - _OutlineWidth));

               fixed4 merge = colLeft + colRight + colUp + colDown +
                   colUpLeft + colUpRight + colDownLeft + colDownRight;
               merge.a = clamp(merge.a, 0, 1);
               merge.a -= col.a;
               merge.a = clamp(merge.a, 0, 1);

               merge.rgb = _OutlineColor * merge.a;

               return col;// + merge;
               }
               ENDCG
           }
       }
}
