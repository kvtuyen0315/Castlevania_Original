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
    public List<BaseState> stateList { get; protected set; }

    public BaseState GetState(eStateType stateType)
    {
        var state = stateList.Find(x => x.stateType == stateType);
        if (state is null)
        {
#if UNITY_EDITOR
            Debug.LogError($"state is null by stateList is {gameObject.name}");
#endif
        }

        return state;
    }

    public void SetState(BaseState state)
    {
        if (curState != null)
            curState.OnExitState();

        curState = state;
        if (curState != null)
            curState.OnEnterState();
    }
    #endregion

    public virtual void SetData(InputController controller)
    {
        noneState = new NoneState() { character = this, config = GetConfig(eStateType.none) };

        stateList = new();

        int length = (int)eStateType.Length;
        for (int i = 0; i < length; i++)
        {
            eStateType stateType = (eStateType)i;
            BaseState state = null;
            switch (stateType)
            {
                case eStateType.intro: state = new IntroState(); break;
                case eStateType.idle: state = new IdleState(); break;
                case eStateType.walk: state = new WalkState(); break;
                case eStateType.duck: state = new DuckState(); break;
                case eStateType.jump: state = new JumpState(); break;
            }

            if (state is null)
                continue;

            state.character = this;
            state.config = GetConfig(stateType);
            state.stateType = stateType;

            if (!stateList.Contains(state))
                stateList.Add(state);
        }
    }

    protected ConfigAnimationRecord GetConfig(eStateType stateType)
    {
        return AnimationManager.configAnimationSimond.records.Find(s => s.name == stateType.ToString());
    }

    public void PlayAnim(ConfigAnimationRecord config)
    {
        List<Sprite> lstSprite = AnimationManager.GetLstAnimation(key: config.name, AnimationManager.configAnimationSimond.dicAnim);
        animationFBF.SetData(lstSprite);
    }

    public eDirectionType curDirectionH => controller.moveX > 0 ? eDirectionType.Right : eDirectionType.Left;
    public eDirectionType curDirectionV => controller.moveY > 0 ? eDirectionType.Up : eDirectionType.Down;
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

        if (controller.isJump)
            return;

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

    #region Jump
    public bool isGround => physicController.isGround;

    public void Jump() => physicController.Jump();
    #endregion

    #region Move
    public void Move(eDirectionType directionType) => physicController.Move(directionType);

    public void StopMove() => physicController.StopMove();
    #endregion

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

        BaseState state = GetState(cheatState);
        if (state is null)
            return;

        SetState(state);
    }

#endif
}
