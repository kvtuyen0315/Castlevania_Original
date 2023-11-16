using System.Collections.Generic;
using UnityEngine;

public class BaseCharacter : MonoBehaviour
{
    public AnimationFBF animationFBF { get; private set; }

    #region State
    public BaseState curState { get; private set; }
    public NoneState noneState { get; private set; } 
    public IntroState introState { get; private set; } 
    public IdleState idleState { get; private set; } 
    public WalkState walkState { get; private set; } 
    #endregion

    public void SetData()
    {
        animationFBF = GetComponent<AnimationFBF>();

        noneState = new NoneState() { character = this, config = GetConfig(eStateType.none) };
        introState = new IntroState() { character = this, config = GetConfig(eStateType.intro) };
        idleState = new IdleState() { character = this, config = GetConfig(eStateType.idle) };
        walkState = new WalkState() { character = this, config = GetConfig(eStateType.walk) };

        curState = noneState;
    }

    private ConfigAnimationRecord GetConfig(eStateType stateType)
    {
        return AnimationManager.configAnimationSimond.records.Find(s => s.name == stateType.ToString());
    }

    public void SetState(BaseState state)
    {
        if (curState != null)
            curState.OnExitState();

        curState = state;
        if (curState != null)
            curState.OnEnterState();
    }

    public void PlayAnim(ConfigAnimationRecord config)
    {
        List<Sprite> lstSprite = AnimationManager.GetLstAnimation(key: config.name, AnimationManager.configAnimationSimond.dicAnim);
        animationFBF.SetData(lstSprite);
    }

    public bool isPause { get; set; }
    private void Update()
    {
        if (isPause) return;
        if (curState is null) return;

        curState.OnExecuteState();
        curState.CheckNextState();
    }

    // Cheat
#if UNITY_EDITOR
    [Header("Cheat")]
    [SerializeField] eStateType cheatState = eStateType.none;

    [ContextMenu("CheatSetState")]
    private void CheatSetState()
    {
        if (curState is null)
            SetData();

        BaseState state = null;
        switch (cheatState)
        {
            case eStateType.ascend: break;
            case eStateType.dead: break;
            case eStateType.descend: break;
            case eStateType.duck: break;
            case eStateType.hurt: break;
            case eStateType.idle: state = idleState; break;
            case eStateType.intro: state = introState; break;
            case eStateType.jump: break;
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
