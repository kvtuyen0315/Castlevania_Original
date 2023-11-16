using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/ConfigAnimation")]
public class ConfigAnimation : BNDataTable<ConfigAnimationRecord>
{
    public const string pathData = "Character/Simond/";

    public Dictionary<string, Sprite> dicAnim { get; private set; } = new();

    public void Init()
    {
        foreach (ConfigAnimationRecord config in records)
        {
            string path = config.name.Replace("_", string.Empty);
            Sprite[] lstSprites = Resources.LoadAll<Sprite>($"{pathData}{path}");
            if (lstSprites.IsNullOrEmpty()) continue;

            for (int i = 0; i < lstSprites.Length; i++)
            {
                Sprite sprite = lstSprites[i];
                if (!dicAnim.ContainsKey(sprite.name))
                    dicAnim.Add(sprite.name, sprite);
            }
        }
    }
}

[Serializable]
public class ConfigAnimationRecord
{
    public string name;
    public int lengthAnim;
}
