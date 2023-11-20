using System.Collections.Generic;
using UnityEngine;

public class BaseCharacter : MonoBehaviour
{
    public AnimationFBF animationFBF { get; protected set; }
    public InputController controller { get; protected set; }
    public PhysicController physicController { get; protected set; }

    #region State
    public BaseState curState { get; protected set; }
    public NoneState noneState { get; protected set; }
    public IntroState introState { get; protected set; }
    public IdleState idleState { get; protected set; }
    public WalkState walkState { get; protected set; }
    public DuckState duckState { get; protected set; }
    public JumpState jumpState { get; protected set; }

    public void SetState(BaseState state)
    {
        if (curState != null)
            curState.OnExitState();

        curState = state;
        if (curState != null)
            curState.OnEnterState();
    }
    #endregion

    public virtual void SetData(InputController controller) { }

    protected ConfigAnimationRecord GetConfig(eStateType stateType)
    {
        return AnimationManager.configAnimationSimond.records.Find(s => s.name == stateType.ToString());
    }

    public void PlayAnim(ConfigAnimationRecord config)
    {
        List<Sprite> lstSprite = AnimationManager.GetLstAnimation(key: config.name, AnimationManager.configAnimationSimond.dicAnim);
        animationFBF.SetData(lstSprite);
    }

    public eDirectionType directionTypeH { get; private set; } = eDirectionType.Left;
    public eDirectionType directionTypeV { get; private set; } = eDirectionType.None;
    public void SetDirection(eDirectionType directionType)
    {
        switch (directionType)
        {
            case eDirectionType.None: directionTypeV = eDirectionType.None; break;
            case eDirectionType.Up: directionTypeV = eDirectionType.Up; break;
            case eDirectionType.Right: directionTypeH = eDirectionType.Right; break;
            case eDirectionType.Down: directionTypeV = eDirectionType.Down; break;
            case eDirectionType.Left: directionTypeH = eDirectionType.Left; break;
        }

        if (controller.isJump) return;

        if (directionType is eDirectionType.Right or eDirectionType.Left)
        {
            float x = directionType is eDirectionType.Right ? -Mathf.Abs(transform.localScale.x) : Mathf.Abs(transform.localScale.x);
            transform.localScale = new Vector3(x, transform.localScale.y, transform.localScale.z);
        }
    }

    public bool isPause { get; set; }
    protected virtual void Update()
    {
        if (isPause) return;
        if (curState is null) return;

        curState.OnExecuteState();
        curState.CheckNextState();
    }

    // Cheat
#if UNITY_EDITOR
    private void Start()
    {
        CheatSetState();
    }

    [Header("Cheat")]
    [SerializeField] eStateType cheatState = eStateType.none;

    [ContextMenu("CheatSetState")]
    private void CheatSetState()
    {
        if (curState is null)
            SetData(InputManager.I.controllerP1);

        BaseState state = null;
        switch (cheatState)
        {
            case eStateType.ascend: break;
            case eStateType.dead: break;
            case eStateType.descend: break;
            case eStateType.duck: state = duckState; break;
            case eStateType.hurt: break;
            case eStateType.idle: state = idleState; break;
            case eStateType.intro: state = introState; break;
            case eStateType.jump: state = jumpState; break;
            case eStateType.walk: state = walkState; break;
            case eStateType.whipAscend: break;
            case eStateType.whipDescend: break;
            case eStateType.whipDuck: break;
            case eStateType.whipStand: break;
        }
        SetState(state);
    }

#endif
}
