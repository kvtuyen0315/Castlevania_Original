public class DuckState : BaseState
{
    public override eStateType stateType => eStateType.duck;

    public override void OnEnterState()
    {
        character.PlayAnim(config);
    }

    public override void CheckNextState()
    {
        if (character.controller.moveY >= 0)
        {
            character.SetState(character.idleState);
            return;
        }

        if (character.controller.isAttack)
        {

        }
        else if (character.controller.isJump)
        {

        }
    }
}
