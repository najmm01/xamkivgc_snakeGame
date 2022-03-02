using UnityEngine;

public class FollowerPart : BodyPart
{
    internal BodyPart Target { get; set; }

    internal void SetSprite(Sprite sprite) => spriteRenderer.sprite = sprite;

    protected override void Update()
    {
        if (!GameController.Alive) return;

        moveDirection = (Target.GetPreviousPosition() - (Vector2)transform.position).normalized;
        rotation = Target.GetPreviousRotation();

        base.Update();
    }
}