// CVGP 11 — Rosary: màu nền đen của CẢ màn NES (sân chơi + HUD) đổi sang _FlashColor (video Map_1 f2252–2264, f4883–4896).
// Dùng trong OnRenderImage (ScreenFlash) chỉ khi đang nháy; pixel tối (max kênh < _Threshold) ⇒ _FlashColor.
Shader "Hidden/CVGP/NesScreenFlash"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
        _FlashColor ("Flash Color", Color) = (0.66,0.66,0.66,1)
        _Threshold ("Black Threshold", Float) = 0.04
    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
        CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _FlashColor;
            float _Threshold;

            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv);
                if (max(c.r, max(c.g, c.b)) < _Threshold)
                    c.rgb = _FlashColor.rgb;
                return c;
            }
        ENDCG
        }
    }
}
