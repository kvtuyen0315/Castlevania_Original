using System;
using System.Collections.Generic;
using afsdk;
using UnityEngine;

/// <summary>
/// Tay cầm NES ảo — lớp DUY NHẤT đọc Input.* (CVGP_PLAN §6). Bàn phím + gamepad Xbox (XInput) theo InputBindings.asset.
/// Như NES đọc tay cầm 1 lần/frame: Update chỉ CHỐT cạnh nhấn (GetKeyDown + cạnh lên của trục) để không lọt khi fps cao/thấp;
/// Tick() (GameManager gọi đầu OnSystemTick, trước mọi logic) tính Held/Pressed/Released cho cả tick.
///   Held     = đang giữ ∨ vừa chốt (lần nhấn nhanh giữa 2 tick vẫn giữ đúng 1 tick)
///   Pressed  = chốt ∨ (Held ∧ tick trước chưa giữ)      Released = ¬Held ∧ tick trước giữ
/// Trái+Phải cùng giữ ⇒ bỏ cả hai (tay NES không bấm được), Lên+Xuống tương tự.
/// Tiêm (Inject/InjectPress) và replay (PlayReplay — ghi đè input thật) đi qua cùng đường; StartRecord ghi bitmask mỗi tick.
/// </summary>
[DefaultExecutionOrder(-200)]
public class NesPad : ManualSingletonMono<NesPad>
{
    public const int BUTTON_COUNT = 9;
    public const int BUTTON_MASK = (1 << BUTTON_COUNT) - 1;
    /// <summary>Bit "nhấn lại khi đang giữ" trong giá trị replay nằm ở bit 16+.</summary>
    public const int REPRESS_SHIFT = 16;

    const int BIT_UP = 1 << (int)eNesButton.Up;
    const int BIT_DOWN = 1 << (int)eNesButton.Down;
    const int BIT_LEFT = 1 << (int)eNesButton.Left;
    const int BIT_RIGHT = 1 << (int)eNesButton.Right;

    [SerializeField] private InputBindings bindings;

    public int HeldMask { get; private set; }
    public int PressedMask { get; private set; }
    public int ReleasedMask { get; private set; }
    /// <summary>NesClock.Frame của lần Tick gần nhất.</summary>
    public int LastTickFrame { get; private set; } = -1;

    public bool IsReplaying => replay != null;
    public bool IsRecording => record != null;
    public int RecordedTicks => record != null ? record.Count : 0;

    private KeyCode[][] keys;
    private string[] axisNames;      // 0 dpadX, 1 dpadY, 2 stickX, 3 stickY (null = không dùng/không có trong InputManager)
    private int latchedMask;         // cạnh nhấn chốt trong Update kể từ tick trước
    private int lastUpdateRaw;       // raw ở Update trước — bắt cạnh lên của trục
    private readonly int[] injectLeft = new int[BUTTON_COUNT];
    private List<int> replay;
    private int replayIndex;
    private List<int> record;
    private bool testRawActive;
    private int testRaw;

#if UNITY_EDITOR
    public const string PREF_OVERLAY = "CVGP.InputOverlay";
    /// <summary>Debug Overlay (menu CVGP/Input/Debug Overlay) — chỉ Editor, không vào build.</summary>
    public static bool ShowOverlay { get; set; }
#endif

    protected override void Awake()
    {
        base.Awake();
        BuildTables();
#if UNITY_EDITOR
        ShowOverlay = UnityEditor.EditorPrefs.GetBool(PREF_OVERLAY, false);
#endif
    }

    private void BuildTables()
    {
        keys = new KeyCode[BUTTON_COUNT][];
        axisNames = new string[4];
        if (bindings == null)
        {
            Debug.LogError("[NesPad] Thiếu InputBindings (Assets/Data/Config/InputBindings.asset) — không đọc được phím");
            for (int i = 0; i < BUTTON_COUNT; i++)
                keys[i] = new KeyCode[0];
            return;
        }
        for (int i = 0; i < BUTTON_COUNT; i++)
            keys[i] = bindings.GetKeys((eNesButton)i);
        axisNames[0] = CheckAxis(bindings.dpadAxisX);
        axisNames[1] = CheckAxis(bindings.dpadAxisY);
        axisNames[2] = CheckAxis(bindings.stickAxisX);
        axisNames[3] = CheckAxis(bindings.stickAxisY);
    }

