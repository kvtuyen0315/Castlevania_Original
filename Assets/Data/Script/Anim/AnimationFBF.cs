using System.Collections.Generic;
using TuyenAFramework;
using UnityEngine;

public class AnimationFBF : BaseUIComp
{
    [SerializeField] SpriteRenderer spriteRenderer;

    [Header("Data")]
    [SerializeField] List<Sprite> lstSprite = new();
    [SerializeField] float frameRate = 60f;
    [SerializeField] float speed = 1f;
    [SerializeField] bool isRevert;

    float curFrameRate => (1f / frameRate) / speed;
    float _timer { get; set; }
    int _curIndex { get; set; }
    bool isNullLstSprite { get; set; }
    bool isOneSprite { get; set; }

    public bool isPlayAnim { get; private set; }
    public bool isEndAnim { get; private set; }

    public void SetData(List<Sprite> lstSprite, bool isPlayAnim = true)
    {
        this.isPlayAnim = isPlayAnim;

        this.lstSprite = lstSprite;
        isNullLstSprite = lstSprite.IsNullOrEmpty();
        isOneSprite = !isNullLstSprite && lstSprite.Count is 1;
        CheckFrame(isRevert: isOneSprite ? true : isRevert, isCheckNext: false);
    }

    private void CheckFrame(bool isRevert, bool isCheckNext)
    {
        _timer = 0f;

        if (isCheckNext)
        {
            if (isRevert)
            {
                isEndAnim = _curIndex <= -1;
                _curIndex = _curIndex - 1;
                if (_curIndex < 0)
                {
                    _curIndex = lstSprite.Count - 1;
                }
            }
            else
            {
                isEndAnim = _curIndex == lstSprite.Count - 1;
                _curIndex = (_curIndex + 1) % lstSprite.Count;
            }
        }
        else
        {
            _curIndex = isRevert ? lstSprite.Count - 1 : 0;
        }

        spriteRenderer.sprite = lstSprite[_curIndex];
    }

    public void Update()
    {
        if (!isPlayAnim) return;
        if (isNullLstSprite) return;
        if (isOneSprite) return;

        _timer += Time.deltaTime;
        if (_timer >= curFrameRate)
        {
            CheckFrame(isRevert, isCheckNext: true);
        }
    }
}
