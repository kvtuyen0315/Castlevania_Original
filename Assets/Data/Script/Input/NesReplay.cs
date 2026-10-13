using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Định dạng replay input (RLE dòng chữ, CVGP_MEMORY §4) — 1 giá trị / tick NesClock:
///   - Token cách nhau bằng khoảng trắng/xuống dòng; dòng bắt đầu bằng '#' là chú thích.
///   - Token = TRẠNG_THÁI*SỐ_TICK (thiếu "*n" ⇒ 1 tick). TRẠNG_THÁI = '-' (không giữ nút nào)
///     hoặc tên nút nối bằng '+': U D L R A B Sel Start Sub (B = nút B NES = Roi; Start = Start; Sub = SubWeapon).
///   - Hậu tố '!' sau tên nút (vd "R+A!") = nhấn lại trong cùng tick dù tick trước đang giữ (Pressed dù Held liên tục) — hiếm.
///   - Phần tử đầu = tick đầu tiên NesPad.Tick() chạy sau khi gọi PlayReplay.
///   Ví dụ: "R*120 R+A*1 R*30 -*10 B*1" = giữ Phải 120 tick, Phải+Nhảy 1 tick, Phải 30 tick, thả 10 tick, Roi 1 tick.
/// Giá trị mỗi tick = bitmask Held (bit = (int)eNesButton) | bitmask nhấn-lại << NesPad.REPRESS_SHIFT.
/// </summary>
public static class NesReplay
{
    static readonly string[] NAMES = { "U", "D", "L", "R", "A", "B", "Sel", "Start", "Sub" };

    public static string FormatState(int value)
    {
        int held = value & NesPad.BUTTON_MASK;
        int repress = (value >> NesPad.REPRESS_SHIFT) & NesPad.BUTTON_MASK;
        if (held == 0)
            return "-";
        var sb = new StringBuilder();
        for (int i = 0; i < NesPad.BUTTON_COUNT; i++)
        {
            if ((held & (1 << i)) == 0)
                continue;
            if (sb.Length > 0)
                sb.Append('+');
            sb.Append(NAMES[i]);
            if ((repress & (1 << i)) != 0)
                sb.Append('!');
        }
        return sb.ToString();
    }

    public static string Encode(IList<int> ticks)
    {
        var sb = new StringBuilder();
        int i = 0;
        int perLine = 0;
        while (i < ticks.Count)
        {
            int v = ticks[i];
            int n = 1;
            while (i + n < ticks.Count && ticks[i + n] == v)
                n++;
            if (sb.Length > 0)
                sb.Append(++perLine % 12 == 0 ? '\n' : ' ');
            sb.Append(FormatState(v));
            sb.Append('*').Append(n);
            i += n;
        }
        return sb.ToString();
    }

    /// <summary>Giải mã chuỗi RLE; sai cú pháp ⇒ FormatException (kèm token lỗi).</summary>
    public static List<int> Decode(string rle)
    {
        var result = new List<int>();
        if (string.IsNullOrEmpty(rle))
            return result;
        foreach (var rawLine in rle.Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line[0] == '#')
                continue;
            foreach (var token in line.Split(new[] { ' ', '\t', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int star = token.IndexOf('*');
                string state = star >= 0 ? token.Substring(0, star) : token;
                int count = 1;
                if (star >= 0 && (!int.TryParse(token.Substring(star + 1), out count) || count < 1))
                    throw new FormatException("NesReplay: số tick sai ở token '" + token + "'");
                int v = ParseState(state, token);
                for (int k = 0; k < count; k++)
                    result.Add(v);
            }
        }
        return result;
    }

    static int ParseState(string state, string token)
    {
        if (state == "-")
            return 0;
        int v = 0;
        foreach (var part in state.Split('+'))
        {
            bool repress = part.EndsWith("!");
            string name = repress ? part.Substring(0, part.Length - 1) : part;
            int idx = Array.IndexOf(NAMES, name);
            if (idx < 0)
                throw new FormatException("NesReplay: tên nút '" + name + "' lạ ở token '" + token + "'");
            v |= 1 << idx;
            if (repress)
                v |= 1 << (idx + NesPad.REPRESS_SHIFT);
        }
        return v;
    }

    /// <summary>FNV-1a 32 bit trên chuỗi giá trị tick — so 2 lần chạy trùng từng tick.</summary>
    public static uint Hash(IList<int> ticks)
    {
        uint h = 2166136261;
        for (int i = 0; i < ticks.Count; i++)
        {
            h = (h ^ (uint)ticks[i]) * 16777619;
        }
        return h;
    }
}
