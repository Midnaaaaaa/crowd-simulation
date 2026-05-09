using NUnit.Framework;
using PathFinding;
using UnityEngine;
using System.Collections.Generic;

public class PathManager
{
    private Vector3 goal;
    List<GridCell> path;
    GridCell nextWaypoint;
    private Vector3 previousWaypointPosition;
    List<Vector3> polyline;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public PathManager()
    {
        goal = Vector3.zero;
        path = new List<GridCell>();
        previousWaypointPosition = Vector3.zero;
        polyline = new List<Vector3>();
    }

    public Vector3 GetGoal() {
        return goal;
    }

    public void SetGoal(Vector3 goal) {
        this.goal = goal;
    }

    public int ComputePathToGoalAndRecomputeGoalIfNotFound(Grid grid, GridCell startCell, bool alternativePathFinding, int triesForRecomputation = 10)
    {
        if (alternativePathFinding)
        {
            for (int i = 0; i < triesForRecomputation; ++i)
            {
                GridCell endCell = grid.getGridCell(goal);
                GridHeuristic gh = new GridHeuristic(endCell);

                Grid_Bidirectional_A_Star grid_A_Star = new Grid_Bidirectional_A_Star(10000, 10000, 10000);

                int found = -1;
                path = grid_A_Star.findpath(grid, startCell, endCell, gh, ref found);

                if (found == 1 && path.Count >= 1) //The and is to avoid edge case where startCell == endCell (we do not add startCell/endCell to path thus path has size 0)
                {
                    polyline.Clear();
                    foreach (GridCell cell in path)
                    {
                        polyline.Add(cell.getCenter());
                    }
                    previousWaypointPosition = startCell.getCenter();
                    nextWaypoint = path[0];
                    path.RemoveAt(0);
                    return found;
                }
                else
                {
                    // recompute goal
                    float xMin, xMax, zMin, zMax;
                    grid.getBounds(out xMin, out xMax, out zMin, out zMax);
                    goal = new Vector3(Random.Range(xMin, xMax), 0, Random.Range(zMin, zMax));
                }

            }
            return -1;
        }
        else
        {
            for (int i = 0; i < triesForRecomputation; ++i)
            {
                GridCell endCell = grid.getGridCell(goal);
                GridHeuristic gh = new GridHeuristic(endCell);

                Grid_A_Star grid_A_Star = new Grid_A_Star(10000, 10000, 10000);

                int found = -1;
                path = grid_A_Star.findpath(grid, startCell, endCell, gh, ref found);

                if (found == 1 && path.Count >= 1) //The and is to avoid edge case where startCell == endCell (we do not add startCell/endCell to path thus path has size 0)
                {
                    polyline.Clear();
                    foreach (GridCell cell in path)
                    {
                        polyline.Add(cell.getCenter());
                    }
                    previousWaypointPosition = startCell.getCenter();
                    nextWaypoint = path[0];
                    path.RemoveAt(0);
                    return found;
                }
                else
                {
                    // recompute goal
                    float xMin, xMax, zMin, zMax;
                    grid.getBounds(out xMin, out xMax, out zMin, out zMax);
                    goal = new Vector3(Random.Range(xMin, xMax), 0, Random.Range(zMin, zMax));
                }

            }
            return -1;
        }
    }

    public List<Vector3> GetPolyline()
    {
        return polyline;
    }

    public GridCell GetNextWaypoint()
    {
        return nextWaypoint;
    }

    public void AdvanceToNextWaypoint()
    {
        if (nextWaypoint != null)
        {
            previousWaypointPosition = nextWaypoint.getCenter();
        }

        if(path.Count > 0)
        {
            nextWaypoint = path[0];
            path.RemoveAt(0);
        }
        else
        {
            nextWaypoint = null;
        }
    }

    public List<GridCell> GetPath()
    {
        return path;
    }

    public Vector3 GetPreviousWaypointPosition()
    {
        return previousWaypointPosition;
    }
}
