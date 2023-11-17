public class IntroState : BaseState
{
    public override eStateType stateType => eStateType.intro;

    public override void OnEnterState()
    {
        character.PlayAnim(config);
    }

#if UNITY_EDITOR
    public override void CheckNextState()
    {
        if (character.controller.isAttack)
        {

        }
        else if (character.controller.isJump)
        {

        }
        else if (character.controller.moveX != 0)
        {
            character.SetDirection(character.controller.moveX > 0 ? eDirectionType.Right : eDirectionType.Left);
            character.SetState(character.walkState);
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
#endif
}
