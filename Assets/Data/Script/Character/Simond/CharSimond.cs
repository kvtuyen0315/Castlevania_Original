public class CharSimond : BaseCharacter
{
    public override void SetData(InputController controller)
    {
        base.SetData(controller);

        this.controller = controller;

        animationFBF = GetComponent<AnimationFBF>();

        physicController = GetComponent<PhysicController>();
        if (physicController)
            physicController.SetData(character: this);

        var introState = GetState(eStateType.intro);
        if (introState != null)
            SetState(introState);
    }
}
