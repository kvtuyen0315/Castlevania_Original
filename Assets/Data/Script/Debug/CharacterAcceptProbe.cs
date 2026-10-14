#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

/// <summary>
/// Nghiệm thu cuối cụm CVGP (routine 21) — CHỈ Editor, scene Arena (Player = Simon, StageWorld.itemCatalog). Chỉ ĐO, không đổi gameplay.
/// Qua MCP khi Play:
///   CharacterAcceptProbe.StartAudit() … AuditReport() / StopAudit()
///       Kiểm sát thương THỰC TẾ ở mọi đòn StageWorld.Hit (CombatResolver) trong lúc audit bật — chạy cùng phòng trưng bày hoặc các bộ
///       probe 13–20: enemy/boss mất đúng CombatHealth.DamageFor(strength, W, S) với W/S đọc LẠI từ ConfigEnemy/ConfigBoss (Mummy chia
///       entityCount khi hitsScope TotalAllEntities), máu runtime (Health.HitsWeak/Strong) = Config; Simon mất đúng ConfigBlock.damageToSimon
///       của stage đang chơi (wiki Gameplay#Damage).
///   CharacterAcceptProbe.RunGallery(CharacterAcceptProbe.TypeNames(), shotDir, traceDir) ⇒ GalleryDone ⇒ GalleryReport()
///       "Phòng trưng bày": mỗi nhân vật (15 enemy wiki + biến thể dơi đen + Phantom Bat thường + Snake + 6 boss / Dracula 2 dạng) chạy 3
///       replay sinh từ mẫu: Simon ĐỨNG YÊN / TIẾN LẠI / TẤN CÔNG; chụp 4 ảnh/replay (shotDir rỗng ⇒ không chụp), ghi chuỗi state từng thực
///       thể + đòn trúng + hash từng replay (so tất định ở nhiều fps). Simon chỉ bất tử ở replay tấn công (2 replay kia đo sát thương thật;
///       máu ≤ 8 ⇒ harness hồi 16 để không chết giữa chừng — ghi số lần).
///   CharacterAcceptProbe.RunCrowd(ticks, seed) ⇒ CrowdDone ⇒ CrowdReport()
///       Arena đông nhất: phòng Death + 11 enemy (đạn: xương, cầu lửa tháp/rồng/dơi, rìu) + Simon ném nước thánh ×3 liên tục ⇒ Step ms,
///       GC/Step (StageWorld.MeasurePerf), số thực thể tối đa, GameObject trước/sau (rò).
/// </summary>
public static class CharacterAcceptProbe
{
    const string ENEMY_DIR = "Assets/Data/Prefabs/Enemy/";

    // ================================================================ danh mục phòng trưng bày

    private struct GType
    {
        public string name, wiki, room, attack;
        public int stage, sx, sy, sf, whip, tIdle, tApproach, tAttack, walkMax;
    }

    // stage = stage xuất hiện đầu (wiki Enemies/Bosses) ⇒ sát thương lên Simon theo stage; attack: G roi đất, J nhảy quất, D ngồi quất,
    // S ném rìu (wiki khuyên Axe với Death). room: "" = Arena, tên section Map 1 (Panther/Fishman như probe 13), "@<boss>" = phòng thử 14/19/20.
    private static readonly GType[] TYPES =
    {
        T("Ghoul", "E01", 1, "", 100, 176, 1, "G", 1),
        T("Panther", "E03", 1, "EntranceHall", 560, 176, 1, "G", 1),
        T("BatRed", "E02", 2, "", 100, 176, 1, "G", 1),
        T("BatBlack", "E02", 4, "", 100, 176, 1, "G", 1),
        T("Fishman", "E06", 2, "CryptB", 86, 96, 1, "G", 1, 90),
        T("SpearKnight", "E11", 4, "", 100, 176, 1, "G", 1),
        T("MedusaHead", "E07", 5, "", 100, 176, 1, "G", 1),
        T("BoneTower", "E12", 6, "", 100, 176, 1, "G", 2),
        T("Ghost", "E09", 6, "", 100, 176, 1, "G", 2),
        T("Raven", "E04", 7, "", 100, 176, 1, "G", 1),
        T("Skeleton", "E08", 7, "", 100, 176, 1, "G", 1),
        T("Fleaman", "E13", 7, "", 100, 176, 1, "G", 1),
        T("Eagle", "E05", 11, "", 150, 176, 1, "G", 3),
        T("BoneDragon", "E14", 11, "", 150, 176, 1, "G", 2),
        T("RedSkeleton", "E10", 13, "", 100, 176, 1, "G", 1),
        T("AxeKnight", "E15", 14, "", 130, 176, 1, "G", 2),
        T("PhantomBatMinor", "phụ", 16, "", 150, 176, 1, "J", 2),
        T("Snake", "phụ", 6, "", 100, 176, 1, "D", 1),
        B("PhantomBat", "B1", 3, "@PhantomBat", 739, 176, -1, "J"),
        B("QueenMedusa", "B2", 6, "@QueenMedusa", 0, 0, 1, "G"),
        B("MummyMen", "B3", 9, "@MummyMan", 0, 0, 1, "G"),
        B("FrankensteinIgor", "B4", 12, "@Frankenstein", 0, 0, 1, "J"),
        B("Death", "B5", 15, "@GrimReaper", 0, 0, 1, "S"),
        B("Dracula1", "B6", 18, "@DraculaPhase1", 0, 0, 1, "J"),
        B("Dracula2", "B6", 18, "@DraculaPhase2", 0, 0, 1, "J"),
    };

