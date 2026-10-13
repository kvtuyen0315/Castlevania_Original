using UnityEngine;

/// <summary>
/// Overlay debug của lõi gameplay (CVGP 07) — đặt cạnh StageWorld. Vẽ bằng GL trong OnRenderObject của MainCamera (vào RT
/// 256×240 của PixelPerfectCamera ⇒ nét đúng 1 px NES), chỉ khi cờ bật (menu CVGP/Debug/*):
///   Hitbox Overlay        : hộp địa hình (lục lam), hurtbox (xanh lá), hitbox đang bật (đỏ), pivot (trắng), SpawnPoint (vàng)
///                           + nhãn tên/toạ độ/state (OnGUI, chỉ Editor)
///   Tile Collision Overlay: tô ô 16×16 theo loại (đặc đỏ, thang vàng, miệng thang cam, nước xanh, cửa tím, phá được xanh lá,
///                           hố tím đậm) + đường đi cầu thang (trắng) trong khung camera.
/// Không ảnh hưởng gameplay; tắt ⇒ không vẽ gì, không cấp phát.
/// </summary>
public class CvgpDebugOverlay : MonoBehaviour
{
    [SerializeField] private StageWorld world;
    [SerializeField] private Transform sectionOrigin;

    private static Material mat;

    private static readonly Color BODY = new Color(0f, 1f, 1f, 1f);
    private static readonly Color HURT = new Color(0.2f, 1f, 0.2f, 1f);
    private static readonly Color HIT = new Color(1f, 0.15f, 0.15f, 1f);
    private static readonly Color PIVOT = Color.white;
    private static readonly Color SPAWN = new Color(1f, 0.9f, 0f, 1f);
    private static readonly Color STAIR = new Color(1f, 1f, 1f, 0.9f);

    private void Reset()
    {
        world = GetComponent<StageWorld>();
    }

    private static Color TileColor(eTileCollision t)
    {
        switch (t)
        {
            case eTileCollision.Solid: return new Color(1f, 0.24f, 0.24f, 0.35f);
            case eTileCollision.Stair: return new Color(1f, 0.9f, 0f, 0.35f);
            case eTileCollision.StairFloor: return new Color(1f, 0.55f, 0f, 0.45f);
            case eTileCollision.Water: return new Color(0f, 0.67f, 1f, 0.45f);
            case eTileCollision.Pit: return new Color(0.6f, 0f, 1f, 0.45f);
            case eTileCollision.Door: return new Color(1f, 0f, 1f, 0.45f);
            case eTileCollision.Breakable: return new Color(0f, 1f, 0.3f, 0.45f);
            case eTileCollision.Hazard: return new Color(1f, 1f, 1f, 0.45f);
            default: return Color.clear;
        }
    }

    private void OnRenderObject()
    {
        if (!(CvgpDebug.ShowHitbox || CvgpDebug.ShowTiles) || world == null || world.Tiles == null)
            return;
        var cam = world.CameraCtl != null ? world.CameraCtl.Cam : Camera.main;
        if (Camera.current != cam)
            return;
        if (mat == null)
        {
            mat = new Material(Shader.Find("Hidden/Internal-Colored")) { hideFlags = HideFlags.HideAndDontSave };
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            mat.SetInt("_ZWrite", 0);
            mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
        }
        mat.SetPass(0);
        GL.PushMatrix();
        var origin = sectionOrigin != null ? sectionOrigin : (world.Section != null ? world.Section.transform : transform);
        GL.MultMatrix(origin.localToWorldMatrix);
        GL.Begin(GL.QUADS);
        int cx = world.CameraCtl != null ? world.CameraCtl.X : 0;
        if (CvgpDebug.ShowTiles)
            DrawTiles(cx);
        if (CvgpDebug.ShowHitbox)
            DrawEntities();
        GL.End();
        GL.PopMatrix();
    }

    private void DrawTiles(int cx)
    {
        var t = world.Tiles;
        int c0 = TileCollision.FloorDiv(cx, TileCollision.CELL) - 1;
        int c1 = c0 + NesCoord.SCREEN_W / TileCollision.CELL + 2;
        int rows = t.Height / TileCollision.CELL;
        for (int r = 0; r < rows; r++)
            for (int c = c0; c <= c1; c++)
            {
                if (c < 0 || c * TileCollision.CELL >= t.Width)
                    continue;
                var col = TileColor(t.CellType(c, r));
                if (col.a <= 0f)
                    continue;
                Fill(c * TileCollision.CELL, r * TileCollision.CELL, TileCollision.CELL, TileCollision.CELL, col);
                Outline(new RectInt(c * TileCollision.CELL, r * TileCollision.CELL, TileCollision.CELL, TileCollision.CELL), new Color(col.r, col.g, col.b, 0.9f));
            }
        for (int i = 0; i < t.StairCount; i++)
        {
            var p = t.GetStair(i);
            for (int s = 0; s <= p.Length; s++)
            {
                var q = p.PointAt(s);
                Fill(q.x, q.y, 1, 1, STAIR);
            }
            Fill(p.bottom.x - 1, p.bottom.y - 1, 3, 3, STAIR);
            Fill(p.top.x - 1, p.top.y - 1, 3, 3, STAIR);
        }
    }