    static string CheckAxis(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;
        try
        {
            Input.GetAxisRaw(name);
            return name;
        }
        catch (ArgumentException)
        {
            Debug.LogError("[NesPad] Trục '" + name + "' không có trong ProjectSettings/InputManager.asset — bỏ qua");
            return null;
        }
    }

    private void Update()
    {
        int keyDown;
        int raw = ReadRaw(out keyDown);
        latchedMask |= keyDown | (raw & ~lastUpdateRaw);
        lastUpdateRaw = raw;
    }

    /// <summary>Chốt input cho tick hiện tại — gọi đúng 1 lần/tick, đầu OnSystemTick (GameManager.HandleTick).</summary>
    public void Tick()
    {
        if (LastTickFrame == NesClock.Frame)
            return;
        LastTickFrame = NesClock.Frame;

        int prev = HeldMask;
        int held;
        int repress;
        if (replay != null)
        {
            int v = replay[replayIndex++];
            held = v & BUTTON_MASK;
            repress = (v >> REPRESS_SHIFT) & BUTTON_MASK & held;
            if (replayIndex >= replay.Count)
                replay = null;
        }
        else
        {
            int unused;
            int latched = latchedMask;
            held = ReadRaw(out unused) | latched;
            for (int i = 0; i < BUTTON_COUNT; i++)
            {
                if (injectLeft[i] > 0)
                {
                    held |= 1 << i;
                    injectLeft[i]--;
                }
            }
            held = CancelOpposite(held);
            repress = latched & prev & held;
        }
        latchedMask = 0;

        HeldMask = held;
        PressedMask = (held & ~prev) | repress;
        ReleasedMask = prev & ~held;

        if (record != null)
            record.Add(held | (repress << REPRESS_SHIFT));
    }

    public bool Held(eNesButton b) => (HeldMask & (1 << (int)b)) != 0;
    public bool Pressed(eNesButton b) => (PressedMask & (1 << (int)b)) != 0;
    public bool Released(eNesButton b) => (ReleasedMask & (1 << (int)b)) != 0;

    /// <summary>Hướng 8 ngả của tick: x −1 trái / +1 phải, y +1 lên / −1 xuống (đã bỏ cặp ngược nhau).</summary>
    public Vector2Int Direction
    {
        get
        {
            int x = ((HeldMask & BIT_RIGHT) != 0 ? 1 : 0) - ((HeldMask & BIT_LEFT) != 0 ? 1 : 0);
            int y = ((HeldMask & BIT_UP) != 0 ? 1 : 0) - ((HeldMask & BIT_DOWN) != 0 ? 1 : 0);
            return new Vector2Int(x, y);
        }
    }

    static int CancelOpposite(int mask)
    {
        if ((mask & (BIT_LEFT | BIT_RIGHT)) == (BIT_LEFT | BIT_RIGHT))
            mask &= ~(BIT_LEFT | BIT_RIGHT);
        if ((mask & (BIT_UP | BIT_DOWN)) == (BIT_UP | BIT_DOWN))
            mask &= ~(BIT_UP | BIT_DOWN);
        return mask;
    }

    // ---------- Tiêm / replay / ghi ----------

    /// <summary>Giữ nút b trong `frames` tick, bắt đầu từ Tick kế tiếp (Pressed ở tick đầu, Released ở tick frames+1).</summary>
    public void Inject(eNesButton b, int frames)
    {
        int i = (int)b;
        injectLeft[i] = Mathf.Max(injectLeft[i], frames);
    }

    /// <summary>Giả lập 1 cạnh nhấn y như Update chốt (lần nhấn nhanh ⇒ Pressed + Held đúng 1 tick ở Tick kế tiếp).</summary>
    public void InjectPress(eNesButton b)
    {
        latchedMask |= 1 << (int)b;
    }

    /// <summary>Phát chuỗi RLE (định dạng ở NesReplay) từ Tick kế tiếp; trong lúc phát input thật/tiêm bị bỏ qua.</summary>
    public void PlayReplay(string rle)
    {
        PlayReplay(NesReplay.Decode(rle));
    }

    public void PlayReplay(List<int> ticks)
    {
        replay = ticks != null && ticks.Count > 0 ? ticks : null;
        replayIndex = 0;
        for (int i = 0; i < BUTTON_COUNT; i++)
            injectLeft[i] = 0;
        latchedMask = 0;
    }

