using System.Collections.Generic;
using afsdk;
using UnityEngine;

/// <summary>
/// State machine màn hình (CVTI_MEMORY §4.3). Mọi màn nằm trong Main.unity, mỗi màn 1 GameScreen; chuyển cảnh = cắt cứng.
/// GoTo(next, blackFrames) gọi ở tick T: tắt màn cũ ngay (camera xoá đen ⇒ màn đen), bật màn mới ở tick T + blackFrames
/// ⇒ đúng blackFrames frame đen. Đang đen thì không màn nào nhận tick (Start bị bỏ qua).
/// </summary>
public class GameManager : ManualSingletonMono<GameManager>
{
    [Tooltip("Log mốc chuyển cảnh theo frame (cũng bật được bằng menu CVTI/Log Flow Frames).")]
    [SerializeField] private bool logFlow;

    [Tooltip("Bảng chỉ số gốc (CVGP 04) — Assets/Data/Config/GameConfig.asset, dựng bằng menu CVGP/Import Config.")]
    [SerializeField] private GameConfig gameConfig;

    private readonly Dictionary<eGameScreen, GameScreen> screens = new Dictionary<eGameScreen, GameScreen>();

    private GameScreen current;
    private int enterFrame;
    private bool transitioning;
    private eGameScreen pendingNext;
    private int blackLeft;

    public eGameScreen State { get; private set; }
    public bool IsTransitioning => transitioning;
    public GameConfig Config => gameConfig;

    protected override void Awake()
    {
        base.Awake();
        if (gameConfig == null)
            Debug.LogError("[GameManager] Chưa gán GameConfig (chạy CVGP/Import Config rồi gán vào prefab GameManager)");
        GameConfig.SetCurrent(gameConfig);
#if UNITY_EDITOR
        CvtiDebug.LogFlow = logFlow || UnityEditor.EditorPrefs.GetBool(CvtiDebug.PREF_KEY, false);
#else
        CvtiDebug.LogFlow = logFlow;
#endif
        State = eGameScreen.Boot;

        foreach (var s in FindObjectsOfType<GameScreen>(true))
        {
            if (screens.ContainsKey(s.Id))
            {
                Debug.LogError("[GameManager] Trùng màn " + s.Id + ": " + s.name);
                continue;
            }
            screens.Add(s.Id, s);
            s.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        NesClock.OnSystemTick += HandleTick;
    }

    private void OnDisable()
    {
        NesClock.OnSystemTick -= HandleTick;
    }

    /// <summary>Chuyển sang màn next sau blackFrames frame đen (0 = cắt thẳng). Trả false nếu đang chuyển cảnh.</summary>
    public bool GoTo(eGameScreen next, int blackFrames)
    {
        if (transitioning)
            return false;

        var from = State;
        if (current != null)
        {
            current.OnExit();
            current.gameObject.SetActive(false);
            current = null;
        }

        if (blackFrames <= 0)
        {
            CvtiDebug.Log(from + "→" + next);
            Enter(next);
            return true;
        }

        CvtiDebug.Log(from + "→Black (" + blackFrames + "f)");
        transitioning = true;
        pendingNext = next;
        blackLeft = blackFrames;
        return true;
    }

    private void HandleTick(int frame)
    {
        // Input chốt trước luồng màn hình (CVGP_MEMORY §4: thứ tự tick).
        if (NesPad.I != null)
            NesPad.I.Tick();

        if (State == eGameScreen.Boot && !transitioning)
        {
            GoTo(eGameScreen.Title, 0);
            return;
        }

        if (transitioning)
        {
            if (--blackLeft <= 0)
            {
                transitioning = false;
                CvtiDebug.Log("Black→" + pendingNext);
                Enter(pendingNext);
            }
            return;
        }

        if (current != null)
            current.OnTick(frame - enterFrame);
    }

    private void Enter(eGameScreen next)
    {
        State = next;
        enterFrame = NesClock.Frame;
        if (!screens.TryGetValue(next, out current))
        {
            Debug.LogError("[GameManager] Không có màn " + next + " trong scene");
            return;
        }
        current.gameObject.SetActive(true);
        current.OnEnter();
    }
}
