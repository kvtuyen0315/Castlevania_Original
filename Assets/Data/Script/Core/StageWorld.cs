using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bộ điều phối gameplay theo tick (CVGP 07, CVGP_PLAN §5.1) — con của màn gameplay (routine 15) / scene Arena.
/// CHỈ StageWorld đăng ký NesClock.OnTick (thực thể không tự đăng ký ⇒ thứ tự tất định, H7). Mỗi tick (Step):
///   (1) NesPad.Tick (đã chốt ở OnSystemTick nếu có GameManager; Arena tự gọi — NesPad chặn gọi 2 lần/tick)
///   (1b) CVM2 06: bệ di động (MovingPlatform.StepPlatform) — đi trước, kéo Simon đang đứng trên theo dx
///   (2–5) OnTick thực thể theo nhóm Player → PlayerWeapon → Enemy → EnemyShot → Item → Prop → Effect, trong nhóm theo thứ tự
///         đăng ký; thực thể sinh trong tick T bắt đầu OnTick từ T+1; sau OnTick kiểm ô chết (checkDeadlyTiles)
///   (6) CombatResolver (bỏ qua khi đóng băng thế giới)
///   (7) camera (NesCameraController.Tick)
///   (8) spawn/despawn theo khung camera (SpawnPoint, EnemySpawner — CVGP 13, despawnOffscreen)
///   dọn thực thể đã Kill (trả pool) → SyncView mọi thực thể → (9) sự kiện TickEnded (HUD)
/// Đóng băng: FreezeWorld(n) (nhặt roi — mọi nhóm, kể cả combat), FreezeEnemies(n) (Stopwatch — Enemy + EnemyShot); có hiệu lực
/// từ tick KẾ TIẾP, kéo dài đúng n tick. Paused = dừng hẳn (TickCount không tăng).
/// RNG: Rng (NesRandom) reset theo rngSeed khi StageWorld bật (đầu stage).
/// </summary>
[DisallowMultipleComponent]
public class StageWorld : MonoBehaviour
{
    public static StageWorld Current { get; private set; }

    /// <summary>Đo GC alloc + thời gian mỗi Step (kiểm hiệu năng) — tắt mặc định.</summary>
    public static bool MeasurePerf;

    [Tooltip("Bảng chỉ số dùng khi GameConfig.Current chưa có (scene thử không có GameManager).")]
    [SerializeField] private GameConfig fallbackConfig;
    [SerializeField] private NesCameraController cameraController;
    [Tooltip("Section chơi lúc bật (con của StageWorld). Null ⇒ section đang bật đầu tiên trong con.")]
    [SerializeField] private LevelSection startSection;
    [Tooltip("Gốc thực thể — cùng chỗ gốc section (NES (0, 32)). Thực thể đặt sẵn là con của gốc này.")]
    [SerializeField] private Transform entityRoot;
    [SerializeField] private int rngSeed = NesRandom.DEFAULT_SEED;
    [Tooltip("Stage đang chơi (sát thương lên Simon theo GameConfig.DamageToSimon).")]
    [SerializeField] private int stage = 1;
    [Tooltip("CVGP 11 — prefab item/FX/vật chứa + vật liệu đổi màu (CVGP/Build Item Prefabs). Có blockPrefab ⇒ tự sinh khối phá được.")]
    [SerializeField] private ItemCatalog itemCatalog;

    private readonly List<Entity>[] groups = new List<Entity>[Entity.GROUP_COUNT];
    private readonly List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
    private readonly List<EnemySpawner> spawners = new List<EnemySpawner>();
    private readonly List<LevelSection> sections = new List<LevelSection>();
    private readonly List<MovingPlatform> platforms = new List<MovingPlatform>();
    private readonly List<Entity> scratch = new List<Entity>();
    private readonly CombatResolver combat = new CombatResolver();
    private readonly EntityPool pool = new EntityPool();
    private bool initialized;
    private int worldFreezeFrom = 1, worldFreezeTo;
    private int enemyFreezeFrom = 1, enemyFreezeTo;
    private int flashFrom = 1, flashTo, flashHalf = 2;
    private ScreenFlash screenFlash;

