using UnityEngine;

public class InputController
{
    public float moveX { get; private set; }
    public float moveY { get; private set; }
    Vector2 move => new Vector2(moveX, moveY);
    public bool isJump { get; private set; }
    public bool isAttack { get; private set; }
    
    public void Update()
    {
        moveX = Input.GetAxisRaw("Horizontal");
        moveY = Input.GetAxisRaw("Vertical");
        isJump = Input.GetKeyDown(KeyCode.X);
        isAttack = Input.GetKeyDown(KeyCode.C);
    }
}
