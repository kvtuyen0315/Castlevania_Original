public class WalkState : BaseState
{
    public override eStateType stateType => eStateType.walk;

    public override void OnEnterState()
    {
        character.PlayAnim(config);
    }
}
