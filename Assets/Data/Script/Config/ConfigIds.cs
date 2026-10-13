// Id của hệ Config (CVGP 04). Tên theo wiki StrategyWiki (tên nhóm sheet ghi ở cột sheetName của CSV).
// Thêm giá trị mới ⇒ thêm dòng CSV tương ứng (importer báo thiếu). Không đổi số đã có (asset lưu theo số).

/// <summary>15 enemy thường (thứ tự E01–E15 của CV_WIKI_DATA §3.1) + 3 thực thể phụ.</summary>
public enum eEnemyId
{
    None = 0,
    Ghoul = 1,
    Bat = 2,
    Panther = 3,
    Raven = 4,
    Eagle = 5,
    Fishman = 6,
    MedusaHead = 7,
    Skeleton = 8,
    Ghost = 9,
    RedSkeleton = 10,
    SpearKnight = 11,
    BoneTower = 12,
    Fleaman = 13,
    BoneDragon = 14,
    AxeKnight = 15,

    // Thực thể phụ (không nằm trong 15 loại của wiki Enemies)
    PhantomBatMinor = 16, // Phantom Bat bản thường, Stage 16
    Igor = 17,            // đi cùng Frankenstein, bất tử
    Snake = 18,           // rắn của Queen Medusa
}

/// <summary>6 boss (B1–B6 của CV_WIKI_DATA §4.1); Dracula có 2 pha = 2 dòng CSV.</summary>
public enum eBossId
{
    None = 0,
    PhantomBat = 1,
    QueenMedusa = 2,
    MummyMan = 3,
    Frankenstein = 4,
    Death = 5,
    Dracula = 6,
}

/// <summary>22 item (I01–I22 của CV_WIKI_DATA §2.1) + 3 vật chứa (C01–C03, từ 101).</summary>
public enum eItemId
{
    None = 0,
    SmallHeart = 1,
    LargeHeart = 2,
    RedMoneyBag = 3,
    PurpleMoneyBag = 4,
    WhiteMoneyBag = 5,
    FlashingMoneyBag = 6,
    Crown = 7,
    TreasureChest = 8,
    Moai = 9,
    MorningStar = 10,
    Dagger = 11,
    Axe = 12,
    HolyWater = 13,
    Cross = 14,
    Stopwatch = 15,
    DoubleShot = 16,
    TripleShot = 17,
    Rosary = 18,
    InvisibilityPotion = 19,
    PotRoast = 20,
    OneUp = 21,
    MagicCrystal = 22,

    Torch = 101,
    WallCandle = 102,
    BreakableBlock = 103,
}

/// <summary>Khoá bảng ConfigSimon.csv.</summary>
public enum eSimonParam
{
    MaxHp,
    StartLives,
    StartHearts,
    HeartsOnNewBlock,
    MaxHearts,
    WalkSpeed,
    JumpAirTicks,
    JumpHeight,
    JumpRiseSpeedAvg,
    JumpRiseTicks,
    JumpApexTicks,
    JumpFallSpeed,
    JumpDistance,
    JumpAirControl,
    StairSpeedX,
    StairSpeedY,
    StairKnockOff,
    KnockbackDx,
    KnockbackDy,
    KnockbackAirTicks,
    InvincibleTicks,
    WhipTotalTicks,
    WhipWindupTicks,
    WhipRaiseTicks,
    WhipExtendTicks,
    WhipHeightAboveFeet,
    WhipPickupFreezeTicks,
    HurtboxWidth,
    HurtboxHeight,
    CrouchHurtboxHeight,
    // CVGP 08 — bảng nhảy (px đi LÊN ở tick 1..16; rơi = đối xứng) + tư thế, rơi, ngồi, cầu thang, knockback, chết
    JumpRise01,
    JumpRise02,
    JumpRise03,
    JumpRise04,
    JumpRise05,
    JumpRise06,
    JumpRise07,
    JumpRise08,
    JumpRise09,
    JumpRise10,
    JumpRise11,
    JumpRise12,
    JumpRise13,
    JumpRise14,
    JumpRise15,
    JumpRise16,
    JumpTuckStartTick,
    JumpUntuckTick,
    JumpTuckDrawY,
    CrouchDrawY,
    FallSpeed,
    LandCrouchMinFallTicks,
    LandCrouchTicks,
    WalkFrameTicks,
    StairFrameTicks,
    StairStepPx,
    StairEnterRange,
    StairLineOffsetUpRight,
    StairLineOffsetUpLeft,
    StairWalkPoseDrawY,
    StairDownDrawBack,
    KnockbackVy0,
    KnockbackGravity,
    KnockbackDxTicks,
    InvincibleBlinkPeriod,
    DeathKneelTicks,
    DeathTicks,
}

/// <summary>Khoá bảng ConfigScore.csv.</summary>
public enum eScoreParam
{
    OneUpFirst,
    OneUpSecond,
    OneUpEvery,
    TallyPointsPerTimeUnit,
    TallyPointsPerHeart,
    TallyTicksPerTimeUnit,
    TallyTicksPerHeart,
    TallyGapTicks,
    MultiKill2,
    MultiKill3,
    MultiKill4,
    MultiplierPickupBonus,
    RosaryKillPoints,
    EnemyProjectilePoints,
}

/// <summary>Khoá bảng ConfigRule.csv (luật chung, HUD, chuyển cảnh).</summary>
public enum eRuleParam
{
    TimeUnitTicks,
    PlayerBarSegments,
    EnemyBarSegments,
    DamageDisplayTicksPerBar,
    HealDisplayTicksPerBar,
    DamageSecondQuest,
    SubWeaponOnScreenBase,
    DoubleShotOnScreen,
    TripleShotOnScreen,
    MultiplierHitsToDrop,
    WhipLevelOnDeath,
    LoseSubWeaponOnDeath,
    ContinueAtStageStart,
    PitWaterSpikeInstantKill,
    StopwatchItemsStillExpire,
    FrozenEnemiesStillHurt,
    RosaryFlashTicks,
    CameraFollowScreenX,
    DoorScrollPx,
    DoorScrollSpeed,
    DoorPauseTicks,
    DoorAutoWalkPx,
    StageClearToTallyTicks,
}
