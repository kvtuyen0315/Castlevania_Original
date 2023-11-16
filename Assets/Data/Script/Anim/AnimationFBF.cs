using TuyenAFramework;
using System.Collections.Generic;
using UnityEngine;

public class AnimationFBF : BaseUIComp
{
    [SerializeField] SpriteRenderer spriteRenderer;

    [Header("Data")]
    [SerializeField] List<Sprite> lstSprite = new();
    [SerializeField] float frameRate = 60f;
    [SerializeField] bool isRevert;

    float curFrameRate => 1f / frameRate;
    float _timer { get; set; }
    int _curIndex { get; set; }

    // AAA - Viết về việc cho character thay đổi anim.

    public void Update()
    {
        if (lstSprite.IsNullOrEmpty()) return;

        if (lstSprite.Count is 1) return;

        _timer += Time.deltaTime;
        if (_timer >= curFrameRate)
        {
            _timer = 0f;
            if (isRevert)
            {
                _curIndex = _curIndex - 1;
                if (_curIndex < 0)
                {
                    _curIndex = lstSprite.Count - 1;
                }
            }
            else
            {
                _curIndex = (_curIndex + 1) % lstSprite.Count;
            }
            spriteRenderer.sprite = lstSprite[_curIndex];
        }
    }
}