    static GType T(string n, string w, int st, string room, int sx, int sy, int sf, string atk, int whip, int walkMax = 999)
    {
        return new GType { name = n, wiki = w, stage = st, room = room, sx = sx, sy = sy, sf = sf, attack = atk, whip = whip, tIdle = 240, tApproach = 240, tAttack = 420, walkMax = walkMax };
    }

    static GType B(string n, string w, int st, string room, int sx, int sy, int sf, string atk)
    {
        return new GType { name = n, wiki = w, stage = st, room = room, sx = sx, sy = sy, sf = sf, attack = atk, whip = 3, tIdle = 480, tApproach = 480, tAttack = 840, walkMax = 999 };
    }

    public static string TypeNames()
    {
        var sb = new StringBuilder();
        foreach (var t in TYPES)
            sb.Append(sb.Length > 0 ? "," : "").Append(t.name);
        return sb.ToString();
    }

    static readonly string[] MODES = { "idle", "approach", "attack" };

    /// <summary>Replay của 1 chế độ (mẫu chép lặp tới đủ số tick).</summary>
    static string BuildRle(GType t, string mode, out int ticks)
    {
        string dir = t.sf > 0 ? "R" : "L";
        var sb = new StringBuilder();
        int n = 0;
        if (mode == "idle")
        {
            ticks = t.tIdle;
            return "-*" + ticks;
        }
        if (mode == "approach")
        {
            ticks = t.tApproach;
            int w = Math.Min(ticks, t.walkMax);
            return dir + "*" + w + (ticks > w ? " -*" + (ticks - w) : "");
        }
        ticks = t.tAttack;
        string cycle;
        switch (t.attack)
        {
            case "J": cycle = dir + "*8 A -*9 B -*39"; break;       // 58 tick: đi 8, nhảy, quất ở tick nhảy 10 (cao ~31 px)
            case "D": cycle = dir + "*8 D*4 D+B D*23"; break;       // 36 tick: đi 8, ngồi quất
            case "S": cycle = "Sub -*39"; break;                    // 40 tick: ném rìu
            default: cycle = dir + "*8 B -*23"; break;              // 32 tick: đi 8, quất đứng
        }
        var one = NesReplay.Decode(cycle);
        while (n < ticks)
        {
            foreach (var v in one)
            {
                if (n >= ticks) break;
                n++;
            }
            sb.Append(sb.Length > 0 ? " " : "").Append(cycle);
        }
        var all = NesReplay.Decode(sb.ToString());
        all.RemoveRange(ticks, all.Count - ticks);
        return NesReplay.Encode(all);
    }

    // ================================================================ trạng thái chung

    private static StageWorld world;
    private static readonly List<GameObject> temp = new List<GameObject>();
    private static int savedStage;

    static SimonController Simon => world != null ? world.Player as SimonController : null;

    static GameConfig Cfg => world != null ? world.Config : GameConfig.Current;

    static LevelSection Section(string name)
    {
        foreach (var s in world.Sections)
            if (s.data != null && s.data.name == name)
                return s;
        return null;
    }

    static void Go(string section, int x, int y, int facing)
    {
        world.Warp(Section(section), x, y);
        Simon.Teleport(x, y, facing);
        if (world.CameraCtl != null)
            world.CameraCtl.SnapToTarget();
    }

    static EnemyBase EP(string name)
    {
        return UnityEditor.AssetDatabase.LoadAssetAtPath<EnemyBase>(ENEMY_DIR + name + ".prefab");
    }

    static EnemyBase BP(string name)
    {
        return UnityEditor.AssetDatabase.LoadAssetAtPath<EnemyBase>(BossRoomSetup.BOSS_DIR + name + ".prefab");
    }

    static EnemyBase Put(EnemyBase prefab, int x, int y, int facing)
    {
        return world.Spawn(prefab, x, y, facing) as EnemyBase;
    }

    static EdgeSpawner Edge(EnemyBase prefab)
    {
        var go = new GameObject("CharacterAcceptProbe_Edge");
        go.transform.SetParent(world.transform, false);
        temp.Add(go);
        var sp = go.AddComponent<EdgeSpawner>();
        sp.prefab = prefab;
        sp.side = eSpawnSide.FrontOfPlayer;
        sp.playerRelativeY = true;
        world.AddSpawner(sp);
        return sp;
    }

    static BossArena Room(BossRoomSetup.Room r)
    {
        GameObject host;
        var a = BossRoomSetup.Enter(world, r, true, out host);
        if (host != null)
            temp.Add(host);
        return a;
    }

    static void CleanupWorld()
    {
        foreach (var go in temp)
            if (go != null)
            {
                var sp = go.GetComponent<EnemySpawner>();
                if (world != null && sp != null)
                    world.RemoveSpawner(sp);
                UnityEngine.Object.DestroyImmediate(go);
            }
        temp.Clear();
        if (world != null)
        {
            world.FreezeEnemies(0);
            world.ClearNonPlayer();
            if (savedStage > 0)
                world.Stage = savedStage;
        }
    }

