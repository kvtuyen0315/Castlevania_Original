public class IntroState : BaseState
{
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
            character.SetDirection(character.curDirectionH);
            SetState(eStateType.walk);
        }
        else if (character.controller.moveY != 0)
        {
            character.SetDirection(character.curDirectionV);
            if (character.directionTypeV is eDirectionType.Down)
                SetState(eStateType.duck);
        }
    } 
#endif
}
