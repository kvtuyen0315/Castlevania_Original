public enum eStateType
{
    none = -1,
    ascend,
    dead,
    descend,
    duck,
    hurt,
    idle,
    intro,
    jump,
    walk,
    whipAscend,
    whipDescend,
    whipDuck,
    whipStand,
}

public class BaseState
{
    public BaseCharacter character { get; set; }
    public ConfigAnimationRecord config { get; set; }

    public virtual eStateType stateType => eStateType.none;
    public string nameState => stateType.ToString();

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
