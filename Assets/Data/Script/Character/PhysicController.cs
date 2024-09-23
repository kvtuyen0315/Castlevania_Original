using UnityEngine;

public class PhysicController : MonoBehaviour
{
    [SerializeField] Vector2 startPosition;

    BaseCharacter _character { get; set; }

    [SerializeField] LayerMask groundLayer;
    [SerializeField] Vector2 boxSize = Vector2.one;
    public bool isGround { get; private set; }
    public bool isPause => !_character || (_character && _character.isPause);

    public void SetData(BaseCharacter character)
    {
        _character = character;
    }

    public void Update()
    {
        if (isPause)
            return;

        if (isGround)
            return;

        if (_isJumping && _onJumpUp)
        {
            transform.Translate(Vector3.up * (jumpForce * (1f - _jumpTime / jumpDuration)) * Time.deltaTime);
            _jumpTime += Time.deltaTime;

            if (_jumpTime >= jumpDuration)
            {
                _jumpTime = 0f;
                _onJumpUp = false;
            }
            else
                return;
        }

        isGround = IsGrounded();
        Debug.Log($"isGround {isGround}");

        if (isGround)
            _isJumping = false;

        Fall();
    }

    [SerializeField] float fallSpeed = 5f;
    private void Fall()
    {
        // Di chuyển nhân vật theo hướng Vector2.down với tốc độ fallSpeed
        _jumpTime += Time.deltaTime;
        transform.Translate(Vector2.down * (jumpForce * _jumpTime / fallSpeed) * Time.deltaTime);
    }

    #region Jump
    [SerializeField] float jumpForce = 2f; // Độ mạnh của cú nhảy  
    [SerializeField] float jumpDuration = 1f;
    float _jumpTime { get; set; }
    bool _isJumping { get; set; } // Kiểm tra xem nhân vật đang nhảy hay không
    bool _onJumpUp { get; set; }
    public void Jump()
    {
        if (_isJumping)
            return;

        _isJumping = true;
        _onJumpUp = true;
        _jumpTime = 0f;
        isGround = false;
    }
    #endregion

    Vector2 posBox => new Vector2(transform.position.x, transform.position.y + (boxSize.y / 2));
    private bool IsGrounded()
    {
        // Tạo một hộp ở dưới chân nhân vật để kiểm tra va chạm với mặt đất
        RaycastHit2D hit = Physics2D.BoxCast(posBox, boxSize, 0f, Vector2.down, .0f, groundLayer);

        // Nếu va chạm với mặt đất, trả về true
        return hit.collider != null;
    }

    // Gọi hàm này để reset vị trí nhân vật khi cần
    [ContextMenu("ResetCharacterPosition")]
    public void ResetCharacterPosition()
    {
        transform.position = startPosition;
    }
}