    /// <summary>Về Arena (48,176), RNG + phiên đặt lại (như probe 13–20).</summary>
    public static void BackToArena()
    {
        world = StageWorld.Current;
        if (world == null || Simon == null)
            return;
        CleanupWorld();
        Go("Arena", 48, 176, 1);
        world.ResetRng();
        GameSession.Current.ResetForNewGame();
        CvgpDebug.CheatInvincible = false;
    }

    static void ResetSession(int whip, eSubWeapon sub, int hearts)
    {
        var ses = GameSession.Current;
        ses.playerHp = GameSession.MAX_HP;
        ses.hearts = hearts;
        ses.score = 0;
        ses.lives = 3;
        ses.whipLevel = whip;
        ses.shotMultiplier = 1;
        ses.subWeapon = sub;
        ses.subWeaponHits = 0;
        ses.enemyHp = GameSession.MAX_HP;
        ses.NotifyChanged();
    }

    // ================================================================ audit sát thương (StageWorld.Hit ↔ Config)

    private class AuditRow
    {
        public string key;
        public int cfgW = -1, cfgS = -1, rtW = -1, rtS = -1;
        public bool immune;
        public int weak, strong, bad, immuneHits, rtMismatch;
        public readonly HashSet<string> attackers = new HashSet<string>();
        public readonly List<string> badSamples = new List<string>();
    }

    private class PlayerRow
    {
        public string key;
        public int hits, bad;
        public readonly HashSet<string> stages = new HashSet<string>();
        public readonly List<string> badSamples = new List<string>();
    }

    public static bool Auditing { get; private set; }
    private static StageWorld auditWorld;
    private static readonly Dictionary<string, AuditRow> audit = new Dictionary<string, AuditRow>();
    private static readonly Dictionary<string, PlayerRow> auditPlayer = new Dictionary<string, PlayerRow>();
    private static readonly Dictionary<string, int> auditShots = new Dictionary<string, int>();
    private static readonly Dictionary<Entity, int> lastHp = new Dictionary<Entity, int>();
    private static int lastPlayerHp = -1, auditTicks;
    private static readonly FieldInfo PHASE = typeof(BossBase).GetField("phase", BindingFlags.NonPublic | BindingFlags.Instance);

    public static void StartAudit()
    {
        StopAudit();
        audit.Clear();
        auditPlayer.Clear();
        auditShots.Clear();
        lastHp.Clear();
        lastPlayerHp = -1;
        auditTicks = 0;
        auditWorld = StageWorld.Current;
        if (auditWorld == null)
        {
            Debug.LogError("[CharacterAcceptProbe] không có StageWorld");
            return;
        }
        auditWorld.Hit += OnAuditHit;
        auditWorld.TickBegan += OnAuditTick;
        Auditing = true;
    }

    public static void StopAudit()
    {
        if (auditWorld != null)
        {
            auditWorld.Hit -= OnAuditHit;
            auditWorld.TickBegan -= OnAuditTick;
        }
        Auditing = false;
    }

    /// <summary>Đầu tick (TickBegan — sau khi harness hồi máu ở cuối tick trước): máu mọi enemy + Simon trước combat.</summary>
    static void OnAuditTick(StageWorld w)
    {
        auditTicks++;
        lastHp.Clear();
        foreach (var e in w.GroupList(eEntityGroup.Enemy))
        {
            var en = e as EnemyBase;
            if (en != null && en.Alive)
                lastHp[en] = en.Health.Current;
        }
        lastPlayerHp = GameSession.Current.playerHp;
    }

    /// <summary>Khoá báo cáo + W/S kỳ vọng đọc lại từ Config (độc lập code nhân vật).</summary>
    static string KeyOf(EnemyBase en, out int w, out int s, out bool immune)
    {
        w = s = -1;
        immune = false;
        var cfg = auditWorld.Config;
        var boss = en as BossBase;
        if (boss != null && !(en is PhantomBatMinor))
        {
            int phase = PHASE != null ? (int)PHASE.GetValue(boss) : 1;
            var row = cfg.GetBoss(boss.BossId, phase);
            if (row != null && row.hitsWeak.has && row.hitsStrong.has)
            {
                int n = row.hitsScope == eHitsScope.TotalAllEntities ? Math.Max(1, row.entityCount) : 1;
                w = row.hitsWeak.value / n;
                s = row.hitsStrong.value / n;
            }
            return "Boss." + boss.BossId + ".p" + phase;
        }
        var er = cfg.GetEnemy(en.EnemyId);
        if (er != null && er.hitsWeak.has && er.hitsStrong.has)
        {
            w = er.hitsWeak.value;
            s = er.hitsStrong.value;
        }
        else
            immune = true;   // Igor: ConfigEnemy hits "-" (bất tử — wiki)
        return "Enemy." + en.EnemyId;
    }

    static string AttackerOf(HitInfo h)
    {
        var sw = h.attacker as SubWeaponProjectile;
        if (sw != null)
            return sw.GetType().Name;
        if (h.attacker is SimonController)
            return "Roi L" + GameSession.Current.whipLevel;
        return h.attacker != null ? h.attacker.GetType().Name : "?";
    }

