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
            character.SetState(character.GetState(eStateType.idle));
            return;
        }
        else if (character.controller.moveX != 0)
            character.SetDirection(character.controller.moveX > 0 ? eDirectionType.Right : eDirectionType.Left);

        if (character.controller.isAttack)
        {

        }
        else if (character.controller.isJump)
        {

        }
    }
}