    private void DrawEntities()
    {
        for (int g = 0; g < Entity.GROUP_COUNT; g++)
        {
            var list = world.GroupList(g);
            for (int i = 0; i < list.Count; i++)
            {
                var e = list[i];
                if (!e.Alive)
                    continue;
                Outline(e.BodyRect, BODY);
                var hus = e.Hurtboxes;
                for (int h = 0; h < hus.Count; h++)
                    if (hus[h].active)
                        Outline(e.ToSection(hus[h].local), HURT);
                var hbs = e.Hitboxes;
                for (int h = 0; h < hbs.Count; h++)
                    if (hbs[h].active)
                        Outline(e.ToSection(hbs[h].local), HIT);
                Fill(e.X - 1, e.Y, 3, 1, PIVOT);
                Fill(e.X, e.Y - 1, 1, 3, PIVOT);
            }
        }
        var sps = world.SpawnPoints;
        for (int i = 0; i < sps.Count; i++)
        {
            var p = sps[i].position;
            Outline(new RectInt(p.x - 2, p.y - 4, 5, 5), SPAWN);
        }
    }

    // px section (y xuống) → toạ độ cục bộ của gốc section (1 unit = 16 px, y lên)
    private static void Vertex(int x, int y)
    {
        GL.Vertex3(x / (float)NesCoord.PPU, -y / (float)NesCoord.PPU, 0f);
    }

    private static void Fill(int x, int y, int w, int h, Color c)
    {
        GL.Color(c);
        Vertex(x, y);
        Vertex(x + w, y);
        Vertex(x + w, y + h);
        Vertex(x, y + h);
    }

    private static void Outline(RectInt r, Color c)
    {
        if (r.width <= 0 || r.height <= 0)
            return;
        Fill(r.x, r.y, r.width, 1, c);
        Fill(r.x, r.y + r.height - 1, r.width, 1, c);
        Fill(r.x, r.y + 1, 1, r.height - 2, c);
        Fill(r.x + r.width - 1, r.y + 1, 1, r.height - 2, c);
    }

#if UNITY_EDITOR
    private GUIStyle style;

    private void OnGUI()
    {
        if (!CvgpDebug.ShowHitbox || world == null || world.Tiles == null)
            return;
        var cam = world.CameraCtl != null ? world.CameraCtl.Cam : Camera.main;
        if (cam == null)
            return;
        if (style == null)
            style = new GUIStyle(GUI.skin.label) { fontSize = 16, richText = false };
        var origin = world.Section != null ? world.Section.transform : transform;
        for (int g = 0; g < Entity.GROUP_COUNT; g++)
        {
            var list = world.GroupList(g);
            for (int i = 0; i < list.Count; i++)
            {
                var e = list[i];
                if (!e.Alive)
                    continue;
                var b = e.BodyRect;
                var wp = origin.TransformPoint(new Vector3(b.x / (float)NesCoord.PPU, -(b.y - 2) / (float)NesCoord.PPU, 0f));
                var sp = cam.WorldToScreenPoint(wp);
                string s = e.name + " (" + e.X + "," + e.Y + ") " + e.DebugState + (e.InvincibleTicks > 0 ? " inv" + e.InvincibleTicks : "");
                var r = new Rect(sp.x, Screen.height - sp.y - 22, 420, 22);
                GUI.color = Color.black;
                GUI.Label(new Rect(r.x + 1, r.y + 1, r.width, r.height), s, style);
                GUI.color = Color.yellow;
                GUI.Label(r, s, style);
            }
        }
        GUI.color = Color.white;
        string head = "tick " + world.TickCount + "  cam " + (world.CameraCtl != null ? world.CameraCtl.X : 0)
                      + "  section " + (world.Section != null ? world.Section.name : "-")
                      + (world.IsWorldFrozen ? "  FREEZE" : "") + (world.IsEnemyFrozen ? "  STOPWATCH" : "")
                      + (CvgpDebug.CheatInvincible ? "  [BẤT TỬ]" : "");
        GUI.color = Color.black;
        GUI.Label(new Rect(cam.pixelRect.x + 9, Screen.height - cam.pixelRect.yMax + 205, 900, 24), head, style);
        GUI.color = Color.cyan;
        GUI.Label(new Rect(cam.pixelRect.x + 8, Screen.height - cam.pixelRect.yMax + 204, 900, 24), head, style);
        GUI.color = Color.white;
    }
#endif
}