    static void OnAuditHit(HitInfo h)
    {
        var w = auditWorld;
        var en = h.victim as EnemyBase;
        if (en != null)
        {
            int cw, cs;
            bool immune;
            string key = KeyOf(en, out cw, out cs, out immune);
            AuditRow r;
            if (!audit.TryGetValue(key, out r))
            {
                r = new AuditRow { key = key, cfgW = cw, cfgS = cs, immune = immune, rtW = en.Health.HitsWeak, rtS = en.Health.HitsStrong };
                audit[key] = r;
            }
            r.attackers.Add(AttackerOf(h) + "/" + h.strength);
            if (!immune && (en.Health.HitsWeak != cw || en.Health.HitsStrong != cs))
                r.rtMismatch++;
            int before;
            if (!lastHp.TryGetValue(en, out before))
                before = en.Health.Max;
            int after = en.Health.Current;
            if (immune)
            {
                r.immuneHits++;
                if (after != before)
                {
                    r.bad++;
                    if (r.badSamples.Count < 4) r.badSamples.Add("bất tử mà máu " + before + "→" + after);
                }
                lastHp[en] = after;
                return;
            }
            int expect = Math.Min(before, CombatHealth.DamageFor(h.strength, cw, cs));
            int delta = before - after;
            // Red Skeleton: hết máu ⇒ đổ + ResetHealth ngay trong OnHurt (không chết — wiki) ⇒ máu sau = Max
            var red = en as RedSkeleton;
            if (red != null && expect == before && after == en.Health.Max)
                delta = before;
            if (h.strength == eHitStrength.Strong) r.strong++; else r.weak++;
            if (delta != expect)
            {
                r.bad++;
                if (r.badSamples.Count < 4) r.badSamples.Add(AttackerOf(h) + " " + h.strength + ": máu " + before + "→" + after + " (kỳ vọng −" + expect + ")");
            }
            lastHp[en] = after;
            return;
        }
        if (h.victim == w.Player)
        {
            string key = h.attacker != null ? h.attacker.GetType().Name + (h.hitbox != null ? "/" + h.hitbox.layer : "") : "?";
            PlayerRow r;
            if (!auditPlayer.TryGetValue(key, out r))
            {
                r = new PlayerRow { key = key };
                auditPlayer[key] = r;
            }
            r.hits++;
            int expect = w.Config.DamageToSimon(w.Stage);
            r.stages.Add(w.Stage + ":" + expect);
            int before = lastPlayerHp >= 0 ? lastPlayerHp : GameSession.MAX_HP;
            int after = GameSession.Current.playerHp;
            if (h.damage != expect || before - after != Math.Min(before, expect))
            {
                r.bad++;
                if (r.badSamples.Count < 4) r.badSamples.Add("stage " + w.Stage + ": damage " + h.damage + ", máu " + before + "→" + after + " (kỳ vọng −" + expect + ")");
            }
            lastPlayerHp = after;
            return;
        }
        if (h.victim != null && h.victim.Group == eEntityGroup.EnemyShot)
        {
            string k = h.victim.GetType().Name;
            int c;
            auditShots.TryGetValue(k, out c);
            auditShots[k] = c + 1;
        }
    }

    public static string AuditReport()
    {
        var sb = new StringBuilder();
        int bad = 0, n = 0, pbad = 0, pn = 0;
        foreach (var r in audit.Values) { bad += r.bad + r.rtMismatch; n += r.weak + r.strong + r.immuneHits; }
        foreach (var r in auditPlayer.Values) { pbad += r.bad; pn += r.hits; }
        sb.Append("AUDIT ").Append(Auditing ? "đang bật" : "tắt").Append(", ").Append(auditTicks).Append(" tick: ").Append(n).Append(" đòn vào ").Append(audit.Count)
          .Append(" loại enemy/boss (sai ").Append(bad).Append("), ").Append(pn).Append(" đòn vào Simon từ ").Append(auditPlayer.Count).Append(" nguồn (sai ").Append(pbad).Append(")\n");
        var keys = new List<string>(audit.Keys);
        keys.Sort(StringComparer.Ordinal);
        foreach (var k in keys)
        {
            var r = audit[k];
            sb.Append(r.bad + r.rtMismatch == 0 ? "  [ĐẠT] " : "  [SAI] ").Append(k).Append(": Config ")
              .Append(r.immune ? "bất tử" : r.cfgW + "/" + r.cfgS).Append(", runtime ").Append(r.rtW).Append("/").Append(r.rtS)
              .Append(" — yếu ").Append(r.weak).Append(", mạnh ").Append(r.strong);
            if (r.immuneHits > 0) sb.Append(", trúng khi bất tử ").Append(r.immuneHits);
            sb.Append("; sai ").Append(r.bad).Append(", lệch máu runtime ").Append(r.rtMismatch).Append(" | ").Append(string.Join(", ", r.attackers));
            foreach (var s in r.badSamples) sb.Append("\n      ! ").Append(s);
            sb.Append('\n');
        }
        keys = new List<string>(auditPlayer.Keys);
        keys.Sort(StringComparer.Ordinal);
        foreach (var k in keys)
        {
            var r = auditPlayer[k];
            sb.Append(r.bad == 0 ? "  [ĐẠT] " : "  [SAI] ").Append("→ Simon ← ").Append(k).Append(": ").Append(r.hits).Append(" đòn, stage:vạch ")
              .Append(string.Join(" ", r.stages)).Append("; sai ").Append(r.bad);
            foreach (var s in r.badSamples) sb.Append("\n      ! ").Append(s);
            sb.Append('\n');
        }
        if (auditShots.Count > 0)
        {
            sb.Append("  đạn bị phá:");
            foreach (var kv in auditShots) sb.Append(' ').Append(kv.Key).Append('×').Append(kv.Value);
            sb.Append('\n');
        }
        return sb.ToString();
    }

