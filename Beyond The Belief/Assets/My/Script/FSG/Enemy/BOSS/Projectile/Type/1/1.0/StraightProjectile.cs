using UnityEngine;

public class StraightProjectile: ProjectileBase
{
    public float Speed = 10f;

    protected override void Move()
    {
        transform.Translate(Vector3.forward * Speed * Time.deltaTime);
    }
}
