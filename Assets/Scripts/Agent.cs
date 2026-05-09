using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    [SerializeField] float maxSpeed = 2f;
    [SerializeField] float radius = 0.5f;
    private Vector3 velocity;
    PathManager pathManager = new PathManager();
    private Vector3 acceleration = Vector3.zero;
    float slowingRadius = 1.0f;

    public void SetAcceleration(Vector3 acceleration){
        this.acceleration = acceleration;
    }

    Vector3 trucante(Vector3 v, float max)
    {
        float size = Mathf.Min(v.magnitude, max);
        return v.normalized * size;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        velocity = Vector3.zero;
        GetComponent<CapsuleCollider>().radius = radius;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        GetComponent<Rigidbody>().position += velocity * Time.fixedDeltaTime;
    }

    public void SetSlowingRadius(float slowingRadius) {
        this.slowingRadius = slowingRadius;
    }
    public float GetMass()
    {
        return GetComponent<Rigidbody>().mass;
    }
    public Vector3 GetVelocity() { 
        return velocity;
    }
    public void SetVelocity(Vector3 velocity) {
        this.velocity = trucante(velocity, maxSpeed);
    }
    public bool CheckGoalReached() {
        Vector3 goal = pathManager.GetGoal();
        float distanceToGoal = Vector3.Distance(transform.position, goal);
        return distanceToGoal < radius;
    }
    public void GenerateRandomGoal(float xMin, float xMax, float zMin, float zMax) {
        float x = Random.Range(xMin, xMax);
        float z = Random.Range(zMin, zMax);
        Vector3 randomGoal = new Vector3(x, 0, z);
        pathManager.SetGoal(randomGoal);
    }

    public Vector3 GetGoal() {
        return pathManager.GetGoal();
    }
    public float GetMaxSpeed() {
        return maxSpeed;
    }
    public float GetRadius()
    {
        return radius;
    } 
    public void SetRadius(float radius)
    {
        this.radius = radius;
    }

    public List<Vector3> GetPolyline()
    {
        return pathManager.GetPolyline();
    }
    public void ComputePathToGoalAndRecomputeGoalIfNotFound(Grid grid, int tries, bool alternativePathFinding)
    {
        GridCell startCell = grid.getGridCell(transform.position);
        int found = pathManager.ComputePathToGoalAndRecomputeGoalIfNotFound(grid, startCell, alternativePathFinding, tries);

        if (found == -1)
        {
            Debug.Log("NOT FOUND PATH TO GOAL WITHIN " + tries + " tries");
        }
    }

    public GridCell getNextWaypoint()
    {
        return pathManager.GetNextWaypoint();
    }

    public void AdvanceToNextWaypoint()
    {
        pathManager.AdvanceToNextWaypoint();
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 goal = pathManager.GetGoal();
        Gizmos.DrawSphere(goal + Vector3.up * 0.1f, 0.2f);

        List<GridCell> path = pathManager.GetPath();
        Gizmos.color = Color.cyan;
        foreach (GridCell cell in path)
        {
            Vector3 cellCenter = cell.getCenter();
            Gizmos.DrawCube(cellCenter + Vector3.up * 0.1f, new Vector3(0.2f, 0.2f, 0.2f));
        }

        GridCell next = pathManager.GetNextWaypoint();
        if(next != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(next.getCenter() + Vector3.up * 0.1f, new Vector3(0.2f, 0.2f, 0.2f));
            //Gizmos.DrawSphere(next.getCenter(), slowingRadius);
            
        }


        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, (transform.position + acceleration * 100));

        Simulator sim = Simulator.instance;
        if (sim == null) sim = FindFirstObjectByType<Simulator>();

        if (sim != null && velocity.magnitude > 0.01f)
        {
            float dynamicLength = velocity.magnitude / maxSpeed * sim.avoidanceLength;
            Vector3 direction = velocity.normalized;
            RaycastHit hit;

            Gizmos.color = Color.white;
            bool hasHitWall = Physics.SphereCast(transform.position, radius, direction, out hit, dynamicLength, sim.wallLayerMask);
            bool hasHitObstacle = false;
            if (hasHitWall)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                if(Physics.SphereCast(transform.position, radius, direction, out hit, dynamicLength, sim.obstacleLayerMask))
                {
                    Gizmos.color = Color.blue;
                    hasHitObstacle = true;
                }
            }
            
            Vector3 endPos = transform.position + direction * dynamicLength;
            if (hasHitWall || hasHitObstacle)
            {
                endPos = transform.position + direction * hit.distance;
            }
            Gizmos.DrawLine(transform.position, endPos);
            Gizmos.DrawWireSphere(endPos, radius);
        }

        if(sim != null && sim.activeForces.HasFlag(Simulator.SteeringType.PathFollowing))
        {
            GetPolyline().ForEach( point => 
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(point + Vector3.up * 0.1f, sim.pathRadius);
            } );
        }
    }

    public Vector3 GetPreviousWaypointPosition()
    {
        return pathManager.GetPreviousWaypointPosition();
    }
}