    // ================================================================ phòng trưng bày

    private class Track
    {
        public Entity e;
        public int spawnTick, spawnRel, endRel = -1, hits, lastX, lastY;
        public string type, prevState;
        public bool defeated;
        public eKillReason reason;
        public readonly StringBuilder chain = new StringBuilder();
        public int changes;
        public readonly HashSet<string> seen = new HashSet<string>();
    }

    private struct Run { public GType t; public string mode; }

    public static bool GalleryRunning { get; private set; }
    public static bool GalleryDone { get; private set; } = true;
    public static readonly List<string> GalleryLines = new List<string>();
    public static readonly Dictionary<string, uint> RunHashes = new Dictionary<string, uint>();
    private static readonly Queue<Run> queue = new Queue<Run>();
    private static string shotDir, traceDir, current;
    private static Run cur;
    private static int target, recorded, startTick, heals;
    private static uint hash;
    private static readonly Dictionary<int, string> shots = new Dictionary<int, string>();
    private static readonly List<Track> tracks = new List<Track>();
    private static readonly List<string> hitLog = new List<string>();
    private static readonly List<string> simonLog = new List<string>();
    private static string prevSimon;

    /// <summary>Chạy phòng trưng bày cho danh sách tên (TypeNames()); shotDir/traceDir tuyệt đối (rỗng ⇒ không chụp / không ghi vết).</summary>
    public static int RunGallery(string names, string shotDirAbs, string traceDirAbs)
    {
        world = StageWorld.Current;
        if (world == null || Simon == null || world.Items == null)
        {
            Debug.LogError("[CharacterAcceptProbe] cần scene Arena với Player = SimonController + StageWorld.itemCatalog");
            return -1;
        }
        UnhookGallery();
        queue.Clear();
        GalleryLines.Clear();
        RunHashes.Clear();
        shotDir = shotDirAbs;
        traceDir = traceDirAbs;
        if (!string.IsNullOrEmpty(shotDir)) Directory.CreateDirectory(shotDir);
        if (!string.IsNullOrEmpty(traceDir)) Directory.CreateDirectory(traceDir);
        foreach (var n in names.Split(','))
            foreach (var t in TYPES)
                if (t.name == n.Trim())
                    foreach (var m in MODES)
                        queue.Enqueue(new Run { t = t, mode = m });
        savedStage = world.Stage;
        GalleryDone = false;
        int count = queue.Count;
        NextRun();
        return count;
    }

    static void NextRun()
    {
        while (queue.Count > 0)
            if (StartRun(queue.Dequeue()))
                return;
        GalleryDone = true;
        BackToArena();
    }

