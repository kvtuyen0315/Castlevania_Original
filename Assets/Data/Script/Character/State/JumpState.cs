public class JumpState : BaseState
{
    public override eStateType stateType => eStateType.jump;

    public override void OnEnterState()
    {
        character.PlayAnim(config);
    }

    public override void OnExecuteState()
    {
        // AAA - check isGround to stand.
    }

    public override void CheckNextState()
    {
        if (character.controller.isAttack)
        {

        }
        else if (character.controller.moveX != 0)
        {
            character.SetDirection(character.controller.moveX > 0 ? eDirectionType.Right : eDirectionType.Left);
        }
    }
}
