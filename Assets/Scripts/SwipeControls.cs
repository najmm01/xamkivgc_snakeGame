using UnityEngine;

public class SwipeControls : MonoBehaviour
{
    public enum Direction
    {
        Right, Left, Up, Down
    }

    public static event System.Action<Direction> OnSwipe = delegate { }; //assign to empty delegate to avoid null check

    private Vector2 startSwipePos, endSwipePos;
    private float threshold = 20.0f;

    private void Update()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        foreach (var touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began) startSwipePos = touch.position;
            else if (touch.phase == TouchPhase.Ended)
            {
                endSwipePos = touch.position;
                ProcessTouch();
            }
        }
#else
        if(Input.GetMouseButtonDown(0))
        {
            startSwipePos = (Vector2)Input.mousePosition;
        }
        else if(Input.GetMouseButtonUp(0))
        {
            endSwipePos = (Vector2)Input.mousePosition;
            ProcessTouch();
        }
#endif
    }

    private void ProcessTouch()
    {
        if (Vector2.Distance(startSwipePos, endSwipePos) < threshold) return;

        if (Mathf.Abs(startSwipePos.x - endSwipePos.x) > Mathf.Abs(startSwipePos.y - endSwipePos.y))
        {
            //horizontal swipe
            if (startSwipePos.x > endSwipePos.x) OnSwipe(Direction.Right);
            else OnSwipe(Direction.Left);
        }
        else
        {
            //vertical swipe
            if (startSwipePos.y > endSwipePos.y) OnSwipe(Direction.Up);
            else OnSwipe(Direction.Down);
        }
    }
}
