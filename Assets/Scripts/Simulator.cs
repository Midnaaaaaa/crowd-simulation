using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class Simulator : MonoBehaviour
{
    public static Simulator instance { get; private set; }
    List<Agent> agents = new List<Agent>();
    private float xMin, xMax, zMin, zMax;

    [SerializeField] private float timestep = 0.02f;
    [SerializeField] private int tries = 10;

    private Grid grid;
    private bool usingAStar = false;
    GridRepresentationGenerator gridRepresentationGenerator;
    CrowdGenerator crowdGenerator;

    [SerializeField] bool alternativePathFinding = false;


    [System.Flags]
    public enum SteeringType
    {
        None = 0,
        Seek = 1,
        Arrive = 2,
        Avoidance = 4,
        WallSlowing = 8,
        PathFollowing = 16
    }

    public SteeringType activeForces;


    [Range(0f, 10f)] public float weightSeek = 1.0f;
    [Range(0f, 1000f)] public float maxForce = 1.0f;

    [Range(0f, 10f)] public float weightArrive = 1.0f;
    [Range(0f, 20f)] public float slowingRadius = 5.0f;
    [Range(0f, 5f)] public float minDistanceToWaypoint = 0.5f;

    [Range(0f, 10f)] public float avoidanceLength = 5.0f;
    [Range(0f, 10f)] public float weightAvoidance = 5.0f;

    [Range(0f, 10f)] public float weightWallSlowing = 5.0f;
    [Range(0f, 10f)] public float weightPathFollowing = 1.0f;
    [Range(0f, 5f)] public float pathRadius = 1.0f;

    public LayerMask obstacleLayerMask;
    public LayerMask wallLayerMask;


    Vector3 trucante(Vector3 v, float max)
    {
        float size = Mathf.Min(v.magnitude, max);
        return v.normalized * size;
    }

    private Vector3 seek(Agent a, Vector3 target)
    {
        Vector3 position = a.transform.position;
        position.y = 0;
        Vector3 desired = (target - position).normalized * a.GetMaxSpeed();

        Vector3 steering = desired - a.GetVelocity();
        return steering;
    }

    private Vector3 arrive(Agent a, Vector3 target)
    {
        Vector3 position = a.transform.position;
        position.y = 0;

        Vector3 direction = target - position;
        float distance = direction.magnitude;

        float rampedSpeed = a.GetMaxSpeed() * (distance / slowingRadius);
        float clippedSpeed = Mathf.Min(rampedSpeed, a.GetMaxSpeed());
        Vector3 desired = (clippedSpeed / distance) * direction;

        Vector3 steering = desired - a.GetVelocity();

        return steering;
    }

    private Vector3 avoid(Agent a)
    {
        Vector3 steering = Vector3.zero;

        float dynamicLength = a.GetVelocity().magnitude / a.GetMaxSpeed() * avoidanceLength;

        RaycastHit hit;

        if (Physics.SphereCast(a.transform.position, a.GetRadius(), a.GetVelocity().normalized, out hit, dynamicLength, obstacleLayerMask))
        {
            Vector3 toObstacle = hit.collider.gameObject.transform.position - a.transform.position;

            float dot = Vector3.Dot(toObstacle, a.transform.right);
            Vector3 avoidanceDirection;

            if (dot > 0)
            {
                avoidanceDirection = -a.transform.right;
            }
            else
            {
                avoidanceDirection = a.transform.right;
            }

            Vector3 desiredVelocity = avoidanceDirection * a.GetMaxSpeed();

            steering = desiredVelocity - a.GetVelocity();

            float multiplier = 1.0f + (dynamicLength - hit.distance) / dynamicLength;
            steering *= multiplier;
        }

        return steering;
    }

    private Vector3 wallSlowing(Agent a)
    {
        Vector3 steering = Vector3.zero;

        float dynamicLength = a.GetVelocity().magnitude / a.GetMaxSpeed() * avoidanceLength;

        RaycastHit hit;

        if (Physics.SphereCast(a.transform.position, a.GetRadius(), a.GetVelocity().normalized, out hit, dynamicLength, wallLayerMask))
        {
            GameObject obstacle = hit.collider.gameObject;

            Vector3 obsY0 = hit.normal;
            obsY0.y = 0;

            steering = (hit.normal - a.GetVelocity());

            float distanceScale = 1.0f + (dynamicLength - hit.distance) / dynamicLength;
            steering *= distanceScale;
        }
        return steering;
    }

    private Vector3 pathFollowing(Agent a, List<Vector3> polyline)
    {
        if (polyline == null || polyline.Count < 2) return Vector3.zero;

        Vector3 futurePos = a.transform.position + a.GetVelocity() * 0.5f;
        futurePos.y = 0;

        Vector3 targetPoint = Vector3.zero;
        float minDist = float.MaxValue;

        for (int i = 0; i < polyline.Count - 1; i++)
        {
            Vector3 p1 = polyline[i];
            Vector3 p2 = polyline[i + 1];
            p1.y = 0;
            p2.y = 0;

            Vector3 segment = p2 - p1;
            float segmentLength = segment.magnitude;
            if (segmentLength < 0.001f) continue;

            Vector3 segmentDir = segment / segmentLength;
            Vector3 p1_to_future = futurePos - p1;

            float dot = Vector3.Dot(p1_to_future, segmentDir);
            float clampedDot = Mathf.Clamp(dot, 0, segmentLength);

            Vector3 closestPoint = p1 + segmentDir * clampedDot;

            float d = Vector3.Distance(futurePos, closestPoint);
            if (d < minDist)
            {
                minDist = d;
                targetPoint = closestPoint + segmentDir;
            }
        }

        return arrive(a, targetPoint);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public void SetBounds(float xMin, float xMax, float zMin, float zMax)
    {
        this.xMin = xMin;
        this.xMax = xMax;
        this.zMin = zMin;
        this.zMax = zMax;
    }

    public void GetBounds(out float xMin, out float xMax, out float zMin, out float zMax)
    {
        xMin = this.xMin;
        xMax = this.xMax;
        zMin = this.zMin;
        zMax = this.zMax;
    }

    void Start()
    {
        gridRepresentationGenerator = FindFirstObjectByType<GridRepresentationGenerator>();
        crowdGenerator = FindFirstObjectByType<CrowdGenerator>();

        crowdGenerator.initDimensions();

        if (gridRepresentationGenerator != null)
        {
            usingAStar = true;
            grid = new Grid(xMin, xMax, zMin, zMax, gridRepresentationGenerator.getCellSize(), gridRepresentationGenerator.getGridHeight(), gridRepresentationGenerator.getObstacleProbability());
            gridRepresentationGenerator.CreateRepresentation(grid);
            crowdGenerator.GenerateCrowdFromGrid(grid);
        }
        else
        {
            crowdGenerator.GenerateCrowd();
        }

        StartCoroutine(SimulationCoroutine());
    }

    IEnumerator SimulationCoroutine()
    {
        while (true)
        {
            UpdateSimulation(timestep);
            yield return new WaitForSeconds(timestep);
        }
    }

    void UpdateSimulation(float dt)
    {
        if (usingAStar)
        {
            grid.computeGridDensity(agents);

            foreach (Agent agent in agents)
            {
                GridCell currentCell = grid.getGridCell(agent.transform.position);
                GridCell nextCell = agent.getNextWaypoint();

                if(nextCell == null || Vector3.Distance(agent.transform.position, agent.GetGoal()) < minDistanceToWaypoint)
                {
                    agent.ComputePathToGoalAndRecomputeGoalIfNotFound(grid, tries, alternativePathFinding);
                    nextCell = agent.getNextWaypoint();
                }

                if(nextCell == null || currentCell == null)
                {
                    continue;
                }

                bool reached = Vector3.Distance(agent.transform.position, nextCell.getCenter()) < minDistanceToWaypoint;

                if (!reached)
                {
                    Vector3 prevWaypoint = agent.GetPreviousWaypointPosition();
                    Vector3 nextWaypoint = nextCell.getCenter();
                    Vector3 pathDir = (nextWaypoint - prevWaypoint);
                    pathDir.y = 0;
                    pathDir.Normalize();

                    Vector3 toAgent = agent.transform.position - nextWaypoint;
                    toAgent.y = 0;

                    if (Vector3.Dot(pathDir, toAgent) > 0)
                    {
                        reached = true;
                    }
                }

                if (reached || currentCell == nextCell)
                {
                    agent.AdvanceToNextWaypoint();
                    nextCell = agent.getNextWaypoint();
                }

                if (nextCell == null || currentCell == null)
                {
                    continue;
                }

                Vector3 nextCellCenterY0 = nextCell.getCenter();
                nextCellCenterY0.y = 0;
                if (activeForces == SteeringType.None)
                {

                    Vector3 currentPositionY0 = agent.transform.position;
                    currentPositionY0.y = 0;

                    Vector3 direction = (nextCellCenterY0 - currentPositionY0).normalized;

                    agent.SetVelocity(direction * agent.GetMaxSpeed());
                }
                else
                {
                    Vector3 finalForce = Vector3.zero;
                    float weightSum = 0;

                    Vector3 avoidanceForce = Vector3.zero;
                    Vector3 wallSlowingForce = Vector3.zero;
                    if (activeForces.HasFlag(SteeringType.WallSlowing))
                    {
                        wallSlowingForce = wallSlowing(agent);
                        if (wallSlowingForce != Vector3.zero)
                        {
                            weightSum += weightWallSlowing;
                        }
                    }
                    if (activeForces.HasFlag(SteeringType.Avoidance) && wallSlowingForce == Vector3.zero) // Avoidance is only computed if no wall slowing is applied for priority
                    {
                        avoidanceForce = avoid(agent);
                        if (avoidanceForce != Vector3.zero)
                        {
                            weightSum += weightAvoidance;
                        }
                    }
                    if(activeForces.HasFlag(SteeringType.PathFollowing))
                    {
                        weightSum += weightPathFollowing;
                    }
                    if (activeForces.HasFlag(SteeringType.Seek))
                    {
                        weightSum += weightSeek;
                    }
                    if (activeForces.HasFlag(SteeringType.Arrive))
                    {
                        weightSum += weightArrive;
                    }

                    if (weightSum > 0)
                    {
                        if (activeForces.HasFlag(SteeringType.Seek))
                        {
                            finalForce += seek(agent, nextCellCenterY0) * weightSeek / weightSum;
                        }
                        if(activeForces.HasFlag(SteeringType.Arrive))
                        {
                            finalForce += arrive(agent, nextCellCenterY0) * weightArrive / weightSum;
                        }
                        if(activeForces.HasFlag(SteeringType.Avoidance) && avoidanceForce != Vector3.zero)
                        {
                            finalForce += avoidanceForce * weightAvoidance / weightSum;
                        }
                        if(activeForces.HasFlag(SteeringType.WallSlowing) && wallSlowingForce != Vector3.zero)
                        {
                            finalForce += wallSlowingForce * weightWallSlowing / weightSum;
                        }
                        if(activeForces.HasFlag(SteeringType.PathFollowing))
                        {
                            finalForce += pathFollowing(agent, agent.GetPolyline()) * weightPathFollowing / weightSum;
                        }

                        finalForce = trucante(finalForce, maxForce);
                    }
                    Vector3 acceleration = finalForce / agent.GetMass();

                    agent.SetVelocity(agent.GetVelocity() + acceleration * dt);
                    agent.SetAcceleration(acceleration * dt);
                    agent.SetSlowingRadius(slowingRadius);
                }
            }
        }
        else
        {
            foreach (Agent agent in agents)
            {
                if (agent.CheckGoalReached())
                {
                    agent.GenerateRandomGoal(xMin, xMax, zMin, zMax);
                }

                Vector3 goal = agent.GetGoal();
                Vector3 direction = (goal - agent.transform.position).normalized;

                agent.SetVelocity(direction * agent.GetMaxSpeed());
            }
        }
    }

    public void AddAgent(Agent agent)
    {
        agents.Add(agent);
    }

    public void RemoveAgent(Agent agent)
    {
        agents.Remove(agent);
    }

    public List<Agent> GetAgents()
    {
        return agents;
    }
}