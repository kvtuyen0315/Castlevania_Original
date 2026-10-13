// CVGP 11 — sprite đổi bảng màu NES (Simon nháy khi nhặt Morning Star, lóe màu khi tàng hình).
// Giống Sprites/Default (UnitySprites.cginc: lật, màu đỉnh, pixel snap) + thay đúng 3 màu _FromN → _ToN khi _Swap = 1.
// Sprite Point/không nén (NesSpriteImporter) ⇒ màu texel khớp chính xác; ngưỡng 0.02 chỉ để chống sai số lấy mẫu.
Shader "CVGP/NesPaletteSwap"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
        _From0 ("From 0", Color) = (0,0,0,1)
        _From1 ("From 1", Color) = (0,0,0,1)
        _From2 ("From 2", Color) = (0,0,0,1)
        _To0 ("To 0", Color) = (0,0,0,1)
        _To1 ("To 1", Color) = (0,0,0,1)
        _To2 ("To 2", Color) = (0,0,0,1)
        _Swap ("Swap", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment PaletteFrag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            fixed4 _From0, _From1, _From2, _To0, _To1, _To2;
            float _Swap;

            fixed3 SwapColor(fixed3 c)
            {
                if (_Swap < 0.5)
                    return c;
                if (distance(c, _From0.rgb) < 0.02) return _To0.rgb;
                if (distance(c, _From1.rgb) < 0.02) return _To1.rgb;
                if (distance(c, _From2.rgb) < 0.02) return _To2.rgb;
                return c;
            }

            fixed4 PaletteFrag(v2f IN) : SV_Target
            {
                fixed4 c = SampleSpriteTexture(IN.texcoord);
                c.rgb = SwapColor(c.rgb);
                c *= IN.color;
                c.rgb *= c.a;
                return c;
            }
        ENDCG
        }
    }
}
