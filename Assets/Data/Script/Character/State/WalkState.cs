public class WalkState : BaseState
{
    public override eStateType stateType => eStateType.walk;

    public override void OnEnterState()
    {
        character.PlayAnim(config);
    }

    public override void CheckNextState()
    {
        if (character.controller.moveX is 0)
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
        else if (character.controller.moveY != 0)
        {
            character.SetDirection(character.controller.moveY > 0 ? eDirectionType.Up : eDirectionType.Down);
            if (character.directionTypeV is eDirectionType.Down)
            {
                character.SetState(character.duckState);
            }
        }
    }
}
