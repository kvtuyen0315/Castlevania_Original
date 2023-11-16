using System.Collections.Generic;
using TuyenAFramework;
using System.Linq;
using UnityEngine;

public class AnimationManager : ManualSingletonMono<AnimationManager>
{
    public static ConfigAnimationSimond configAnimationSimond { get; private set; }

    const string pathConfig = "Config/";

    public void Init()
    {
        configAnimationSimond = Resources.Load<ConfigAnimationSimond>($"{pathConfig}ConfigAnimationSimond");
        if (configAnimationSimond) configAnimationSimond.Init();
    }

    public static List<Sprite> GetLstAnimation(string key, Dictionary<string, Sprite> dicAnim)
    {
        List<Sprite> lstSprite = dicAnim
            .Where(item => item.Key.Contains(key))
            .Select(item => item.Value)
            .ToList();

        return lstSprite;
    }
}
