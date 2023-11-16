using System.Collections.Generic;
using System.Linq;
using TuyenAFramework;
using UnityEngine;

public class AnimationManager : ManualSingletonMono<AnimationManager>
{
    public static ConfigAnimation configAnimationSimond { get; private set; }

    const string pathConfig = "Config/";

    public void Init()
    {
        configAnimationSimond = Resources.Load<ConfigAnimation>($"{pathConfig}ConfigAnimationSimond");
        if (configAnimationSimond) configAnimationSimond.Init();
    }

    public static List<Sprite> GetLstAnimation(string key, Dictionary<string, Sprite> dicAnim)
    {
        return dicAnim
            .Where(item => item.Key.Contains(key))
            .Select(item => item.Value)
            .ToList();
    }
}
