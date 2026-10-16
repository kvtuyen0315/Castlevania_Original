#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// BỘ CHẠY HỒI QUY 1 LỆNH (CVM2 00; gộp cvgp-fix-05) — CHỈ Editor, đang Play. Chạy NỐI TIẾP các probe qua EditorApplication.update,
/// ghi từng bộ ra &lt;dir&gt;/suites/&lt;bộ&gt;.txt + SUMMARY.txt (bộ | đạt/tổng | hash | hash chuẩn | khớp?) + progress.txt + ALL.done, rồi so
/// BẢNG HASH CHUẨN (EXPECTED dưới — chép ở CVM2_MEMORY §4; chỉ đổi khi routine CỐ Ý đổi hành vi, ghi lý do vào sổ).
///   RegressionRunner.Start("all", 240, dir)   scene Arena, Player = Simon: long, simon, attack, item, enemy, boss, b23, b46, b24, b56, holy (CVM2 04), mech (CVM2 06), rules
///   RegressionRunner.Start("full", 240, dir)  = all + gallery (25 nhân vật × 3 replay, KHÔNG chụp; ~4 phút)
///   RegressionRunner.Start("core", 240, dir)  scene Arena với EditorPrefs CVGP.ArenaUseDummy = true (Player = ArenaDummy) — Play MỚI
///   RegressionRunner.Start("main", 60, dir)   scene Main: flow (luồng thật Title → STAGE 04 + h8 5 mốc) + spam1..4 (spam phím 4 lượt)
///   Tên lẻ (cách nhau dấu phẩy): long,simon,attack,item,enemy,boss,b23,b46,b24,b56,holy,mech,rules,gallery,core,flow,flow30,flow144,spam,spam1..spam4
///   RegressionRunner.Status() ⇒ tiến độ / SUMMARY khi xong; RegressionRunner.Abort().
/// Thứ tự: long ĐẦU phiên (RegressionProbe rò trạng thái khi chạy sau RulesProbe — CVM2_MEMORY §4), rules CUỐI (sau fix-05 vẫn để cuối cho chắc).
/// fps = targetFrameRate cho cả bộ (hash tất định theo fps); gallery tự chạy 240; spam tự chạy ≥ 144. Trả lại fps/vSync/runInBackground khi xong.
/// </summary>
public static class RegressionRunner
{
    /// <summary>BẢNG HASH CHUẨN (baseline = CVGP 21 / CVM2 00). h8 = 5 mốc L (Start, đen, Intro, đen, Stage).</summary>
    public static readonly Dictionary<string, string> EXPECTED = new Dictionary<string, string>
    {
        { "long", "5963c4ba" }, { "simon", "b2954cbe" }, { "attack", "eee2cdfb" }, { "item", "7ca5bf56" },
        { "enemy", "f288b0ce" }, { "boss", "31e6d711" }, { "b23", "171be0c1" }, { "b46", "58a8edf9" },
        { "b24", "7adfbc54" }, { "b56", "6cb8a1ad" }, { "rules", "03fafb24" }, { "core", "76516076" },
        { "gallery", "517442ed" }, { "flow", "e0b2d54c" }, { "flow30", "e0b2d54c" }, { "flow144", "e0b2d54c" },
        { "holy", "52c59864" },   // CVM2 04 (HolyPierceProbe — bộ mới)
        { "mech", "91215ebd" },   // CVM2 06 (Map2MechProbe — bộ mới: bệ di động, bẫy ép gai, hố)
        { "h8", "352/432/448/890/922" },
    };

    const string CORE_NAMES = "walk300,wallR,wallR3,wallR24,wallL,wallL3,ledge,stairs,stair2,water,pit,camR,camL,hit,touch,matrix,spawn,freeze,freezeEnemy,mix,mix";
    const string ARENA_ALL = "long,simon,attack,item,enemy,boss,b23,b46,b24,b56,holy,mech,rules";

    class Result
    {
        public int pass, total;
        public string hash = "-";
        public string text = "";
        public string note = "";
        public bool skip;
    }

    class SuiteDef
    {
        public string name;
        public string session;            // arena | core | main
        public int fps;                   // 0 = dùng fps của bộ
        public int timeoutSec = 1200;
        public Func<string> start;        // null/"" ⇒ ok; chuỗi khác ⇒ lỗi
        public Func<bool> done;
        public Func<Result> finish;
    }

