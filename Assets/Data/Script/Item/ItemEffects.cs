using System;
using UnityEngine;

/// <summary>
/// Hiệu ứng nhặt item — 1 chỗ duy nhất (CVGP 11). Số liệu: ConfigItem (tim, điểm, hồi máu, thời gian), ConfigScore, ConfigRule;
/// trạng thái ván: GameSession; thế giới: StageWorld (đóng băng, diệt enemy, nháy màn). Luật theo wiki (CV_WIKI_DATA §1.5–1.9, §2.1),
/// thời gian theo video (CVGP_MEMORY §2i). SFX theo định danh routine 02 (item không có SFX đã định danh ⇒ im lặng, §6).
/// </summary>
public static class ItemEffects
{
    /// <summary>Trần tim: HUD 2 chữ số (♥-dd) — SUY LUẬN (wiki không nêu; ConfigSimon.MaxHearts chỉ ghi giá trị lớn nhất đã thấy 76).</summary>
    public const int HEART_CAP = 99;

    /// <summary>Mỗi lần nhặt (harness/HUD): item, chuỗi mô tả hiệu ứng.</summary>
    public static event Action<eItemId, string> Picked;
    /// <summary>Nhặt Magic Crystal — routine 12 (tally) / 14 (kết thúc stage) nghe.</summary>
    public static event Action<StageWorld> CrystalPicked;

    /// <summary>Mô tả hiệu ứng lần nhặt gần nhất (debug/harness).</summary>
    public static string LastLog { get; private set; } = "";

    public static void Apply(StageWorld world, ItemPickup item, SimonController simon)
    {
        Apply(world, item.id, item.X, item.Y, simon);
    }

