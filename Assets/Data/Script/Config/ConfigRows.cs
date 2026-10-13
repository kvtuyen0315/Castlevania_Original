using System;

// Dòng dữ liệu của từng bảng Config (CVGP 04). Tên field = tên cột CSV (importer bắt buộc đủ cột, không thừa cột).
// Cột "source" = nguồn của cả dòng; cột "src_xxx" = nguồn riêng cho các ô gắn [CfgSrc("src_xxx")] hoặc field "xxx".

/// <summary>ConfigEnemy.csv — 15 enemy thường + 3 thực thể phụ. Sát thương lên Simon KHÔNG ở đây: theo stage (ConfigBlock).</summary>
[Serializable]
public class ConfigEnemyRow
{
    public eEnemyId id;
    public string nameEn;
    public string nameAlt;
    public string sheetName;
    public eEnemyKind kind;
    [CfgIntList] public string stages;
    [CfgSrc("src_hits")] public CfgInt hitsWeak;
    [CfgSrc("src_hits")] public CfgInt hitsStrong;
    public string src_hits;
    public CfgInt score;
    public string src_score;
    public bool canDie;
    public string projectile;
    public string dropRule;
    public CfgFloat moveSpeed;
    public string src_moveSpeed;
    public string behaviorShort;
    public string source;
}

/// <summary>ConfigBoss.csv — 1 dòng / (boss, pha).</summary>
[Serializable]
public class ConfigBossRow
{
    public eBossId id;
    public int phase;
    public string nameEn;
    public string sheetName;
    public int stage;
    public int entityCount;
    [CfgSrc("src_hits")] public CfgInt hitsWeak;
    [CfgSrc("src_hits")] public CfgInt hitsStrong;
    public eHitsScope hitsScope;
    public string src_hits;
    public CfgInt score;
    public string src_score;
    public bool stopwatchAffects;
    public string weakPoint;
    public string attacks;
    public string itemsInRoom;
    public CfgFloat moveSpeed;
    public string src_moveSpeed;
    public string source;
}

/// <summary>ConfigActorParam.csv — tham số hành vi riêng từng thực thể (actor = Enemy.X / Boss.X / SubWeapon.X / Item.X).</summary>
[Serializable]
public class ConfigActorParamRow
{
    public string actor;
    public string key;
    public CfgFloat value;
    public CfgFloat min;
    public CfgFloat max;
    public string unit;
    public string source;
    public string note;
}

/// <summary>ConfigItem.csv — 22 item + 3 vật chứa.</summary>
[Serializable]
public class ConfigItemRow
{
    public eItemId id;
    public string nameEn;
    public string nameAlt;
    public eItemKind kind;
    public string sheetGroup;
    public CfgInt hearts;
    public CfgInt score;
    public string src_score;
    public CfgInt healBars;
    public bool healFull;
    public CfgInt effectTicks;
    public string src_effectTicks;
    public CfgInt lifetimeTicks;
    public string src_lifetimeTicks;
    public string fallStyle;
    public string effectShort;
    public string source;
}

/// <summary>ConfigSubWeapon.csv — 5 vũ khí phụ (id dùng lại eSubWeapon của CVTI).</summary>
[Serializable]
public class ConfigSubWeaponRow
{
    public eSubWeapon id;
    public string nameEn;
    public eItemId itemId;
    public int heartCost;
    public eHitStrength strength;
    public CfgInt pierces;
    public string src_pierces;
    public bool returns;
    public CfgInt freezeTicks;
    public string src_freezeTicks;
    [CfgSrc("src_motion")] public CfgFloat speedX;
    [CfgSrc("src_motion"), CfgAllowNegative] public CfgFloat speedY0;
    [CfgSrc("src_motion")] public CfgFloat gravity;
    public string src_motion;
    public string behaviorShort;
    public string source;
}

/// <summary>ConfigWhip.csv — roi cấp 1/2/3.</summary>
[Serializable]
public class ConfigWhipRow
{
    public int level;
    public string nameEn;
    public eHitStrength strength;
    [CfgSrc("src_damage")] public CfgFloat damageVsLeather;
    public string src_damage;
    public CfgInt spriteLenPx;
    public string src_spriteLenPx;
    public CfgInt reachPx;
    public string src_reachPx;
    public CfgInt upgradeHearts;
    public string src_upgradeHearts;
    /// <summary>Hộp đánh (CVGP 09): bbox pixel đục của mảnh roi duỗi, tính từ góc trên-trái mảnh (overlayOffsets khung 2).</summary>
    [CfgSrc("src_hit")] public CfgInt hitX;
    [CfgSrc("src_hit")] public CfgInt hitY;
    [CfgSrc("src_hit")] public CfgInt hitW;
    [CfgSrc("src_hit")] public CfgInt hitH;
    public string src_hit;
    public int colorCount;
    public string source;
}

/// <summary>ConfigBlock.csv — 6 block (3 stage/block): giờ, boss, bậc sát thương Simon nhận.</summary>
[Serializable]
public class ConfigBlockRow
{
    public int block;
    [CfgIntList] public string stages;
    public eBossId boss;
    public CfgInt timeLimit;
    public string src_timeLimit;
    public int damageToSimon;
    public string src_damage;
    public string bossMusic;
    public string src_music;
    public string descriptionShort;
    public string source;
}

/// <summary>ConfigStage.csv — 18 stage: khu vực, enemy, boss, nhạc.</summary>
[Serializable]
public class ConfigStageRow
{
    public int stage;
    public int block;
    public string areaShort;
    [CfgEnumList(typeof(eEnemyId))] public string enemies;
    public eBossId boss;
    public string music;
    public string src_music;
    public string source;
}

/// <summary>ConfigDrop.csv — nội dung từng vật chứa đặt trên map (khung trống; routine 11/15 điền).</summary>
[Serializable]
public class ConfigDropRow
{
    public string objectId;
    public int stage;
    public eItemId container;
    public eItemId item;
    public string note;
    public string source;
}