    static bool StartRun(Run r)
    {
        var t = r.t;
        CleanupWorld();
        cur = r;
        current = t.name + "_" + r.mode;
        int ticks;
        string rle = BuildRle(t, r.mode, out ticks);
        bool attack = r.mode == "attack";
        ResetSession(attack ? t.whip : 1, attack && t.attack == "S" ? eSubWeapon.Axe : eSubWeapon.None, attack && t.attack == "S" ? 99 : 5);
        CvgpDebug.CheatInvincible = attack;
        world.ResetRng();
        world.Stage = t.stage;
        var ses = GameSession.Current;
        ses.stage = t.stage;
        ses.NotifyChanged();   // HUD hiện đúng STAGE của nhân vật
        bool ok = true;
        switch (t.name)
        {
            case "Ghoul": Go("Arena", t.sx, t.sy, t.sf); Put(EP("Ghoul"), 240, 176, -1); break;
            case "Panther": Go("EntranceHall", t.sx, t.sy, t.sf); Put(EP("Panther"), 699, 112, -1); break;
            case "BatRed": Go("Arena", t.sx, t.sy, t.sf); Put(EP("Bat"), 248, 176 - 14, -1); break;
            case "BatBlack":
                {
                    Go("Arena", t.sx, t.sy, t.sf);
                    var b = Put(EP("Bat"), 220, 176 - 48, -1) as Bat;
                    if (b != null) b.ForcePerch();
                }
                break;
            case "Fishman":
                {
                    Go("CryptB", t.sx, t.sy, t.sf);
                    var f = Put(EP("Fishman"), 160, 192, -1) as Fishman;
                    if (f != null) f.Emerge(160);
                }
                break;
            case "SpearKnight": Go("Arena", t.sx, t.sy, t.sf); Put(EP("SpearKnight"), 220, 176, -1); break;
            case "MedusaHead": Go("Arena", t.sx, t.sy, t.sf); Edge(EP("MedusaHead")); break;
            case "BoneTower": Go("Arena", t.sx, t.sy, t.sf); Put(EP("BoneTower"), 220, 176, -1); break;
            case "Ghost": Go("Arena", t.sx, t.sy, t.sf); Put(EP("Ghost"), 220, 176 - 24, -1); break;
            case "Raven": Go("Arena", t.sx, t.sy, t.sf); Put(EP("Raven"), 220, 176 - 40, -1); break;
            case "Skeleton": Go("Arena", t.sx, t.sy, t.sf); Put(EP("Skeleton"), 220, 176, -1); break;
            case "Fleaman": Go("Arena", t.sx, t.sy, t.sf); Put(EP("Fleaman"), 220, 176, -1); break;
            case "Eagle": Go("Arena", t.sx, t.sy, t.sf); Edge(EP("Eagle")); break;
            case "BoneDragon": Go("Arena", t.sx, t.sy, t.sf); Put(EP("BoneDragon"), 300, 136, -1); break;
            case "RedSkeleton": Go("Arena", t.sx, t.sy, t.sf); Put(EP("RedSkeleton"), 300, 176, -1); break;
            case "AxeKnight": Go("Arena", t.sx, t.sy, t.sf); Put(EP("AxeKnight"), 250, 176, -1); break;
            case "PhantomBatMinor": Go("Arena", t.sx, t.sy, t.sf); Put(EP("PhantomBatMinor"), 260, 96, -1); break;
            case "Snake": Go("Arena", t.sx, t.sy, t.sf); Put(BP("Snake"), 200, 150, -1); break;
            case "PhantomBat":
                {
                    Go("PhantomBat", t.sx, t.sy, t.sf);
                    var go = new GameObject("CharacterAcceptProbe_BossArena");
                    go.transform.SetParent(world.transform, false);
                    temp.Add(go);
                    var a = go.AddComponent<BossArena>();
                    a.prefab = BP("PhantomBat");
                    world.AddSpawner(a);
                    world.CameraCtl.Lock(512);
                    a.SpawnBoss(world);
                }
                break;
            case "QueenMedusa": Room(BossRoomSetup.MEDUSA); break;
            case "MummyMen": Room(BossRoomSetup.MUMMY); break;
            case "FrankensteinIgor": Room(BossRoomSetup.FRANK); break;
            case "Death": Room(BossRoomSetup.DEATH); break;
            case "Dracula1": Room(BossRoomSetup.DRACULA); break;
            case "Dracula2":
                {
                    var rr = BossRoomSetup.DRACULA;
                    rr.boss = "DraculaPhase2";
                    Room(rr);
                }
                break;
            default: ok = false; break;
        }
        if (!ok)
        {
            GalleryLines.Add("[LỖI] không có nhân vật " + t.name);
            return false;
        }
        world.Stage = t.stage;   // phòng thử đặt stage boss — giữ stage xuất hiện đầu
        world.ResetRng();
        shots.Clear();
        if (!string.IsNullOrEmpty(shotDir))
            for (int k = 1; k <= 4; k++)
                shots[ticks * k / 5] = Path.Combine(shotDir, current + "_r" + (ticks * k / 5).ToString("000") + ".png");
        Simon.ResetStats();
        tracks.Clear();
        hitLog.Clear();
        simonLog.Clear();
        prevSimon = null;
        heals = 0;
        target = ticks;
        recorded = 0;
        hash = 2166136261u;
        startTick = world.TickCount;
        foreach (var e in world.GroupList(eEntityGroup.Enemy))
            TrackOf(e, 0);
        NesPad.I.PlayReplay(NesReplay.Decode(rle));
        world.TickEnded += OnGalleryTick;
        world.Hit += OnGalleryHit;
        GalleryRunning = true;
        return true;
    }

    static void UnhookGallery()
    {
        if (world != null)
        {
            world.TickEnded -= OnGalleryTick;
            world.Hit -= OnGalleryHit;
        }
        GalleryRunning = false;
    }

    static Track TrackOf(Entity e, int rel)
    {
        if (e == null)
            return null;
        foreach (var t in tracks)
            if (t.e == e && t.spawnTick == e.SpawnTick)
                return t;
        var n = new Track { e = e, spawnTick = e.SpawnTick, spawnRel = rel, type = e.GetType().Name };
        tracks.Add(n);
        return n;
    }

    static void OnGalleryHit(HitInfo h)
    {
        int rel = world.TickCount - startTick - 1;
        var en = h.victim as EnemyBase;
        if (en != null)
        {
            var t = TrackOf(en, rel);
            t.hits++;
            t.defeated = en.defeated;   // đòn chí mạng: Die() trong OnHurt ⇒ thực thể đã gỡ trước TickEnded
            hitLog.Add(rel + ": " + AttackerOf(h) + " " + h.strength + " → " + t.type + " máu " + en.Health.Current + "/" + en.Health.Max);
        }
        else if (h.victim == world.Player)
            hitLog.Add(rel + ": " + (h.attacker != null ? h.attacker.GetType().Name : "?") + "/" + (h.hitbox != null ? h.hitbox.layer.ToString() : "?") + " → Simon −" + h.damage
                       + " (máu " + GameSession.Current.playerHp + ")");
        else if (h.victim != null && h.victim.Group == eEntityGroup.EnemyShot)
            hitLog.Add(rel + ": " + AttackerOf(h) + " phá " + h.victim.GetType().Name);
    }

    static void Mix(int v)
    {
        uint x = hash;
        for (int i = 0; i < 4; i++)
        {
            x ^= (uint)(v & 0xFF);
            x *= 16777619u;
            v >>= 8;
        }
        hash = x;
    }