    public static bool Running { get; private set; }
    public static bool Done { get; private set; } = true;
    static readonly List<SuiteDef> queue = new List<SuiteDef>();
    static readonly List<string> summary = new List<string>();
    static readonly List<string> progress = new List<string>();
    static SuiteDef cur;
    static double curStart, allStart;
    static string outDir;
    static int fpsParam, mismatches, skips, failed;
    static int prevFps, prevVSync;
    static bool prevBg;

    // ---------------------------------------------------------------- vào

    public static string Start(string suites, int fps, string dir)
    {
        if (Running)
            return "đang chạy — Abort() trước";
        if (!EditorApplication.isPlaying)
            return "cần Play (Arena hoặc Main)";
        outDir = dir.Replace('\\', '/').TrimEnd('/');
        Directory.CreateDirectory(outDir + "/suites");
        queue.Clear();
        summary.Clear();
        progress.Clear();
        mismatches = skips = failed = 0;
        fpsParam = fps;
        foreach (var raw in suites.Split(','))
        {
            var n = raw.Trim();
            if (n == "all") foreach (var a in ARENA_ALL.Split(',')) Add(a);
            else if (n == "full") { foreach (var a in ARENA_ALL.Split(',')) Add(a); Add("gallery"); }
            else if (n == "main") { Add("flow"); Add("spam1"); Add("spam2"); Add("spam3"); Add("spam4"); }
            else if (n == "spam") { Add("spam1"); Add("spam2"); Add("spam3"); Add("spam4"); }
            else if (n.Length > 0) Add(n);
        }
        foreach (var d in queue)
            if (d == null)
                return "tên bộ lạ — xem chú thích đầu RegressionRunner";
        File.Delete(outDir + "/suites/ALL.done");
        prevFps = Application.targetFrameRate;
        prevVSync = QualitySettings.vSyncCount;
        prevBg = Application.runInBackground;
        QualitySettings.vSyncCount = 0;
        Application.runInBackground = true;
        if (StageWorld.Current != null && GameSession.Current != null && StageWorld.Current.Player is SimonController)
            GameSession.Current.ResetForNewGame();   // static không reset giữa các phiên Play (domain reload tắt)
        allStart = EditorApplication.timeSinceStartup;
        cur = null;
        Running = true;
        Done = false;
        EditorApplication.update += Pump;
        Log("bắt đầu " + queue.Count + " bộ @" + fps + " fps → " + outDir);
        return "bộ chạy bắt đầu: " + queue.Count + " bộ";
    }

    public static void Abort()
    {
        if (!Running)
            return;
        Finish("HUỶ TAY");
    }

    public static string Status()
    {
        var sb = new StringBuilder();
        sb.Append(Running ? "ĐANG CHẠY" : "xong").Append(cur != null ? " — bộ " + cur.name : "").Append(", còn " + queue.Count + " bộ\n");
        foreach (var p in progress)
            sb.Append(p).Append('\n');
        if (Done)
            foreach (var s in summary)
                sb.Append(s).Append('\n');
        return sb.ToString();
    }

    static void Add(string n)
    {
        queue.Add(Make(n));
    }

    // ---------------------------------------------------------------- vòng

    static void Pump()
    {
        try
        {
            if (cur == null)
            {
                if (queue.Count == 0)
                {
                    Finish(null);
                    return;
                }
                cur = queue[0];
                queue.RemoveAt(0);
                Begin(cur);
                return;
            }
            if (cur.done == null || cur.done())
            {
                var r = cur.finish != null ? cur.finish() : new Result { total = 0 };
                Record(cur, r, EditorApplication.timeSinceStartup - curStart);
                cur = null;
                return;
            }
            if (EditorApplication.timeSinceStartup - curStart > cur.timeoutSec)
            {
                Record(cur, new Result { total = 1, text = "TIMEOUT " + cur.timeoutSec + " s", note = "TIMEOUT" }, cur.timeoutSec);
                cur = null;
                queue.Clear();   // probe còn hook ⇒ không chạy tiếp bộ khác (hash sẽ sai)
                Finish("TIMEOUT");
            }
        }
        catch (Exception e)
        {
            Record(cur ?? new SuiteDef { name = "?" }, new Result { total = 1, text = "EXCEPTION: " + e, note = "EXCEPTION" }, 0);
            cur = null;
            queue.Clear();
            Finish("EXCEPTION");
        }
    }

