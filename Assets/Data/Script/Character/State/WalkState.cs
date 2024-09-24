public class WalkState : BaseState
{
    public override void OnEnterState()
    {
        base.OnEnterState();
        character.PlayAnim(config);
    }

    public override void CheckNextState()
    {
        base.CheckNextState();

        if (character.controller.moveX is 0)
        {
            SetState(eStateType.idle);
            return;
        }

        if (character.controller.isAttack)
        {

        }
        else if (character.controller.isJump)
        {
            SetState(eStateType.jump);
        }
        else if (character.controller.moveX != 0)
            character.SetDirection(character.curDirectionH);
        else if (character.controller.moveY != 0)
        {
            character.SetDirection(character.curDirectionV);
            if (character.directionTypeV is eDirectionType.Down)
                SetState(eStateType.duck);
        }
    }

    public override void OnExecuteState()
    {
        base.OnExecuteState();

        character.Move(character.curDirectionH);
    }

    public override void OnExitState()
    {
        base.OnExitState();

        character.StopMove();
    }
}
