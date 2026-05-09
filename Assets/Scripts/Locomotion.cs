using UnityEngine;

[RequireComponent(typeof(Tracker))]
[RequireComponent(typeof(Animator))]
public class Locomotion : MonoBehaviour
{
    private Animator animator;
    private Tracker tracker;
    public float smoothingFactor;
    private Vector3 smoothedVelocity;

    void Start()
    {
        animator = GetComponent<Animator>();
        tracker = GetComponent<Tracker>();

        smoothedVelocity = Vector3.zero;
    }

    void Update()
    {
        Vector3 targetVelocity = tracker.localVelocityVector;
        smoothedVelocity = Vector3.Lerp(smoothedVelocity, targetVelocity, smoothingFactor * Time.deltaTime);

        animator.SetFloat("VelX", smoothedVelocity.x);
        animator.SetFloat("VelZ", smoothedVelocity.z);
    }
}