    public NesRandom Rng { get; } = new NesRandom();
    public LevelSection Section { get; private set; }
    public TileCollision Tiles { get; private set; }
    public NesCameraController CameraCtl => cameraController;
    public CombatResolver Combat => combat;
    public EntityPool Pool => pool;
    public GameConfig Config => GameConfig.Current;
    public ItemCatalog Items { get => itemCatalog; set => itemCatalog = value; }
    public int Stage { get => stage; set => stage = value; }
    /// <summary>CVGP 15: section nạp ở Init (màn chơi đặt theo stage đang chơi trước khi bật — StageScreen).</summary>
    public LevelSection StartSection { get => startSection; set => startSection = value; }
    /// <summary>CVGP 15: dừng mọi bộ sinh enemy (chuỗi cổng/cửa/chuyển tầng — luồng stage).</summary>
    public bool SpawnersPaused { get; set; }
    /// <summary>Số tick đã Step (không tính tick Paused). 0 trước tick đầu.</summary>
    public int TickCount { get; private set; }
    public bool Paused { get; set; }
    public Entity Player { get; private set; }
    public IReadOnlyList<LevelSection> Sections => sections;
    public IReadOnlyList<SpawnPoint> SpawnPoints => spawnPoints;
    /// <summary>CVGP 13: bộ sinh enemy theo vùng của section đang chơi (+ thêm lúc chạy bằng AddSpawner).</summary>
    public IReadOnlyList<EnemySpawner> Spawners => spawners;

    public bool IsWorldFrozen => TickCount >= worldFreezeFrom && TickCount <= worldFreezeTo;
    public bool IsEnemyFrozen => TickCount >= enemyFreezeFrom && TickCount <= enemyFreezeTo;

    /// <summary>1 đòn đã giải (CombatResolver).</summary>
    public event Action<HitInfo> Hit;
    /// <summary>Thực thể chạm ô chết (Water/Pit/Hazard, rơi khỏi đáy).</summary>
    public event Action<Entity, eTileCollision> DeadlyTile;
    /// <summary>CVGP 12 — đầu mỗi tick, TRƯỚC khi xét Paused (StageRules: Start = pause, màn đen hồi sinh).</summary>
    public event Action<StageWorld> TickBegan;
    /// <summary>Bước 9 — cuối mỗi tick (HUD đọc trạng thái ở đây).</summary>
    public event Action<StageWorld> TickEnded;

    // Đo hiệu năng (MeasurePerf)
    public long LastStepAlloc { get; private set; }
    public long TotalStepAlloc { get; private set; }
    public long MaxStepAlloc { get; private set; }
    /// <summary>Số Step có cấp phát &gt; 0 (kể từ ResetPerf).</summary>
    public int AllocSteps { get; private set; }
    public int MeasuredSteps { get; private set; }
    public double MaxStepMs { get; private set; }
    public double LastStepMs { get; private set; }
    public double TotalStepMs { get; private set; }

    private void Awake()
    {
        for (int i = 0; i < groups.Length; i++)
            groups[i] = new List<Entity>(16);
    }

    private void OnEnable()
    {
        Current = this;
        NesClock.OnTick += HandleTick;
    }

    private void OnDisable()
    {
        NesClock.OnTick -= HandleTick;
        Shutdown();
        if (Current == this)
            Current = null;
    }

    private void HandleTick(int frame)
    {
        if (NesPad.I != null)
            NesPad.I.Tick();
        Step();
    }

    public List<Entity> GroupList(int g)
    {
        return groups[g];
    }

    public List<Entity> GroupList(eEntityGroup g)
    {
        return groups[(int)g];
    }

    // ---------------------------------------------------------------- khởi tạo / tắt

    /// <summary>CVGP 15: khởi tạo ngay (màn chơi đặt Simon/camera trước tick đầu) — Step tự gọi nếu chưa.</summary>
    public void EnsureInit()
    {
        if (!initialized)
            Init();
    }

