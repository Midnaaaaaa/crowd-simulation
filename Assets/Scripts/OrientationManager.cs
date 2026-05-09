using UnityEngine;

public class OrientationManager : MonoBehaviour
{
    private Tracker tracker;
    public bool fixedOrientation = true;

    void Start()
    {
        tracker = GetComponent<Tracker>();

    }

    // Update is called once per frame
    void Update()
    {
        if (fixedOrientation)
            transform.forward = tracker.forwardDir.normalized;
        else
        {
            Vector3 worldVelocity = tracker.worldVelocityVector;
            if (worldVelocity.sqrMagnitude > 0.01f)
            {
                transform.forward = Vector3.Lerp(transform.forward, worldVelocity.normalized, 5 * Time.deltaTime);
                tracker.forwardDir = transform.forward;
            }
        }
    }
}