    /// <summary>Áp hiệu ứng item <paramref name="id"/> nhặt tại (x, y) = pivot item (đáy ô 16 px).</summary>
    public static void Apply(StageWorld world, eItemId id, int x, int y, SimonController simon)
    {
        var cfg = world.Config;
        var s = GameSession.Current;
        var row = cfg != null ? cfg.GetItem(id) : null;
        string log;
        switch (id)
        {
            case eItemId.SmallHeart:
            case eItemId.LargeHeart:
                {
                    int n = row != null && row.hearts.has ? row.hearts.value : 0;
                    int before = s.hearts;
                    s.hearts = Mathf.Min(HEART_CAP, s.hearts + n);
                    Sfx(eSfx.PickupHeart);
                    log = "tim " + before + "→" + s.hearts;
                }
                break;
            case eItemId.RedMoneyBag:
            case eItemId.PurpleMoneyBag:
            case eItemId.WhiteMoneyBag:
            case eItemId.FlashingMoneyBag:
            case eItemId.Crown:
            case eItemId.TreasureChest:
            case eItemId.Moai:
                {
                    int pts = row != null && row.score.has ? row.score.value : 0;
                    log = AddScore(world, pts, x, y, id == eItemId.FlashingMoneyBag);
                    Sfx(eSfx.PickupBag);
                }
                break;
            case eItemId.MorningStar:
                {
                    int before = s.whipLevel;
                    if (simon != null && s.whipLevel < 3)
                    {
                        simon.UpgradeWhip();
                        simon.StartPickupFlash(world.TickCount + 1, cfg.simon.GetInt(eSimonParam.WhipPickupFreezeTicks));
                    }
                    Sfx(eSfx.PickupWeapon);
                    log = "roi " + before + "→" + s.whipLevel + (before < 3 ? " + đóng băng " + cfg.simon.GetInt(eSimonParam.WhipPickupFreezeTicks) + " tick" : " (đã tối đa — không đổi)");
                }
                break;
            case eItemId.Dagger:
            case eItemId.Axe:
            case eItemId.HolyWater:
            case eItemId.Cross:
            case eItemId.Stopwatch:
                {
                    var w = ItemCatalog.SubWeaponOf(id);
                    var before = s.subWeapon;
                    int mult = s.shotMultiplier;
                    if (simon != null)
                        simon.PickupSubWeapon(w);
                    else
                        s.SetSubWeapon(w);
                    Sfx(eSfx.PickupWeapon);
                    log = "vũ khí phụ " + before + "→" + s.subWeapon + ", shot " + mult + "→" + s.shotMultiplier;
                }
                break;
            case eItemId.DoubleShot:
            case eItemId.TripleShot:
                {
                    int target = id == eItemId.DoubleShot ? 2 : 3;
                    // wiki Items/Gameplay: nhặt khi đang cầm Stopwatch (hoặc không có vũ khí phụ) ⇒ 700 điểm thay vì hiệu lực
                    if (s.subWeapon == eSubWeapon.None || s.subWeapon == eSubWeapon.Stopwatch)
                        log = "multiplier khi cầm " + s.subWeapon + " ⇒ " + AddScore(world, cfg.score.GetInt(eScoreParam.MultiplierPickupBonus), x, y, false);
                    else
                    {
                        int before = s.shotMultiplier;
                        s.shotMultiplier = Mathf.Max(s.shotMultiplier, target);
                        s.subWeaponHits = 0;
                        log = "shot " + before + "→" + s.shotMultiplier;
                    }
                    Sfx(eSfx.PickupWeapon);
                }
                break;
            case eItemId.Rosary:
                {
                    int n = world.RosaryClear();
                    world.FlashScreen(cfg.rules.GetInt(eRuleParam.RosaryFlashTicks, 26), cfg.rules.GetInt(eRuleParam.RosaryFlashPeriodTicks, 4));
                    Sfx(eSfx.Rosary);
                    log = "rosary diệt " + n + " thực thể trên màn (0 điểm), nháy " + cfg.rules.GetInt(eRuleParam.RosaryFlashTicks, 26) + " tick";
                }
                break;
            case eItemId.InvisibilityPotion:
                {
                    int ticks = row != null && row.effectTicks.has ? row.effectTicks.value : 0;
                    if (simon != null)
                        simon.StartInvisible(ticks);
                    Sfx(eSfx.PotionInvisible);
                    log = "tàng hình " + ticks + " tick";
                }
                break;
            case eItemId.PotRoast:
                {
                    int heal = row != null && row.healBars.has ? row.healBars.value : 0;
                    int before = s.playerHp;
                    s.playerHp = Mathf.Min(GameSession.MAX_HP, s.playerHp + heal);
                    log = "máu " + before + "→" + s.playerHp + " (+" + heal + ")";
                }
                break;
            case eItemId.OneUp:
                {
                    int before = s.lives;
                    s.lives++;
                    log = "mạng " + before + "→" + s.lives;
                }
                break;
            case eItemId.MagicCrystal:
                {
                    int before = s.playerHp;
                    if (row != null && row.healFull)
                        s.playerHp = GameSession.MAX_HP;
                    log = "quả cầu: máu " + before + "→" + s.playerHp + " ⇒ sự kiện CrystalPicked (tally — routine 12)";
                    if (CrystalPicked != null)
                        CrystalPicked(world);
                }
                break;
            default:
                log = "không có hiệu ứng";
                break;
        }
        s.NotifyChanged();
        LastLog = id + ": " + log;
        if (CvgpDebug.LogState)
            Debug.Log("[Item] t" + world.TickCount + " " + LastLog);
        if (Picked != null)
            Picked(id, log);
    }

    /// <summary>Cộng điểm + chữ điểm nổi tại ô item (góc trên-trái ô = (x − 8, y − 15) với vẽ viewOffset (0,1)).</summary>
    static string AddScore(StageWorld world, int pts, int x, int y, bool flash)
    {
        var s = GameSession.Current;
        int before = s.score;
        GameRules.AddScore(pts);   // CVGP 12: 1UP theo mốc điểm
        if (pts > 0)
            ScorePopup.Spawn(world, pts, x - 8, y - 15, flash);
        return "điểm " + before + "→" + s.score;
    }

    static void Sfx(eSfx sfx)
    {
        if (AudioManager.I != null)
            AudioManager.I.PlaySfx(sfx);
    }
}