    static void OnGalleryTick(StageWorld w)
    {
        if (!GalleryRunning)
            return;
        int rel = w.TickCount - startTick - 1;
        var s = Simon;
        var ses = GameSession.Current;
        for (int g = (int)eEntityGroup.Enemy; g <= (int)eEntityGroup.EnemyShot; g++)
            foreach (var e in w.GroupList(g))
            {
                if (!e.Alive) continue;
                var t = TrackOf(e, rel);
                string st = e.DebugState;
                int sp = st.IndexOf(' ');
                if (sp > 0) st = st.Substring(0, sp);
                var en = e as EnemyBase;
                if (en != null && en.hidden) st += "(ẩn)";
                if (st != t.prevState)
                {
                    if (t.changes < 40)
                        t.chain.Append(t.chain.Length > 0 ? " → " : "").Append(st).Append('@').Append(rel);
                    t.changes++;
                    t.prevState = st;
                    t.seen.Add(st);
                }
                t.lastX = e.X;
                t.lastY = e.Y;
                if (en != null) t.defeated = en.defeated;
                Mix(e.GetType().Name.Length); Mix(e.fx); Mix(e.fy); Mix(e.Facing);
            }
        foreach (var t in tracks)
            if (t.endRel < 0 && (!t.e.Alive || t.e.SpawnTick != t.spawnTick))
            {
                t.endRel = rel;
                t.reason = t.e.KillReason;
            }
        string ss = s.Fsm.CurState.ToString();
        if (ss != prevSimon)
        {
            if (simonLog.Count < 60) simonLog.Add(ss + "@" + rel);
            prevSimon = ss;
        }
        Mix(rel); Mix(s.fx); Mix(s.fy); Mix((int)s.Fsm.CurState); Mix(ses.playerHp); Mix(ses.score); Mix(w.Rng.State);
        if (ses.playerHp <= 8 && ses.playerHp > 0)
        {
            ses.playerHp = GameSession.MAX_HP;   // harness: không để Simon chết giữa replay đo sát thương
            ses.NotifyChanged();
            heals++;
        }
        string shot;
        if (shots.TryGetValue(rel, out shot))
            ScreenCapture.CaptureScreenshot(shot);
        if (++recorded >= target)
            FinishRun();
    }

    static void FinishRun()
    {
        UnhookGallery();
        NesPad.I.StopReplay();
        var ses = GameSession.Current;
        var sb = new StringBuilder();
        sb.Append("[").Append(cur.t.wiki).Append(" ").Append(current).Append("] ").Append(recorded).Append(" tick, stage ").Append(cur.t.stage)
          .Append(", hash ").Append(hash.ToString("x8")).Append(", điểm ").Append(ses.score).Append(", Simon máu cuối ").Append(ses.playerHp)
          .Append(heals > 0 ? " (hồi " + heals + " lần)" : "").Append('\n');
        var byType = new Dictionary<string, int>();
        foreach (var t in tracks)
        {
            int c;
            byType.TryGetValue(t.type, out c);
            byType[t.type] = c + 1;
        }
        sb.Append("    thực thể:");
        foreach (var kv in byType) sb.Append(' ').Append(kv.Key).Append('×').Append(kv.Value);
        sb.Append('\n');
        int shown = 0;
        foreach (var t in tracks)
        {
            if (t.e.Group == eEntityGroup.EnemyShot) continue;
            if (shown++ >= 6) break;
            sb.Append("    ").Append(t.type).Append(" sinh@").Append(t.spawnRel).Append(": ").Append(t.chain);
            if (t.changes > 40) sb.Append(" … (").Append(t.changes).Append(" lần đổi)");
            sb.Append(" | state đã qua {").Append(string.Join(",", t.seen)).Append("}, trúng ").Append(t.hits)
              .Append(t.endRel >= 0 ? (t.defeated ? ", HẠ@" : ", gỡ(" + t.reason + ")@") + t.endRel : ", còn ở (" + t.lastX + "," + t.lastY + ")").Append('\n');
        }
        int simonHits = 0, enemyHits = 0;
        foreach (var l in hitLog) { if (l.Contains("→ Simon")) simonHits++; else enemyHits++; }
        sb.Append("    đòn: ").Append(enemyHits).Append(" vào enemy/đạn, ").Append(simonHits).Append(" vào Simon");
        if (hitLog.Count > 0) sb.Append(" — ").Append(string.Join(" | ", hitLog.GetRange(0, Math.Min(4, hitLog.Count))));
        sb.Append('\n');
        sb.Append("    Simon: ").Append(string.Join(" ", simonLog.GetRange(0, Math.Min(14, simonLog.Count))));
        GalleryLines.Add(sb.ToString());
        RunHashes[current] = hash;
        if (!string.IsNullOrEmpty(traceDir))
        {
            var tr = new StringBuilder(sb.ToString()).Append("\n\n# đầy đủ\n");
            foreach (var t in tracks)
                tr.Append(t.type).Append(" sinh@").Append(t.spawnRel).Append(" kết@").Append(t.endRel).Append(" ").Append(t.reason).Append(" trúng ").Append(t.hits)
                  .Append(": ").Append(t.chain).Append('\n');
            tr.Append("\n# đòn\n");
            foreach (var l in hitLog) tr.Append(l).Append('\n');
            tr.Append("\n# Simon\n").Append(string.Join(" ", simonLog)).Append('\n');
            File.WriteAllText(Path.Combine(traceDir, current + ".txt"), tr.ToString(), new UTF8Encoding(false));
        }
        CleanupWorld();
        NextRun();
    }

