/// <summary>
/// Lớp nền state Simon (CVGP 08) — giữ phong cách code cũ (git 2bfd90b^ Character/State): mỗi state 1 lớp với đúng 4 hàm
/// OnEnterState / OnExecuteState / CheckNextState / OnExitState; `owner` = SimonController. Execute = hành động của tick,
/// CheckNextState = quyết định state kế (đổi trong Check ⇒ state mới chạy nối cùng tick, SimonController chặn đi 2 lần/tick).
/// </summary>
public abstract class SimonState : BaseState<SimonController>
{
    protected void SetState(eSimonState s)
    {
        owner.Fsm.SetState(s);
    }
}
