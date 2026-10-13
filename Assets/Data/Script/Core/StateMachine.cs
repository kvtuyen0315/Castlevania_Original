using System;
using Unity.Collections.LowLevel.Unsafe;

/// <summary>
/// 1 state = 1 lớp (giữ phong cách code cũ của người dùng — git 2bfd90b^ Character/State/BaseState.cs):
/// OnEnterState khi vào, mỗi tick OnExecuteState rồi CheckNextState (đổi state ở đây), OnExitState khi rời.
/// `owner` = thực thể sở hữu (như `character` của code cũ). StateFrame = số tick đã Execute trong state hiện tại
/// (OnEnterState thấy 0, lần Execute đầu thấy 1) ⇒ "chờ N tick" = `if (StateFrame >= N)` trong CheckNextState.
/// Lớp state của 1 nhân vật thường kế thừa qua 1 lớp nền riêng có hàm SetState(eXxxState) tiện dụng (xem ArenaDummy).
/// </summary>
public abstract class BaseState<TOwner> where TOwner : class
{
    public TOwner owner { get; internal set; }
    internal IStateClock clock;

    /// <summary>Số tick đã Execute trong state hiện tại.</summary>
    protected int StateFrame => clock != null ? clock.StateFrame : 0;

    public virtual void OnEnterState()
    {
    }

    public virtual void OnExecuteState()
    {
    }

    public virtual void CheckNextState()
    {
    }

    public virtual void OnExitState()
    {
    }
}

internal interface IStateClock
{
    int StateFrame { get; }
}

/// <summary>
/// Máy trạng thái theo tick (CVGP 07). Thực thể gọi Tick() đúng 1 lần mỗi tick của nó (trong Entity.OnTick — StageWorld
/// gọi theo thứ tự cố định ⇒ tất định). SetState gọi được bất cứ lúc nào: trong Execute/Check ⇒ state mới chạy nối ngay
/// cùng tick (xem Tick); từ ngoài (vd trúng đòn ở bước giải va chạm) ⇒ state mới Execute từ tick sau.
/// Không cấp phát mỗi tick: state lưu mảng theo giá trị enum (UnsafeUtility.EnumToInt — không boxing).
/// </summary>
public class StateMachine<TOwner, TState> : IStateClock
    where TOwner : class
    where TState : struct, Enum, IConvertible
{
    private readonly TOwner owner;
    private readonly string debugName;
    private BaseState<TOwner>[] states = new BaseState<TOwner>[8];
    private bool hasState;

    public TState CurState { get; private set; }
    public TState PrevState { get; private set; }
    public BaseState<TOwner> Current { get; private set; }
    public int StateFrame { get; private set; }
    /// <summary>Tổng số lần đổi state (debug/kiểm thử).</summary>
    public int ChangeCount { get; private set; }

    public StateMachine(TOwner owner, string debugName)
    {
        this.owner = owner;
        this.debugName = debugName;
    }

    public bool HasState => hasState;

    public void Add(TState id, BaseState<TOwner> state)
    {
        int i = UnsafeUtility.EnumToInt(id);
        if (i < 0)
            throw new ArgumentOutOfRangeException("id", debugName + ": giá trị state âm " + id);
        if (i >= states.Length)
            Array.Resize(ref states, Math.Max(i + 1, states.Length * 2));
        state.owner = owner;
        state.clock = this;
        states[i] = state;
    }

    public BaseState<TOwner> Get(TState id)
    {
        int i = UnsafeUtility.EnumToInt(id);
        return i >= 0 && i < states.Length ? states[i] : null;
    }

    public bool Is(TState id)
    {
        return hasState && UnsafeUtility.EnumToInt(CurState) == UnsafeUtility.EnumToInt(id);
    }

    /// <summary>Rời state cũ (OnExitState), vào state mới (OnEnterState), StateFrame = 0. Đặt lại cùng state cũng chạy Exit/Enter.</summary>
    public void SetState(TState id)
    {
        var next = Get(id);
        if (next == null)
        {
            UnityEngine.Debug.LogError("[StateMachine] " + debugName + ": chưa Add state " + id);
            return;
        }
        if (Current != null)
            Current.OnExitState();
        if (CvgpDebug.LogState)
            CvgpDebug.Log(debugName + ": " + (hasState ? CurState.ToString() : "-") + " → " + id + " (sau " + StateFrame + " tick)");
        PrevState = hasState ? CurState : id;
        CurState = id;
        Current = next;
        hasState = true;
        StateFrame = 0;
        ChangeCount++;
        next.OnEnterState();
    }

    /// <summary>Số lần đổi state tối đa được "chạy nối" trong 1 tick (chặn vòng lặp A→B→A).</summary>
    public const int MAX_CHAIN = 4;

    /// <summary>
    /// 1 tick: StateFrame++, OnExecuteState, rồi CheckNextState. Nếu Execute/Check đổi state ⇒ state MỚI chạy luôn
    /// OnExecuteState (+ Check) trong CÙNG tick (tối đa MAX_CHAIN lần) — như NES đọc input, đổi trạng thái và di chuyển
    /// trong 1 frame (vd Idle + bấm → ⇒ Walk đi 1 px ngay tick đó). SetState gọi từ NGOÀI Tick (combat ở bước 6…)
    /// ⇒ state mới Execute từ tick sau.
    /// </summary>
    public void Tick()
    {
        for (int chain = 0; chain < MAX_CHAIN; chain++)
        {
            var s = Current;
            if (s == null)
                return;
            StateFrame++;
            s.OnExecuteState();
            if (Current != s)
                continue;
            s.CheckNextState();
            if (Current == s)
                return;
        }
    }
}
