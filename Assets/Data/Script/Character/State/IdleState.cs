public class IdleState : BaseState
{
    public override void OnEnterState()
    {
        character.SetDirection(eDirectionType.None);
        character.PlayAnim(config);
    }

    public override void CheckNextState()
    {
        if (character.controller.isAttack)
        {

        }
        else if (character.controller.isJump)
            character.physicController.Jump();
        else if (character.controller.moveX != 0)
            SetState(eStateType.walk);

        if (character.controller.moveY != 0)
        {
            character.SetDirection(character.curDirectionV);
            if (character.directionTypeV is eDirectionType.Down)
                SetState(eStateType.duck);
        }
    }
}
