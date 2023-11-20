using UnityEngine;

public class PhysicController : MonoBehaviour
{
    [SerializeField] Vector2 startPosition;
    [SerializeField] float fallSpeed = 5f;

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
        if (isPause) return;
        if (isGround) return;

        isGround = IsGrounded();
        Debug.Log($"isGround {isGround}");

        if (!isGround)
        {
            Fall();
        }
    }

    private void Fall()
    {
        // Di chuyển nhân vật theo hướng Vector2.down với tốc độ fallSpeed
        transform.Translate(Vector2.down * fallSpeed * Time.deltaTime);
    }

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
