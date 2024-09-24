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

        #region Move
        if (_isMove)
        {
            // Tạo vector hướng di chuyển 2D  
            Vector2 movement = new Vector2(onMoveDicrection, 0);

            // Di chuyển transform  
            transform.Translate(movement * speed * Time.deltaTime);
        }
        #endregion

        if (isGround)
            return;

        #region Jump
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
        #endregion

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

    #region Move
    
    bool _isMove { get; set; }
    public float onMoveDicrection { get; set; } = 1f;
    public float speed = 1f; // Tốc độ di chuyển
    public void Move(eDirectionType directionType)
    {
        _isMove = true;
        onMoveDicrection = directionType is eDirectionType.Right ? 1 : -1;
    }

    public void StopMove() => _isMove = false;
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
