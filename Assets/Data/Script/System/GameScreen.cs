using UnityEngine;

public enum eGameScreen
{
    Boot,
    Title,
    Intro,
    Stage1,     // màn chơi Map 1 (CVGP 15: StageScreen thay Stage1Stub — giữ tên id để GoTo cũ không đổi)
    GameOver,   // CVGP 12
    CastleMap,  // CVGP 15: bản đồ lâu đài sau Block 1
    Stage4Stub, // CVGP 15: STAGE 04 (chờ cụm Map 2) — CVM2 11: bỏ khỏi luồng + xoá màn (STAGE 04 thật = Stage1/StageScreen); giữ để số enum không đổi
    Stage7Stub, // CVM2 11: STAGE 07 sau bản đồ lâu đài Block 2 (chờ cụm Map 3) — CVM3 11: bỏ khỏi luồng + xoá màn (STAGE 07 thật = Stage1/StageScreen); giữ để số enum không đổi
    Stage10Stub, // CVM3 11: STAGE 10 sau bản đồ lâu đài Block 3 (chờ cụm Map 4)
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
