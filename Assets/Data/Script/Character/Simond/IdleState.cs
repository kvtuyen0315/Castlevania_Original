public class IdleState : BaseState
{
    public override eStateType stateType => eStateType.idle;

    public override void OnEnterState()
    {
        character.PlayAnim(config);
    }
}
