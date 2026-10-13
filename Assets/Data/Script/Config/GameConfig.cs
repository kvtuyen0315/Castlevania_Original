using UnityEngine;

/// <summary>
/// Gốc hệ Config (CVGP 04, asset Assets/Data/Config/GameConfig.asset): giữ tham chiếu mọi bảng.
/// Prefab GameManager giữ tham chiếu serialized (không Resources.Load) và gán Current ở Awake.
/// Số gameplay đọc từ đây (H6); ô "chờ đo" có has = false — xem CHARACTER_STATS.md.
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "CVGP/GameConfig")]
public class GameConfig : ScriptableObject
{
    public static GameConfig Current { get; private set; }

    public ConfigSimon simon;
    public ConfigEnemy enemies;
    public ConfigBoss bosses;
    public ConfigActorParam actorParams;
    public ConfigItem items;
    public ConfigSubWeapon subWeapons;
    public ConfigWhip whips;
    public ConfigBlock blocks;
    public ConfigStage stages;
    public ConfigScore score;
    public ConfigRule rules;
    public ConfigDrop drops;

    public static void SetCurrent(GameConfig config)
    {
        Current = config;
    }

    public ConfigEnemyRow GetEnemy(eEnemyId id) { return enemies.Get(id); }
    public ConfigBossRow GetBoss(eBossId id, int phase = 1) { return bosses.Get(id, phase); }
    public ConfigItemRow GetItem(eItemId id) { return items.Get(id); }
    public ConfigSubWeaponRow GetSubWeapon(eSubWeapon id) { return subWeapons.Get(id); }
    public ConfigWhipRow GetWhip(int level) { return whips.Get(level); }
    public ConfigStageRow GetStage(int stage) { return stages.Get(stage); }
    public ConfigBlockRow GetBlock(int block) { return blocks.Get(block); }

    public ConfigActorParamRow GetParam(eEnemyId id, string key) { return actorParams.Find(ConfigActorParam.ActorKey(id), key); }
    public ConfigActorParamRow GetParam(eBossId id, string key) { return actorParams.Find(ConfigActorParam.ActorKey(id), key); }

    /// <summary>Block chứa stage 1–18 (Second Quest đánh số khác — wiki Secrets: Stage 19 và 20 đều là Stage 1 — chưa hỗ trợ, trả null).</summary>
    public ConfigBlockRow GetBlockOfStage(int stage)
    {
        if (stage < 1 || stage > 18)
            return null;
        var s = GetStage(stage);
        return s != null ? GetBlock(s.block) : null;
    }

    /// <summary>Số vạch Simon mất mỗi lần trúng đòn ở stage này (wiki Gameplay#Damage: theo stage, không theo loại enemy).</summary>
    public int DamageToSimon(int stage)
    {
        if (stage > 18)
            return rules.GetInt(eRuleParam.DamageSecondQuest);
        var b = GetBlockOfStage(stage);
        return b != null ? b.damageToSimon : 0;
    }
}
