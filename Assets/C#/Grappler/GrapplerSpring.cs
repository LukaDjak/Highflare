using UnityEngine;

public class GrappleSpring
{
    private float strength, damper, target, velocity, value;

    public void Update(float deltaTime)
    {
        float direction = Mathf.Sign(target - value);
        float force = Mathf.Abs(target - value) * strength;
        velocity += (force * direction - velocity * damper) * deltaTime;
        value += velocity * deltaTime;
    }

    public void Reset() => (velocity, value) = (0f, 0f);
    public void SetValue(float v) => value = v;
    public void SetTarget(float t) => target = t;
    public void SetDamper(float d) => damper = d;
    public void SetStrength(float s) => strength = s;
    public void SetVelocity(float v) => velocity = v;
    public float Value => value;
}