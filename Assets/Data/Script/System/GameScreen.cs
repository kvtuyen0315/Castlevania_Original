using UnityEngine;

public enum eGameScreen
{
    Boot,
    Title,
    Intro,
    Stage1,
}

/// <summary>
/// Một màn hình (prefab dưới ScreenRoot trong Main.unity). GameManager bật/tắt GameObject tức thời (cắt cứng NES)
/// và gọi logic theo tick: OnEnter ở tick bật màn (localFrame 0), OnTick(localFrame ≥ 1) các tick sau, OnExit khi rời.
/// Màn chỉ chạy logic trong OnTick — không Update/deltaTime.
/// </summary>
public abstract class GameScreen : MonoBehaviour
{
    public abstract eGameScreen Id { get; }

    public virtual void OnEnter() { }

    public virtual void OnTick(int localFrame) { }

    public virtual void OnExit() { }
}
