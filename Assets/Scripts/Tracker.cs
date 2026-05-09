using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Tracker : MonoBehaviour
{
    public Vector3 worldVelocityVector { get; private set; }
    public Vector3 localVelocityVector { get; private set; }
    public float speed { get; private set; }

    private Vector3 prevPos;

    public Vector3 forwardDir;
    private Vector3 displacement;

    void Start()
    {
        prevPos = transform.position;
    }

    void FixedUpdate()
    {
        displacement = transform.position - prevPos;
        worldVelocityVector = displacement / Time.fixedDeltaTime;
        speed = worldVelocityVector.magnitude;

        localVelocityVector = transform.InverseTransformDirection(worldVelocityVector);

        prevPos = transform.position;

        Debug.Log($"World Velocity: {worldVelocityVector}, Local Velocity: {localVelocityVector}, Speed: {speed}");
        Debug.Log($"Forward Dir: {forwardDir}");
    }

    void OnValidate()
    {
        forwardDir.y = 0;
        forwardDir = forwardDir.normalized;
    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.red;
        //Vector3 start = transform.position;
        //Vector3 end = start + (forwardDir.normalized);

        //Gizmos.DrawLine(start, end);
        //Gizmos.DrawSphere(end, 0.05f);
        //Handles.Label(end, "Forward direction");


        Gizmos.color = new Color(0.0f, 0.75f, 0.0f, 0.75f);
        Gizmos.DrawLine(transform.position, transform.position + displacement * 5);
    }
}

