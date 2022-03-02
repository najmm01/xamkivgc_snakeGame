using UnityEngine;

public class BodyPart : MonoBehaviour
{
    private const int FollowBufferSize = 5;

    //circular buffer
    private Vector2[] posBuffer = new Vector2[FollowBufferSize];
    private float[] rotBuffer = new float[FollowBufferSize];
    private int setIndex = 0;
    private int getIndex = -(FollowBufferSize - 1);

    [SerializeField] protected SpriteRenderer spriteRenderer;
    protected Vector2 moveDirection;
    protected float rotation;

    internal int SortingOrder
    {
        get => spriteRenderer.sortingOrder;
        set => spriteRenderer.sortingOrder = value;
    }

    internal void ResetMemory()
    {
        posBuffer = new Vector2[FollowBufferSize];
        rotBuffer = new float[FollowBufferSize];
    }

    internal Vector2 GetPreviousPosition()
    {
        if (getIndex < 0) return transform.position;

        return posBuffer[getIndex];
    }

    internal float GetPreviousRotation()
    {
        if (getIndex < 0) return transform.rotation.eulerAngles.z;

        return rotBuffer[getIndex];
    }

    protected void Movement()
    {
        //position
        transform.position = (Vector2)transform.position + moveDirection * GameController.SnakeSpeed * Time.deltaTime;

        //rotation
        transform.rotation = Quaternion.Euler(0, 0, rotation);
    }

    protected virtual void Update()
    {
        if (!GameController.Alive) return;

        Movement();

        //update circular buffer
        posBuffer[setIndex].x = transform.position.x;
        posBuffer[setIndex].y = transform.position.y;
        rotBuffer[setIndex] = transform.rotation.eulerAngles.z;
        setIndex = (setIndex + 1) % FollowBufferSize;
        getIndex = (getIndex + 1) % FollowBufferSize;
    }
}