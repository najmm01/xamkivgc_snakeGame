using UnityEngine;

public class FillScreen : MonoBehaviour
{
    private void Start()
    {
        var camera = Camera.main;
        var height = camera.orthographicSize * 2;
        var width = camera.aspect * height;

        GetComponent<SpriteRenderer>().size = new Vector2(width, height);
    }
}
