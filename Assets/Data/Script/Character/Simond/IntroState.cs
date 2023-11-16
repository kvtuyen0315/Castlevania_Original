public class IntroState : BaseState
{
    public override eStateType stateType => eStateType.intro;

    public override void OnEnterState()
    {
        character.PlayAnim(config);
    }
}
