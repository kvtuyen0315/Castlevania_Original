using afsdk;

public class GameManager : ManualSingletonMono<GameManager>
{
    private void Start()
    {
        AnimationManager.I.Init();
    }
}
