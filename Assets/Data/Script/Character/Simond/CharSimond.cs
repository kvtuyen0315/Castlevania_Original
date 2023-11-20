public class CharSimond : BaseCharacter
{
    public override void SetData(InputController controller)
    {
        this.controller = controller;

        animationFBF = GetComponent<AnimationFBF>();
        physicController = GetComponent<PhysicController>();
        if (physicController) physicController.SetData(character: this);

        noneState = new NoneState() { character = this, config = GetConfig(eStateType.none) };
        introState = new IntroState() { character = this, config = GetConfig(eStateType.intro) };
        idleState = new IdleState() { character = this, config = GetConfig(eStateType.idle) };
        walkState = new WalkState() { character = this, config = GetConfig(eStateType.walk) };
        duckState = new DuckState() { character = this, config = GetConfig(eStateType.duck) };
        jumpState = new JumpState() { character = this, config = GetConfig(eStateType.jump) };

        SetState(introState);
    }
}
