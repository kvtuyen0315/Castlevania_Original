/// <summary>
/// PRNG có seed cho gameplay (CVGP 07, H7): xorshift 16-bit (7, 9, 8) — chu kỳ 65 535, trạng thái 1 số nguyên ⇒ replay
/// ra đúng từng frame. StageWorld giữ 1 bộ (StageWorld.Rng) và Reset ở đầu stage. Thuật toán RNG của ROM Castlevania
/// chưa biết (không có tài liệu) ⇒ chỉ đảm bảo tất định, không giả lập chuỗi số của bản gốc.
/// KHÔNG dùng UnityEngine.Random / System.Random cho gameplay.
/// </summary>
public class NesRandom
{
    public const int DEFAULT_SEED = 0x2A5D;

    private int state;

    public NesRandom(int seed = DEFAULT_SEED)
    {
        Reset(seed);
    }

    /// <summary>Trạng thái hiện tại (ghi vào log/hash khi kiểm tất định).</summary>
    public int State => state;

    public void Reset(int seed)
    {
        state = seed & 0xFFFF;
        if (state == 0)
            state = DEFAULT_SEED;
    }

    /// <summary>Số 1..65535.</summary>
    public int Next16()
    {
        int x = state;
        x ^= (x << 7) & 0xFFFF;
        x ^= x >> 9;
        x ^= (x << 8) & 0xFFFF;
        state = x;
        return x;
    }

    /// <summary>Số 0..max−1 (max ≤ 65536); max ≤ 0 ⇒ 0.</summary>
    public int Next(int max)
    {
        if (max <= 1)
            return 0;
        return (int)(((long)(Next16() - 1) * max) / 65535);
    }

    /// <summary>Số min..max−1.</summary>
    public int Range(int min, int max)
    {
        return min + Next(max - min);
    }

    /// <summary>true với xác suất num/den.</summary>
    public bool Chance(int num, int den)
    {
        return Next(den) < num;
    }
}
