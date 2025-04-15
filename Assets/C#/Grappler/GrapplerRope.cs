using UnityEngine;

public class GrapplerRope : MonoBehaviour
{
    private GrappleSpring spring;
    private Vector3 currentGrapplePosition;
    private LineRenderer lr;
    private Grappler gg;

    public int quality;
    public float damper;
    public float strength;
    public float velocity;
    public float waveCount;
    public float waveHeight;
    public AnimationCurve ac;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        gg = GetComponent<Grappler>();
        spring = new GrappleSpring();
        spring.SetTarget(0);
        lr.useWorldSpace = true;
    }

    private void LateUpdate() => DrawRope();

    void DrawRope()
    {
        if (!gg.IsGrappling())
        {
            currentGrapplePosition = gg.lineOrigin.position;
            spring.Reset();
            if (lr.positionCount > 0) lr.positionCount = 0;
            return;
        }

        if (lr.positionCount == 0)
        {
            spring.SetVelocity(velocity);
            lr.positionCount = quality + 1;
        }

        spring.SetDamper(damper);
        spring.SetStrength(strength);
        spring.Update(Time.deltaTime);

        var grapplePoint = gg.GetGrapplePoint();
        var gunTipPos = gg.lineOrigin.position;
        var up = Quaternion.LookRotation((grapplePoint - gunTipPos).normalized) * Vector3.up;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 12f);

        for (int i = 0; i < quality + 1; i++)
        {
            var delta = i / (float)quality;
            var offset = Mathf.Sin(delta * waveCount * Mathf.PI * spring.Value * ac.Evaluate(delta)) * waveHeight * up;
            lr.SetPosition(i, Vector3.Lerp(gunTipPos, currentGrapplePosition, delta) + offset);
        }
    }
}