    private void Init()
    {
        initialized = true;
        SpawnersPaused = false;
        CvgpDebug.LoadPrefs();
        if (GameConfig.Current == null && fallbackConfig != null)
            GameConfig.SetCurrent(fallbackConfig);
        ResetRng();
        TickCount = 0;
        worldFreezeFrom = enemyFreezeFrom = 1;
        worldFreezeTo = enemyFreezeTo = 0;
        flashFrom = 1;
        flashTo = 0;
        Paused = false;

        sections.Clear();
        GetComponentsInChildren(true, sections);
        var first = startSection;
        if (first == null)
            for (int i = 0; i < sections.Count && first == null; i++)
                if (sections[i].gameObject.activeSelf)
                    first = sections[i];
        if (cameraController != null)
            cameraController.Init(GameConfig.Current);
        if (first != null)
            LoadSection(first);

        if (entityRoot != null)
        {
            entityRoot.localPosition = NesCoord.ToWorld(0, LevelSectionData.SCREEN_Y);
            scratch.Clear();
            entityRoot.GetComponentsInChildren(false, scratch);
            for (int i = 0; i < scratch.Count; i++)
                if (scratch[i].SourcePrefab == null)
                    Register(scratch[i]);
            scratch.Clear();
        }
        SpawnSectionProps();
        if (cameraController != null)
        {
            cameraController.Target = Player;
            cameraController.SnapToTarget();
        }
        SyncViews();
    }

    private void Shutdown()
    {
        if (!initialized)
            return;
        initialized = false;
        for (int g = 0; g < groups.Length; g++)
        {
            var list = groups[g];
            for (int i = 0; i < list.Count; i++)
            {
                var e = list[i];
                if (e == null)
                    continue;
                e.Kill(eKillReason.Cleared);
                RemoveEntity(e);
            }
            list.Clear();
        }
        Player = null;
        spawnPoints.Clear();
        spawners.Clear();
        SetScreenFlash(false);
        if (Section != null)
            Section.RestoreCells();
        if (cameraController != null)
            cameraController.ResetHome();
    }

    // ---------------------------------------------------------------- section

    /// <summary>Bật section s (tắt các section khác), dựng lưới va chạm, biên camera, danh sách SpawnPoint. Không đụng thực thể.</summary>
    public void LoadSection(LevelSection s)
    {
        for (int i = 0; i < sections.Count; i++)
            if (sections[i] != s && sections[i].gameObject.activeSelf)
                sections[i].gameObject.SetActive(false);
        if (!s.gameObject.activeSelf)
            s.gameObject.SetActive(true);
        Section = s;
        s.PlaceForCamera(0);
        s.RestoreCells();   // lưới va chạm dựng mới ⇒ khối đã phá hiện lại trên Tilemap (CVGP 11)
        Tiles = new TileCollision(s.data);
        Tiles.Platforms = platforms;   // CVM2 06: bệ di động (rỗng ở Block 1 ⇒ va chạm như cũ)
        spawnPoints.Clear();
        s.GetComponentsInChildren(true, spawnPoints);
        for (int i = 0; i < spawnPoints.Count; i++)
            spawnPoints[i].ResetSpawn();
        spawners.Clear();
        s.GetComponentsInChildren(true, spawners);
        for (int i = 0; i < spawners.Count; i++)
            spawners[i].ResetSpawner();
        if (cameraController != null)
            cameraController.Bind(s.data);
    }

    /// <summary>CVGP 19: thêm section dựng lúc chạy (phòng boss thử ở Arena — instance prefab con của StageWorld, đang tắt) để Warp tới.</summary>
    public void AddSection(LevelSection s)
    {
        if (s != null && !sections.Contains(s))
            sections.Add(s);
    }

    /// <summary>CVM2 06: gỡ section dựng lúc chạy trước khi huỷ (harness) — LoadSection duyệt cả danh sách.</summary>
    public void RemoveSection(LevelSection s)
    {
        sections.Remove(s);
    }

    /// <summary>Đổi section: dọn mọi thực thể trừ Player, đặt Player tại (x, y), camera cắt theo Player.</summary>
    public void Warp(LevelSection s, int x, int y)
    {
        ClearNonPlayer();
        LoadSection(s);
        SpawnSectionProps();
        if (Player != null)
            Player.SetPositionPx(x, y);
        if (cameraController != null)
            cameraController.SnapToTarget();
        SyncViews();
    }

    /// <summary>
    /// CVGP 15: cho SpawnPoint của section xét khung camera NGAY (luồng stage nạp section mới ở cuối tick — sau bước 8 — mà khung hình
    /// đó đã phải thấy nến/chậu như video) rồi đặt lại hình. Không đổi Warp (bộ kiểm 07–14 giữ nguyên).
    /// </summary>
    public void RefreshSpawnPoints()
    {
        int cx = cameraController != null ? cameraController.X : 0;
        for (int i = 0; i < spawnPoints.Count; i++)
            spawnPoints[i].UpdateSpawn(this, cx);
        SyncViews();
    }

