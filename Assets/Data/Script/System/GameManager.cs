using TuyenAFramework;

public class GameManager : ManualSingletonMono<GameManager>
{
    private void Start()
    {
        AnimationManager.I.Init();
    }
}
