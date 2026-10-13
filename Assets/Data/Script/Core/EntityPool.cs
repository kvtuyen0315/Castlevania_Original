using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pool thực thể theo prefab (CVGP 07): Get lấy instance tắt sẵn (hoặc Instantiate lần đầu — Awake chạy 1 lần), Release tắt
/// GameObject và cất lại. Thực thể tái dùng ⇒ lớp con PHẢI đặt lại trạng thái trong OnSpawned. Prewarm để tránh Instantiate giữa trận.
/// </summary>
public class EntityPool
{
    private readonly Dictionary<Entity, Stack<Entity>> free = new Dictionary<Entity, Stack<Entity>>();

    public int CreatedCount { get; private set; }

    public Entity Get(Entity prefab, Transform parent)
    {
        Stack<Entity> stack;
        if (free.TryGetValue(prefab, out stack) && stack.Count > 0)
        {
            var e = stack.Pop();
            if (e.transform.parent != parent)
                e.transform.SetParent(parent, false);
            e.gameObject.SetActive(true);
            return e;
        }
        var inst = Object.Instantiate(prefab, parent, false);
        inst.name = prefab.name;
        inst.SourcePrefab = prefab;
        CreatedCount++;
        return inst;
    }

    public void Release(Entity e)
    {
        if (e == null || e.SourcePrefab == null)
            return;
        e.gameObject.SetActive(false);
        Stack<Entity> stack;
        if (!free.TryGetValue(e.SourcePrefab, out stack))
        {
            stack = new Stack<Entity>();
            free.Add(e.SourcePrefab, stack);
        }
        stack.Push(e);
    }

    public void Prewarm(Entity prefab, int count, Transform parent)
    {
        var tmp = new List<Entity>(count);
        for (int i = 0; i < count; i++)
            tmp.Add(Get(prefab, parent));
        for (int i = 0; i < tmp.Count; i++)
            Release(tmp[i]);
    }
}
