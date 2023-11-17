using TuyenAFramework;

public enum eDirectionType
{
    None = -1,
    Up,
    Right,
    Down,
    Left,
}

public class InputManager : ManualSingletonMono<InputManager>
{
    public InputController controllerP1 { get; private set; }
    public InputController controllerP2 { get; private set; }

#if UNITY_EDITOR
    protected override void Awake()
    {
        base.Awake();
        SetPlayerController(controllerP1: new InputController(), controllerP2: null);
    } 
#endif

    public void SetPlayerController(InputController controllerP1, InputController controllerP2)
    {
        this.controllerP1 = controllerP1;
        this.controllerP2 = controllerP2;
    }

    private void Update()
    {
        if (controllerP1 != null) controllerP1.Update();
        if (controllerP2 != null) controllerP2.Update();
    }
}
