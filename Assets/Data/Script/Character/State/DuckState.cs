public class DuckState : BaseState
{
    public override void OnEnterState()
    {
        character.PlayAnim(config);
    }

    public override void CheckNextState()
    {
        if (character.controller.moveY >= 0)
        {
            SetState(eStateType.idle);
            return;
        }
        else if (character.controller.moveX != 0)
            character.SetDirection(character.curDirectionH);

        if (character.controller.isAttack)
        {

        }
        else if (character.controller.isJump)
        {

        }
    }
}