    static void Begin(SuiteDef d)
    {
        curStart = EditorApplication.timeSinceStartup;
        string why = Precondition(d);
        if (why == null)
        {
            Application.targetFrameRate = d.fps > 0 ? d.fps : fpsParam;
            Log(d.name + " bắt đầu @" + Application.targetFrameRate);
            try
            {
                why = d.start();
            }
            catch (Exception e)
            {
                why = "EXCEPTION start: " + e.Message;
            }
        }
        if (!string.IsNullOrEmpty(why))
        {
            Record(d, new Result { skip = true, text = "BỎ QUA: " + why, note = "SKIP: " + why }, 0);
            cur = null;
        }
    }

    static string Precondition(SuiteDef d)
    {
        var w = StageWorld.Current;
        switch (d.session)
        {
            case "arena":
                if (w == null || !(w.Player is SimonController) || GameManager.I != null)
                    return "cần Play scene Arena, Player = Simon (CVGP.ArenaUseDummy = false)";
                break;
            case "core":
                if (w == null || !(w.Player is ArenaDummy) || GameManager.I != null)
                    return "cần Play scene Arena với CVGP.ArenaUseDummy = true (Player = ArenaDummy)";
                break;
            case "main":
                if (GameManager.I == null)
                    return "cần Play scene Main";
                break;
        }
        return null;
    }

    static void Record(SuiteDef d, Result r, double sec)
    {
        string exp;
        EXPECTED.TryGetValue(d.name, out exp);
        string ok;
        if (r.skip)
        {
            ok = "SKIP";
            skips++;
        }
        else if (r.note == "TIMEOUT" || r.note == "EXCEPTION")
        {
            ok = r.note;
            failed++;
        }
        else
        {
            bool passAll = r.pass == r.total;
            bool hashOk = exp == null || r.hash == exp;
            ok = passAll && hashOk ? "KHỚP" : (passAll ? "LỆCH HASH" : "FAIL");
            if (!(passAll && hashOk))
                mismatches++;
        }
        string line = d.name + " | " + (r.skip ? "-" : r.pass + "/" + r.total) + " | " + r.hash + " | " + (exp ?? "-") + " | " + ok + " | " + sec.ToString("0") + " s"
                      + (string.IsNullOrEmpty(r.note) || r.skip ? "" : " | " + r.note);
        summary.Add(line);
        File.WriteAllText(outDir + "/suites/" + d.name + ".txt", line + "\n" + r.text + "\n", new UTF8Encoding(false));
        Log(d.name + " xong sau " + sec.ToString("0") + " s: " + line);
    }

    static void Finish(string abort)
    {
        EditorApplication.update -= Pump;
        Application.targetFrameRate = prevFps;
        QualitySettings.vSyncCount = prevVSync;
        Application.runInBackground = prevBg;
        Running = false;
        Done = true;
        var sb = new StringBuilder();
        sb.Append("bộ | đạt/tổng | hash | hash chuẩn | khớp? | thời gian\n");
        foreach (var s in summary)
            sb.Append(s).Append('\n');
        double total = EditorApplication.timeSinceStartup - allStart;
        sb.Append("TỔNG: ").Append(summary.Count).Append(" bộ, lệch/FAIL ").Append(mismatches).Append(", lỗi ").Append(failed).Append(", bỏ qua ").Append(skips)
          .Append(", ").Append(total.ToString("0")).Append(" s").Append(abort != null ? " — DỪNG: " + abort : "").Append('\n');
        File.WriteAllText(outDir + "/suites/SUMMARY.txt", sb.ToString(), new UTF8Encoding(false));
        string verdict = abort != null ? "ABORT " + abort : (mismatches + failed == 0 ? (skips > 0 ? "OK (bỏ qua " + skips + ")" : "OK") : "LỆCH " + (mismatches + failed));
        File.WriteAllText(outDir + "/suites/ALL.done", verdict + "\n", new UTF8Encoding(false));
        Log("XONG " + verdict + " — " + total.ToString("0") + " s");
    }