    public void StopReplay()
    {
        replay = null;
    }

    /// <summary>Bắt đầu ghi từ Tick kế tiếp.</summary>
    public void StartRecord()
    {
        record = new List<int>();
    }

    /// <summary>Dừng ghi, trả danh sách giá trị từng tick (NesReplay.Encode để ra chuỗi RLE).</summary>
    public List<int> StopRecord()
    {
        var r = record ?? new List<int>();
        record = null;
        return r;
    }

    /// <summary>Kiểm thử chốt cạnh: thay "phần cứng" bằng bitmask raw (Update vẫn chốt như thật). ClearTestRaw để trả lại.</summary>
    public void SetTestRaw(int mask)
    {
        testRawActive = true;
        testRaw = mask & BUTTON_MASK;
    }

    public void ClearTestRaw()
    {
        testRawActive = false;
        testRaw = 0;
    }

    // ---------- Đọc phần cứng ----------

    private int ReadRaw(out int keyDown)
    {
        keyDown = 0;
        if (testRawActive)
            return testRaw;
        if (keys == null)
            return 0;

        int raw = 0;
        for (int i = 0; i < BUTTON_COUNT; i++)
        {
            var list = keys[i];
            for (int k = 0; k < list.Length; k++)
            {
                if (Input.GetKey(list[k]))
                    raw |= 1 << i;
                if (Input.GetKeyDown(list[k]))
                    keyDown |= 1 << i;
            }
        }
        if (bindings != null)
        {
            raw |= AxisBits(axisNames[0], axisNames[1], bindings.dpadThreshold);
            raw |= AxisBits(axisNames[2], axisNames[3], bindings.stickThreshold);
        }
        return raw;
    }

    static int AxisBits(string axisX, string axisY, float threshold)
    {
        int bits = 0;
        if (axisX != null)
        {
            float x = Input.GetAxisRaw(axisX);
            if (x >= threshold) bits |= BIT_RIGHT;
            else if (x <= -threshold) bits |= BIT_LEFT;
        }
        if (axisY != null)
        {
            float y = Input.GetAxisRaw(axisY);
            if (y >= threshold) bits |= BIT_UP;
            else if (y <= -threshold) bits |= BIT_DOWN;
        }
        return bits;
    }

    /// <summary>Giá trị thô 4 trục (debug overlay / kiểm tay cầm). Trục không có ⇒ 0.</summary>
    public float GetAxisDebug(int index)
    {
        return axisNames != null && axisNames[index] != null ? Input.GetAxisRaw(axisNames[index]) : 0f;
    }

    public static string[] GetJoystickNamesDebug()
    {
        return Input.GetJoystickNames();
    }

#if UNITY_EDITOR
    private GUIStyle overlayStyle;

    private void OnGUI()
    {
        if (!ShowOverlay)
            return;
        if (overlayStyle == null)
            overlayStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, richText = true };

        var sb = new System.Text.StringBuilder();
        sb.Append("<b>NesPad</b>  tick ").Append(LastTickFrame).Append('\n');
        for (int i = 0; i < BUTTON_COUNT; i++)
        {
            bool h = (HeldMask & (1 << i)) != 0;
            sb.Append(h ? "<color=#ffff40>■ " : "<color=#808080>□ ").Append((eNesButton)i).Append("</color>\n");
        }
        sb.Append(string.Format("D-pad {0:0.00} {1:0.00}\nStick {2:0.00} {3:0.00}\n",
            GetAxisDebug(0), GetAxisDebug(1), GetAxisDebug(2), GetAxisDebug(3)));
        var names = Input.GetJoystickNames();
        sb.Append("Tay cầm: ").Append(names.Length == 0 ? "(không có)" : string.Join(", ", names));
        if (IsReplaying) sb.Append("\n<color=#40ff40>REPLAY</color>");
        if (IsRecording) sb.Append("\n<color=#ff4040>REC ").Append(RecordedTicks).Append("</color>");

        var r = new Rect(12, 12, 420, 520);
        GUI.color = new Color(0, 0, 0, 0.6f);
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(r.x + 10, r.y + 6, r.width - 20, r.height - 12), sb.ToString(), overlayStyle);
    }
#endif
}