    /// <summary>Gỡ ngay mọi thực thể trừ nhóm Player (eKillReason.Cleared) và đặt lại mọi SpawnPoint của section (Ready).</summary>
    public void ClearNonPlayer()
    {
        for (int g = 1; g < groups.Length; g++)
        {
            var list = groups[g];
            for (int i = 0; i < list.Count; i++)
                list[i].Kill(eKillReason.Cleared);
        }
        Cleanup();
        for (int i = 0; i < spawnPoints.Count; i++)
            spawnPoints[i].ResetSpawn();
        for (int i = 0; i < spawners.Count; i++)
            spawners[i].ResetSpawner();
    }

    /// <summary>Thêm bộ sinh lúc chạy (harness / màn chơi tự dựng) — gỡ bằng RemoveSpawner; LoadSection dựng lại danh sách từ section.</summary>
    public void AddSpawner(EnemySpawner sp)
    {
        if (sp == null || spawners.Contains(sp))
            return;
        sp.ResetSpawner();
        spawners.Add(sp);
    }

    public void RemoveSpawner(EnemySpawner sp)
    {
        spawners.Remove(sp);
    }

    /// <summary>
    /// Sinh thực thể địa hình của section (CVGP 11): 1 BreakableBlock cho mỗi ô Breakable chưa vỡ (ItemCatalog.blockPrefab).
    /// Init/Warp tự gọi; harness gọi lại sau ClearNonPlayer + LoadSection để đặt lại.
    /// </summary>
    public void SpawnSectionProps()
    {
        if (itemCatalog == null || itemCatalog.blockPrefab == null || Section == null || Tiles == null)
            return;
        var d = Section.data;
        for (int r = 0; r < d.rows; r++)
            for (int c = 0; c < d.cols; c++)
                if (Tiles.CellType(c, r) == eTileCollision.Breakable && !Tiles.IsBroken(c, r))
                {
                    var b = Spawn(itemCatalog.blockPrefab, 0, 0, -1) as BreakableBlock;
                    if (b != null)
                        b.Bind(c, r);
                }
    }

    // ---------------------------------------------------------------- thực thể

    /// <summary>Đưa thực thể (đặt sẵn hoặc vừa lấy pool) vào thế giới. Nhóm Player: cái đầu tiên = Player (camera bám).</summary>
    public void Register(Entity e)
    {
        var list = groups[(int)e.Group];
        if (list.Contains(e))
            return;
        if (e.transform.parent != entityRoot && entityRoot != null)
            e.transform.SetParent(entityRoot, false);
        list.Add(e);
        e.Attach(this, TickCount);
        if (e.Group == eEntityGroup.Player && Player == null)
            Player = e;
    }

    /// <summary>Sinh thực thể từ prefab (pool) tại (x, y) px section.</summary>
    public Entity Spawn(Entity prefab, int x, int y, int facing, SpawnPoint spawner = null)
    {
        var e = pool.Get(prefab, entityRoot);
        e.SetPositionPx(x, y);
        e.Facing = facing >= 0 ? 1 : -1;
        e.vx = e.vy = 0;
        e.Spawner = spawner;
        Register(e);
        e.SyncView();
        return e;
    }

    /// <summary>Kill mọi thực thể nhóm g (cheat / Rosary — routine 11 lọc boss).</summary>
    public int KillGroup(eEntityGroup g, eKillReason reason = eKillReason.Killed)
    {
        int n = 0;
        var list = groups[(int)g];
        for (int i = 0; i < list.Count; i++)
            if (list[i].Alive)
            {
                list[i].Kill(reason);
                n++;
            }
        return n;
    }

    /// <summary>
    /// Rosary (CVGP 11, wiki Items#Rosary): diệt mọi Enemy trên màn trừ boss (IsBoss) + phá mọi đạn enemy/boss trên màn; KHÔNG cho
    /// điểm (Kill thẳng, không qua OnHurt). Trả số thực thể đã gỡ. Video 1:15.0: Zombie biến mất ngay, không lửa.
    /// </summary>
    public int RosaryClear()
    {
        int cx = cameraController != null ? cameraController.X : 0, n = 0;
        for (int g = (int)eEntityGroup.Enemy; g <= (int)eEntityGroup.EnemyShot; g++)
        {
            var list = groups[g];
            for (int i = 0; i < list.Count; i++)
            {
                var e = list[i];
                if (!e.Alive || (g == (int)eEntityGroup.Enemy && e.IsBoss))
                    continue;
                if (e.X < cx || e.X >= cx + NesCoord.SCREEN_W)
                    continue;
                e.Kill(eKillReason.Killed);
                n++;
            }
        }
        return n;
    }