    public static string GalleryReport()
    {
        var sb = new StringBuilder();
        uint all = 2166136261u;
        foreach (var kv in RunHashes)
        {
            all ^= kv.Value;
            all *= 16777619u;
        }
        sb.Append("GALLERY ").Append(GalleryDone ? "xong" : "ĐANG CHẠY (" + current + " " + recorded + "/" + target + ")").Append(", ").Append(GalleryLines.Count)
          .Append(" replay, hash bộ ").Append(all.ToString("x8")).Append('\n');
        foreach (var l in GalleryLines)
            sb.Append(l).Append('\n');
        return sb.ToString();
    }

    public static string HashList()
    {
        var sb = new StringBuilder();
        foreach (var kv in RunHashes)
            sb.Append(kv.Key).Append(' ').Append(kv.Value.ToString("x8")).Append('\n');
        return sb.ToString();
    }

    // ================================================================ Arena đông nhất (hiệu năng)

    public static bool CrowdDone { get; private set; } = true;
    private static int crowdTarget, crowdTicks, crowdMaxEnt, crowdMaxShots, crowdGoBefore, crowdStart;
    private static string crowdReport = "";

    static int CountGameObjects()
    {
        return UnityEngine.Object.FindObjectsOfType<Transform>(true).Length;
    }

    /// <summary>Phòng Death + 11 enemy + Simon (bất tử) ném nước thánh ×3 mỗi 10 tick; đo ticks tick.</summary>
    public static int RunCrowd(int ticks, int seed)
    {
        world = StageWorld.Current;
        if (world == null || Simon == null)
            return -1;
        savedStage = world.Stage;
        CleanupWorld();
        crowdGoBefore = CountGameObjects();
        ResetSession(3, eSubWeapon.HolyWater, 99);
        GameSession.Current.shotMultiplier = 3;
        CvgpDebug.CheatInvincible = true;
        Room(BossRoomSetup.DEATH);
        world.Stage = 15;
        Put(EP("SpearKnight"), 60, 176, 1);
        Put(EP("Skeleton"), 200, 176, -1);
        Put(EP("BoneTower"), 232, 176, -1);
        Put(EP("AxeKnight"), 180, 176, -1);
        Put(EP("Ghost"), 120, 120, -1);
        Put(EP("Raven"), 90, 100, -1);
        Put(EP("Fleaman"), 150, 176, -1);
        Put(EP("RedSkeleton"), 100, 176, -1);
        Put(EP("BoneDragon"), 240, 96, -1);
        Put(EP("PhantomBatMinor"), 64, 80, 1);
        Edge(EP("MedusaHead"));
        world.Rng.Reset(seed);
        var rle = new StringBuilder();
        for (int n = 0; n < ticks; n += 10)
            rle.Append(n > 0 ? " " : "").Append("Sub -*9");
        NesPad.I.PlayReplay(NesReplay.Decode(rle.ToString()));
        world.ResetPerf();
        StageWorld.MeasurePerf = true;
        crowdTarget = ticks;
        crowdTicks = 0;
        crowdMaxEnt = crowdMaxShots = 0;
        crowdStart = world.TickCount;
        CrowdDone = false;
        world.TickEnded += OnCrowdTick;
        return ticks;
    }

    static void OnCrowdTick(StageWorld w)
    {
        int ent = 0, sh = 0;
        for (int g = 0; g < Entity.GROUP_COUNT; g++)
            foreach (var e in w.GroupList(g))
                if (e.Alive)
                {
                    ent++;
                    if (e.Group == eEntityGroup.EnemyShot || e.Group == eEntityGroup.PlayerWeapon) sh++;
                }
        crowdMaxEnt = Math.Max(crowdMaxEnt, ent);
        crowdMaxShots = Math.Max(crowdMaxShots, sh);
        if (++crowdTicks < crowdTarget)
            return;
        w.TickEnded -= OnCrowdTick;
        StageWorld.MeasurePerf = false;
        NesPad.I.StopReplay();
        int n = Math.Max(1, w.MeasuredSteps);
        crowdReport = "CROWD " + w.MeasuredSteps + " Step (phòng Death + 11 enemy + nước thánh ×3): thực thể sống tối đa " + crowdMaxEnt + " (vũ khí/đạn tối đa " + crowdMaxShots
                      + "), Step TB " + (w.TotalStepMs / n).ToString("0.0000") + " ms, max " + w.MaxStepMs.ToString("0.000") + " ms; GC tổng " + w.TotalStepAlloc
                      + " B, Step có cấp phát " + w.AllocSteps + ", max " + w.MaxStepAlloc + " B; pool đã tạo " + w.Pool.CreatedCount;
        BackToArena();
        crowdReport += "; GameObject trước " + crowdGoBefore + " / sau dọn " + CountGameObjects();
        CrowdDone = true;
    }

    public static string CrowdReport() { return CrowdDone ? crowdReport : "CROWD ĐANG CHẠY " + crowdTicks + "/" + crowdTarget; }
}
#endif