    static void Log(string s)
    {
        progress.Add(DateTime.Now.ToString("HH:mm:ss") + " " + s);
        if (outDir != null)
            File.AppendAllText(outDir + "/suites/progress.txt", progress[progress.Count - 1] + "\n", new UTF8Encoding(false));
    }

    // ---------------------------------------------------------------- định nghĩa từng bộ

    static readonly Regex failLine = new Regex(@"(?m)^\s+FAIL ");

    /// <summary>Đạt/tổng theo kịch bản: kịch bản "hỏng" = báo cáo có dòng FAIL.</summary>
    static Result FromSuite(IList<string> reports, int fails, uint hash, string text)
    {
        int bad = 0;
        foreach (var r in reports)
            if (failLine.IsMatch(r))
                bad++;
        if (fails > 0 && bad == 0)
            bad = 1;
        return new Result { total = reports.Count, pass = reports.Count - bad, hash = hash.ToString("x8"), text = text };
    }

    static SuiteDef Make(string name)
    {
        switch (name)
        {
            case "long":
                return new SuiteDef
                {
                    name = name, session = "arena",
                    start = () => RegressionProbe.RunLong(2, true) > 0 ? null : "RunLong lỗi",
                    done = () => RegressionProbe.LongDone,
                    finish = () => new Result
                    {
                        total = 1, pass = RegressionProbe.Recorded == RegressionProbe.Target ? 1 : 0,   // replay dài CÓ 1 lần rơi hố (chết 1 — baseline)
                        hash = RegressionProbe.Hash.ToString("x8"),
                        text = RegressionProbe.LongReport() + "\n" + RegressionProbe.PerfReport() + "\nFirstDiff so lần trước: " + RegressionProbe.FirstDiff()
                    }
                };
            case "simon":
                return new SuiteDef
                {
                    name = name, session = "arena",
                    start = () => { SimonProbe.RunSuite(SimonProbe.ScenarioNames()); return null; },
                    done = () => SimonProbe.SuiteDone,
                    finish = () => FromSuite(SimonProbe.SuiteReports, SimonProbe.SuiteFails, SimonProbe.SuiteHash, SimonProbe.SuiteReport())
                };
            case "attack":
                return new SuiteDef
                {
                    name = name, session = "arena",
                    start = () => { AttackProbe.RunSuite(AttackProbe.ScenarioNames()); return null; },
                    done = () => AttackProbe.SuiteDone,
                    finish = () => FromSuite(AttackProbe.SuiteReports, AttackProbe.SuiteFails, AttackProbe.SuiteHash, AttackProbe.SuiteReport())
                };
            case "item":
                return new SuiteDef
                {
                    name = name, session = "arena",
                    start = () => { ItemProbe.RunSuite(ItemProbe.ScenarioNames()); return null; },
                    done = () => ItemProbe.SuiteDone,
                    finish = () => FromSuite(ItemProbe.SuiteReports, ItemProbe.SuiteFails, ItemProbe.SuiteHash, ItemProbe.SuiteReport())
                };
            case "enemy":
                return new SuiteDef
                {
                    name = name, session = "arena",
                    start = () => { EnemyProbe.ShotDir = null; EnemyProbe.RunSuite(EnemyProbe.ScenarioNames()); return null; },
                    done = () => EnemyProbe.SuiteDone,
                    finish = () => FromSuite(EnemyProbe.SuiteReports, EnemyProbe.SuiteFails, EnemyProbe.SuiteHash, EnemyProbe.SuiteReport())
                };
            case "boss":
                return new SuiteDef
                {
                    name = name, session = "arena",
                    start = () => { BossProbe.ShotDir = null; BossProbe.RunSuite(BossProbe.ScenarioNames()); return null; },
                    done = () => BossProbe.SuiteDone,
                    finish = () => FromSuite(BossProbe.SuiteReports, BossProbe.SuiteFails, BossProbe.SuiteHash, BossProbe.SuiteReport())
                };
            case "b23":
                return new SuiteDef
                {
                    name = name, session = "arena",
                    start = () => { EnemyB23Probe.ShotDir = null; EnemyB23Probe.RunSuite(EnemyB23Probe.ScenarioNames()); return null; },
                    done = () => EnemyB23Probe.SuiteDone,
                    finish = () => FromSuite(EnemyB23Probe.SuiteReports, EnemyB23Probe.SuiteFails, EnemyB23Probe.SuiteHash, EnemyB23Probe.SuiteReport())
                };
            case "b46":
                return new SuiteDef
                {
                    name = name, session = "arena",
                    start = () => { EnemyB46Probe.ShotDir = null; EnemyB46Probe.RunSuite(EnemyB46Probe.ScenarioNames()); return null; },
                    done = () => EnemyB46Probe.SuiteDone,
                    finish = () => FromSuite(EnemyB46Probe.SuiteReports, EnemyB46Probe.SuiteFails, EnemyB46Probe.SuiteHash, EnemyB46Probe.SuiteReport())
                };
            case "b24":
                return new SuiteDef
                {
                    name = name, session = "arena",
                    start = () => { BossB24Probe.ShotDir = null; BossB24Probe.RunSuite(BossB24Probe.ScenarioNames()); return null; },
                    done = () => BossB24Probe.SuiteDone,
                    finish = () => FromSuite(BossB24Probe.SuiteReports, BossB24Probe.SuiteFails, BossB24Probe.SuiteHash, BossB24Probe.SuiteReport())
                };
            case "b56":
                return new SuiteDef
                {
                    name = name, session = "arena",
                    start = () => { BossB56Probe.ShotDir = null; BossB56Probe.RunSuite(BossB56Probe.ScenarioNames()); return null; },
                    done = () => BossB56Probe.SuiteDone,
                    finish = () => FromSuite(BossB56Probe.SuiteReports, BossB56Probe.SuiteFails, BossB56Probe.SuiteHash, BossB56Probe.SuiteReport())
                };
            case "holy":
                return new SuiteDef
                {
                    name = name, session = "arena",
                    start = () => { HolyPierceProbe.RunSuite(HolyPierceProbe.ScenarioNames()); return null; },
                    done = () => HolyPierceProbe.SuiteDone,
                    finish = () => FromSuite(HolyPierceProbe.SuiteReports, HolyPierceProbe.SuiteFails, HolyPierceProbe.SuiteHash, HolyPierceProbe.SuiteReport())
                };
            case "mech":
                return new SuiteDef
                {
                    name = name, session = "arena",
                    start = () => { Map2MechProbe.ShotDir = null; return Map2MechProbe.RunSuite(Map2MechProbe.ScenarioNames()) > 0 ? null : "Map2MechProbe không chạy (thiếu section Block 2?)"; },
                    done = () => Map2MechProbe.SuiteDone,
                    finish = () => FromSuite(Map2MechProbe.SuiteReports, Map2MechProbe.SuiteFails, Map2MechProbe.SuiteHash, Map2MechProbe.SuiteReport())
                };
            case "rules":
                {
                    string rep = null;
                    return new SuiteDef
                    {
                        name = name, session = "arena",
                        start = () => { rep = RulesProbe.RunSuite(RulesProbe.ScenarioNames()); return null; },   // đồng bộ (tự bước NesClock)
                        done = () => rep != null,
                        finish = () => new Result { total = RulesProbe.Pass + RulesProbe.Fail, pass = RulesProbe.Pass, hash = RulesProbe.SuiteHash.ToString("x8"), text = rep }
                    };
                }
            case "core":
                return new SuiteDef
                {
                    name = name, session = "core",
                    start = () => { CvgpCoreProbe.RunSuite(CORE_NAMES); return null; },
                    done = () => CvgpCoreProbe.SuiteDone,
                    finish = () => FromSuite(CvgpCoreProbe.SuiteReports, CvgpCoreProbe.SuiteFails, CvgpCoreProbe.SuiteHash, CvgpCoreProbe.SuiteReport())
                };
            case "gallery":
                return new SuiteDef
                {
                    name = name, session = "arena", fps = 240, timeoutSec = 2400,
                    start = () => CharacterAcceptProbe.RunGallery(CharacterAcceptProbe.TypeNames(), "", "") > 0 ? null : "RunGallery lỗi",
                    done = () => CharacterAcceptProbe.GalleryDone,
                    finish = () =>
                    {
                        string rep = CharacterAcceptProbe.GalleryReport();
                        var m = Regex.Match(rep, @"hash bộ ([0-9a-f]{8})");
                        int n = CharacterAcceptProbe.GalleryLines.Count;
                        return new Result { total = n, pass = n, hash = m.Success ? m.Groups[1].Value : "?", text = rep + "\n" + CharacterAcceptProbe.HashList() };
                    }
                };
            case "flow":
            case "flow30":
            case "flow144":
                {
                    int f = name == "flow30" ? 30 : name == "flow144" ? 144 : 0;
                    return new SuiteDef
                    {
                        name = name, session = "main", fps = f, timeoutSec = 900,
                        start = () =>
                        {
                            string r = Map1AcceptProbe.StartFlow(f > 0 ? f : fpsParam, outDir + "/shots/" + name);
                            return r.StartsWith("flow bắt đầu") ? null : r;
                        },
                        done = () => Map1AcceptProbe.Finished && !Map1AcceptProbe.Running,
                        finish = () =>
                        {
                            Map1AcceptProbe.SaveReport(outDir + "/suites/" + name + "_report.txt");
                            var res = new Result
                            {
                                total = 1, pass = string.IsNullOrEmpty(Map1AcceptProbe.Error) && Map1AcceptProbe.ConsoleErrors.Count == 0 ? 1 : 0,
                                hash = Map1AcceptProbe.Hash.ToString("x8"),
                                text = Map1AcceptProbe.Report() + "\nGameObject: " + string.Join(" ", Map1AcceptProbe.ObjectCounts.ConvertAll(x => x.ToString()).ToArray())
                            };
                            H8(name);
                            return res;
                        }
                    };
                }
            case "spam1": return Spam(name, 1, 3600, 16001, false, null, 0, 0, 0, 0);
            case "spam2": return Spam(name, 2, 3600, 16002, true, null, 0, 0, 0, 0);
            case "spam3": return Spam(name, 3, 4800, 16003, true, null, 0, 0, 1, 2);
            case "spam4": return Spam(name, 2, 4800, 16004, true, "CryptB", 240, 96, 1, 0);
        }
        return null;
    }

