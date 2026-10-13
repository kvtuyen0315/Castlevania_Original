using System;
using UnityEngine;

/// <summary>
/// Bộ animation của 1 nhân vật/nhóm (CVGP_PLAN §5.7): tên anim → khung sprite + thời lượng (tick NesClock).
/// Asset ở Assets/Data/Config/Anim/&lt;Tên&gt;AnimSet.asset, dựng bằng menu CVGP/Build AnimSets từ manifest
/// Tools/cvgp/slices_gameplay.json (KHÔNG sửa tay asset — sửa manifest rồi dựng lại).
/// Quy ước: sprite vẽ quay TRÁI như sheet (quay phải ⇒ flipX). measured=false ⇒ durations là mặc định chờ routine đo.
/// Lớp phủ (overlays — roi Simon): góc trên-trái sprite phủ đặt tại pivot thân + overlayOffsets[i] (px NES, y xuống,
/// khi quay TRÁI); quay phải ⇒ x = −offset.x và flipX (sprite phủ pivot Top-Left lật quanh gốc).
/// </summary>
[CreateAssetMenu(fileName = "AnimSet", menuName = "CVGP/AnimSet")]
public class AnimSet : ScriptableObject
{
    [Serializable]
    public class Anim
    {
        public string name;
        public Sprite[] frames = new Sprite[0];
        public int[] durations = new int[0];
        public bool loop;
        public bool measured;
        public Sprite[] overlays = new Sprite[0];
        public Vector2Int[] overlayOffsets = new Vector2Int[0];
        public bool[] overlayOnTop = new bool[0];
        public string note;

        public int FrameCount => frames.Length;
        public bool HasOverlay => overlays != null && overlays.Length > 0;

        public int TotalTicks
        {
            get
            {
                int t = 0;
                for (int i = 0; i < durations.Length; i++)
                    t += durations[i];
                return t;
            }
        }
    }

    [Serializable]
    public class Palette
    {
        public string name;
        public Color32[] from = new Color32[0];
        public Color32[] to = new Color32[0];
        public string note;
    }

    [SerializeField] private Anim[] anims = new Anim[0];
    [SerializeField] private Palette[] palettes = new Palette[0];

    public int Count => anims.Length;
    public Anim this[int i] => anims[i];

    /// <summary>Tìm anim theo tên; không có ⇒ log lỗi, trả null.</summary>
    public Anim Get(string animName)
    {
        for (int i = 0; i < anims.Length; i++)
            if (anims[i].name == animName)
                return anims[i];
        Debug.LogError(name + ": không có anim '" + animName + "'");
        return null;
    }

    public bool Has(string animName)
    {
        for (int i = 0; i < anims.Length; i++)
            if (anims[i].name == animName)
                return true;
        return false;
    }

    public Palette GetPalette(string paletteName)
    {
        for (int i = 0; i < palettes.Length; i++)
            if (palettes[i].name == paletteName)
                return palettes[i];
        return null;
    }

#if UNITY_EDITOR
    public void SetData(Anim[] newAnims, Palette[] newPalettes)
    {
        anims = newAnims ?? new Anim[0];
        palettes = newPalettes ?? new Palette[0];
    }
#endif
}