    /// <summary>Nháy màn (Rosary): từ tick kế tiếp, <paramref name="ticks"/> tick, xám/thường mỗi nửa chu kỳ <paramref name="period"/>.</summary>
    public void FlashScreen(int ticks, int period)
    {
        flashFrom = TickCount + 1;
        flashTo = TickCount + ticks;
        flashHalf = Mathf.Max(1, period / 2);
    }

    /// <summary>Tick hiện tại đang ở pha xám của nháy màn?</summary>
    public bool IsScreenFlashOn => TickCount >= flashFrom && TickCount <= flashTo && ((TickCount - flashFrom) / flashHalf) % 2 == 0;

    private void SetScreenFlash(bool on)
    {
        if (screenFlash == null)
        {
            if (!on || cameraController == null || cameraController.Cam == null || itemCatalog == null)
                return;
            screenFlash = cameraController.Cam.GetComponent<ScreenFlash>();
            if (screenFlash == null)
                screenFlash = cameraController.Cam.gameObject.AddComponent<ScreenFlash>();
            screenFlash.material = itemCatalog.flashMaterial;
            screenFlash.flashColor = itemCatalog.flashColor;
        }
        if (screenFlash.enabled != on)
            screenFlash.enabled = on;
    }

    /// <summary>Đặt lại RNG về seed của stage (Init tự gọi; harness gọi để so tất định giữa các lần chạy).</summary>
    public void ResetRng()
    {
        Rng.Reset(rngSeed);
    }

    public void FreezeWorld(int ticks)
    {
        worldFreezeFrom = TickCount + 1;
        worldFreezeTo = TickCount + ticks;
    }

    public void FreezeEnemies(int ticks)
    {
        // CVM3 03: dùng lại đồng hồ khi đang đóng băng ⇒ chỉ kéo dài, không hở tick đang chạy
        if (!IsEnemyFrozen)
            enemyFreezeFrom = TickCount + 1;
        enemyFreezeTo = TickCount + ticks;
    }

    /// <summary>CVM2 06: bệ di động vào/ra thế giới (MovingPlatform.OnSpawned/OnRemoved) — đi ở bước (1b) trước nhóm Player.</summary>
    internal void AddPlatform(MovingPlatform p)
    {
        if (!platforms.Contains(p))
            platforms.Add(p);
    }

    internal void RemovePlatform(MovingPlatform p)
    {
        platforms.Remove(p);
    }

    public IReadOnlyList<MovingPlatform> Platforms => platforms;

    internal void RaiseHit(ref HitInfo info)
    {
        if (Hit != null)
            Hit(info);
    }

    /// <summary>Sát thương lên Simon ở stage hiện tại (ConfigBlock.damageToSimon).</summary>
    public int DamageToPlayer()
    {
        return Config != null ? Config.DamageToSimon(stage) : 0;
    }

    // ---------------------------------------------------------------- tick