    static SuiteDef Spam(string name, int stage, int ticks, int seed, bool withStart, string warp, int wx, int wy, int lives, int hp)
    {
        return new SuiteDef
        {
            name = name, session = "main", fps = Mathf.Max(fpsParam, 144), timeoutSec = 900,
            start = () =>
            {
                string r = Map1AcceptProbe.StartSpam(stage, ticks, seed, withStart, warp, wx, wy, lives, hp);
                return r == "spam bắt đầu" ? null : r;
            },
            done = () => Map1AcceptProbe.Finished && !Map1AcceptProbe.Running,
            finish = () => new Result
            {
                total = 1, pass = string.IsNullOrEmpty(Map1AcceptProbe.Error) && Map1AcceptProbe.ConsoleErrors.Count == 0 ? 1 : 0,
                hash = "-", text = Map1AcceptProbe.Report(), note = "không có hash — đạt = 0 lỗi console, kết thúc đúng"
            }
        };
    }

    /// <summary>h8: đọc 5 mốc luồng CVTI từ Events của lần flow vừa xong ⇒ ghi suites/h8.txt (+ dòng SUMMARY).</summary>
    static void H8(string flowName)
    {
        string[] keys = { "Title: Start", "Title→Black", "Black→Intro", "Intro→Black", "Black→Stage1" };
        var ls = new List<string>();
        foreach (var k in keys)
        {
            string l = "?";
            foreach (var e in Map1AcceptProbe.Events)
                if (e.Contains("\t" + k))
                {
                    var m = Regex.Match(e, @"\tL(\d+)\t");
                    if (m.Success)
                    {
                        l = m.Groups[1].Value;
                        break;
                    }
                }
            ls.Add(l);
        }
        string got = string.Join("/", ls.ToArray());
        string exp = EXPECTED["h8"];
        summary.Add("h8(" + flowName + ") | " + (got == exp ? "5/5" : "0/5") + " | " + got + " | " + exp + " | " + (got == exp ? "KHỚP" : "LỆCH") + " | 0 s");
        if (got != exp)
            mismatches++;
        File.WriteAllText(outDir + "/suites/h8_" + flowName + ".txt", "H8 5 mốc (Start, đen, Intro, đen, Stage) = " + got + " (chuẩn " + exp + ")\n", new UTF8Encoding(false));
    }
}
#endif
