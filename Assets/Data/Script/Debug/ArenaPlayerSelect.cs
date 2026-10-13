using UnityEngine;

/// <summary>
/// Scene Arena (CVGP 08): chọn thực thể Player trước khi StageWorld khởi tạo (Awake chạy trước tick đầu) — Simon thật (mặc định,
/// routine 08+) hoặc ArenaDummy (bộ kiểm lõi CvgpCoreProbe của routine 07 — hash tham chiếu `cc0c9a97`).
/// Chọn bằng menu CVGP/Arena/Player = ArenaDummy (EditorPrefs, chỉ Editor; build thường luôn Simon).
/// </summary>
[DefaultExecutionOrder(-300)]
public class ArenaPlayerSelect : MonoBehaviour
{
    public const string PREF_USE_DUMMY = "CVGP.ArenaUseDummy";

    [SerializeField] private GameObject simon;
    [SerializeField] private GameObject dummy;

    public static bool UseDummy
    {
        get
        {
#if UNITY_EDITOR
            return UnityEditor.EditorPrefs.GetBool(PREF_USE_DUMMY, false);
#else
            return false;
#endif
        }
    }

    private void Awake()
    {
        bool d = UseDummy;
        if (simon != null)
            simon.SetActive(!d);
        if (dummy != null)
            dummy.SetActive(d);
    }
}
