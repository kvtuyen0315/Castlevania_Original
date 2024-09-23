public enum eStateType
{
    none = -1,
    intro,
    idle,
    walk,
    ascend,
    duck,
    jump,
    hurt,
    dead,
    descend,
    whipAscend,
    whipDescend,
    whipDuck,
    whipStand,
    Length,
}

public class BaseState
{
    public BaseCharacter character { get; set; }
    public ConfigAnimationRecord config { get; set; }
    public eStateType stateType { get; set; } = eStateType.none;

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