    /// <summary>1 tick thế giới (NesClock.OnTick gọi; harness có thể gọi tay).</summary>
    public void Step()
    {
        if (!initialized)
            Init();
        if (TickBegan != null)
            TickBegan(this);
        long a0 = 0;
        System.Diagnostics.Stopwatch sw = null;
        if (MeasurePerf)
        {
            a0 = GC.GetAllocatedBytesForCurrentThread();
            sw = PerfWatch;
            sw.Restart();
        }

        if (!Paused)
        {
            TickCount++;
            // (1b) CVM2 06: bệ di động đi TRƯỚC mọi nhóm ⇒ Simon đứng trên bệ được kéo theo dx rồi mới tính input/sàn trong tick này.
            // Block 1 không có bệ ⇒ vòng rỗng, thứ tự cũ y nguyên.
            if (platforms.Count > 0)
            {
                bool pwf = IsWorldFrozen, pef = IsEnemyFrozen;
                for (int i = 0; i < platforms.Count; i++)
                {
                    var p = platforms[i];
                    if (p.Alive && p.SpawnTick != TickCount && !p.IsFrozenBy(pwf, pef))
                        p.StepPlatform();
                }
            }
            for (int g = 0; g < groups.Length; g++)
            {
                bool wf = IsWorldFrozen, ef = IsEnemyFrozen;
                var list = groups[g];
                for (int i = 0; i < list.Count; i++)
                {
                    var e = list[i];
                    if (!e.Alive || e.SpawnTick == TickCount || e.IsFrozenBy(wf, ef))
                        continue;
                    if (e.InvincibleTicks > 0)
                        e.InvincibleTicks--;
                    e.OnTick();
                    if (e.Alive && e.CheckDeadlyTiles && Tiles != null)
                    {
                        eTileCollision kind;
                        if (Tiles.CheckDeadly(e.BodyRect, out kind))
                        {
                            if (DeadlyTile != null)
                                DeadlyTile(e, kind);
                            e.OnDeadlyTile(kind);
                        }
                    }
                }
            }
            if (!IsWorldFrozen)
                combat.Resolve(this);
            if (cameraController != null)
                cameraController.Tick();
            int cx = cameraController != null ? cameraController.X : 0;
            for (int i = 0; i < spawnPoints.Count; i++)
                spawnPoints[i].UpdateSpawn(this, cx);
            // CVGP 13: bộ sinh enemy — dừng khi đóng băng thế giới / Stopwatch (QUY ƯỚC)
            if (!IsWorldFrozen && !IsEnemyFrozen && !SpawnersPaused)
                for (int i = 0; i < spawners.Count; i++)
                    spawners[i].Tick(this, cx);
            DespawnOffscreen(cx);
        }
        Cleanup();
        SyncViews();
        SetScreenFlash(IsScreenFlashOn);

        if (MeasurePerf)
        {
            sw.Stop();
            LastStepMs = sw.Elapsed.TotalMilliseconds;
            TotalStepMs += LastStepMs;
            LastStepAlloc = GC.GetAllocatedBytesForCurrentThread() - a0;
            TotalStepAlloc += LastStepAlloc;
            if (LastStepAlloc > 0)
                AllocSteps++;
            if (LastStepAlloc > MaxStepAlloc)
                MaxStepAlloc = LastStepAlloc;
            if (LastStepMs > MaxStepMs)
                MaxStepMs = LastStepMs;
            MeasuredSteps++;
        }
        if (TickEnded != null)
            TickEnded(this);
    }

    private static readonly System.Diagnostics.Stopwatch PerfWatch = new System.Diagnostics.Stopwatch();

    public void ResetPerf()
    {
        TotalStepAlloc = 0;
        MaxStepAlloc = 0;
        AllocSteps = 0;
        TotalStepMs = 0;
        MaxStepMs = 0;
        MeasuredSteps = 0;
    }

    private void DespawnOffscreen(int cx)
    {
        for (int g = 0; g < groups.Length; g++)
        {
            var list = groups[g];
            for (int i = 0; i < list.Count; i++)
            {
                var e = list[i];
                if (!e.Alive || !e.DespawnOffscreen)
                    continue;
                int m = e.OffscreenMargin;
                if (e.X < cx - m || e.X >= cx + NesCoord.SCREEN_W + m)
                    e.Kill(eKillReason.Offscreen);
            }
        }
    }

    /// <summary>Gỡ thực thể đã Kill (giữ thứ tự còn lại), báo SpawnPoint, trả pool / tắt GameObject.</summary>
    private void Cleanup()
    {
        for (int g = 0; g < groups.Length; g++)
        {
            var list = groups[g];
            int w = 0;
            for (int r = 0; r < list.Count; r++)
            {
                var e = list[r];
                if (e.Alive)
                {
                    list[w++] = e;
                    continue;
                }
                RemoveEntity(e);
            }
            if (w < list.Count)
                list.RemoveRange(w, list.Count - w);
        }
        if (Player != null && !Player.Alive)
            Player = null;
    }

    private void RemoveEntity(Entity e)
    {
        var sp = e.Spawner;
        e.Detach();
        if (sp != null)
            sp.NotifyRemoved(e);
        if (e.SourcePrefab != null)
            pool.Release(e);
        else if (e.KillReason != eKillReason.Cleared)
            e.gameObject.SetActive(false);
    }

    private void SyncViews()
    {
        for (int g = 0; g < groups.Length; g++)
        {
            var list = groups[g];
            for (int i = 0; i < list.Count; i++)
                list[i].SyncView();
        }
    }
}
