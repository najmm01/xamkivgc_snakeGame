using UnityEngine;

public class SnakeHead : BodyPart
{
    [SerializeField] private GameController gameController;

    private void OnEnable() => SwipeControls.OnSwipe += SetMoveDelta;

    private void OnDisable() => SwipeControls.OnSwipe -= SetMoveDelta;

    private void SetMoveDelta(SwipeControls.Direction direction)
    {
        switch (direction)
        {
            case SwipeControls.Direction.Right:
                moveDirection = Vector2.right;
                rotation = -90.0f;
                break;
            case SwipeControls.Direction.Left:
                moveDirection = Vector2.left;
                rotation = 90.0f;
                break;
            case SwipeControls.Direction.Up:
                moveDirection = Vector2.up;
                rotation = 0.0f;
                break;
            case SwipeControls.Direction.Down:
                moveDirection = Vector2.down;
                rotation = 180.0f;
                break;
        }
    }

    internal void ResetMovement()
    {
        ResetMemory();
        moveDirection = Vector2.up;
        rotation = 0.0f;
        transform.position = Vector2.zero;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Egg"))
        {
            gameController.GameOver();
            return;
        }

        gameController.EatEgg(other.gameObject);
    }
}