using System;
using UnityEngine;

/// <summary>
/// Chữ điểm nổi khi nhặt túi tiền / bonus (CVGP 11) — nhóm Effect, đứng yên tại góc trên-trái ô item (video Map_1: "700" f1339 trùng
/// ô túi), tồn tại ScorePopupTicks (36 — 3 mẫu video) / ScorePopupFlashTicks (48 — túi 1000). Ghép 2 mảnh sprite sheet
/// (ObjectAnimSet "score_parts": 10, 20, 40, 70, 0, 00 — CVGP 05): 100/200/400/700 = [x0][0], 1000/2000/4000/7000 = [x0][00].
/// Chữ 1000 trong video đổi màu theo túi — CHƯA làm (sheet chỉ có 1 màu; §6).
/// </summary>
public class ScorePopup : Entity
{
    [SerializeField] private SpriteRenderer second;

    [NonSerialized] public int points, age, life;

    public override string DebugState => points + " t" + age;

    protected override void OnSpawned()
    {
        points = age = life = 0;
        Facing = -1;
    }

    /// <summary>Sinh chữ điểm (góc trên-trái tại (x, y) px section). Điểm không ghép được ⇒ không vẽ (vẫn cộng điểm ở nơi gọi).</summary>
    public static ScorePopup Spawn(StageWorld world, int pts, int x, int y, bool flash)
    {
        var cat = world.Items;
        if (cat == null || cat.scorePrefab == null)
            return null;
        int lead, tail;
        if (!Pieces(pts, out lead, out tail))
            return null;
        var p = world.Spawn(cat.scorePrefab, x, y, -1) as ScorePopup;
        if (p == null)
            return null;
        var cfg = world.Config;
        p.points = pts;
        p.life = cfg != null ? cfg.rules.GetInt(flash ? eRuleParam.ScorePopupFlashTicks : eRuleParam.ScorePopupTicks, 36) : 36;
        var a = p.animSet != null ? p.animSet.Get("score_parts") : null;
        if (a != null && p.view != null)
        {
            p.view.sprite = a.frames[lead];
            p.view.enabled = true;
            if (p.second != null)
            {
                p.second.sprite = a.frames[tail];
                p.second.enabled = true;
                p.second.transform.localPosition = NesCoord.ToWorld(8, 0) - NesCoord.ToWorld(0, 0);
            }
        }
        p.SyncView();
        return p;
    }

    /// <summary>Chỉ số khung trong "score_parts": 0 = "10", 1 = "20", 2 = "40", 3 = "70", 4 = "0", 5 = "00".</summary>
    public static bool Pieces(int pts, out int lead, out int tail)
    {
        lead = tail = -1;
        int head = pts % 1000 == 0 ? pts / 1000 : pts / 100;
        tail = pts % 1000 == 0 ? 5 : 4;
        if (pts <= 0 || (pts % 1000 != 0 && pts % 100 != 0) || pts > 7000)
            return false;
        switch (head)
        {
            case 1: lead = 0; return true;
            case 2: lead = 1; return true;
            case 4: lead = 2; return true;
            case 7: lead = 3; return true;
            default: return false;
        }
    }

    public override void OnTick()
    {
        age++;
        if (age >= life)
            Kill();
    }

    public override void SyncView()
    {
        transform.localPosition = NesCoord.ToWorld(X + viewOffset.x, Y + viewOffset.y);
    }
}
