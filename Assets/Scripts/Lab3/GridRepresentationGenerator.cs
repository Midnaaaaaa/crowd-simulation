using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Necesario para usar Listas

public class GridRepresentationGenerator : MonoBehaviour
{
    [SerializeField] float cellSize;
    [SerializeField] float gridHeight;

    [Range(0f, 1f)]
    [SerializeField] float obstacleProbability;
    [SerializeField] GameObject floorPrefab;
    [SerializeField] GameObject wallPrefab;

    [SerializeField] GameObject[] propPrefabs;
    [SerializeField] int numProps;
    [SerializeField] int tries;

    List<GameObject> props;

    public float getCellSize() { return cellSize; }
    public float getGridHeight() { return gridHeight; }
    public float getObstacleProbability() { return obstacleProbability; }

    public List<GameObject> getProps() { return props; }

    public void Start()
    {
        props = new List<GameObject>();
    }

    public void CreateRepresentation(Grid grid)
    {
        float xMin, xMax, zMin, zMax;
        float height;

        grid.getBounds(out xMin, out xMax, out zMin, out zMax);
        height = grid.getGridHeight();

        GameObject wallsParent = new GameObject("Walls");
        wallsParent.transform.SetParent(transform);
        wallsParent.transform.localPosition = Vector3.zero;

        GameObject propsParent = new GameObject("Props");
        propsParent.transform.SetParent(transform);
        propsParent.transform.localPosition = Vector3.zero;

        MeshFilter mfFloor = floorPrefab.GetComponent<MeshFilter>();
        Vector3 floorSize = mfFloor.sharedMesh.bounds.size;

        MeshFilter mfWall = wallPrefab.GetComponent<MeshFilter>();
        Vector3 wallSize = mfWall.sharedMesh.bounds.size;

        GameObject floor = Instantiate(floorPrefab, transform.position, Quaternion.identity);
        floor.transform.localScale = new Vector3((xMax - xMin) / floorSize.x, 1.0f, (zMax - zMin) / floorSize.z);
        floor.transform.position = new Vector3((xMin + xMax) / 2.0f, 0.0f, (zMin + zMax) / 2.0f);
        floor.transform.SetParent(transform);

        List<Bounds> wallBoundsList = new List<Bounds>();

        for (int i = 0; i < grid.getNumNodes(); i++)
        {
            GridCell cell = grid.getNode(i);
            float cellxMin, cellxMax, cellzMin, cellzMax;
            cell.getBounds(out cellxMin, out cellxMax, out cellzMin, out cellzMax);

            if (cell.isOccupied())
            {
                Vector3 center = cell.getCenter();
                GameObject wall = Instantiate(wallPrefab, transform.position, Quaternion.identity);
                wall.transform.localScale = new Vector3((cellxMax - cellxMin) / wallSize.x, height, (cellzMax - cellzMin) / wallSize.z);
                wall.transform.position = new Vector3(center.x, height / 2, center.z);

                wall.transform.SetParent(wallsParent.transform);

                Renderer wallRend = wall.GetComponent<Renderer>();
                if (wallRend != null)
                {
                    wallBoundsList.Add(wallRend.bounds);
                }
            }
        }

        if (propPrefabs.Length > 0)
        {
            Bounds floorBounds = floor.GetComponent<MeshRenderer>().bounds;

            for (int i = 0; i < numProps; i++)
            {
                GameObject propPrefab = propPrefabs[Random.Range(0, propPrefabs.Length)];

                Renderer prefabRenderer = propPrefab.GetComponentInChildren<MeshRenderer>();
                if (prefabRenderer == null) continue;

                Vector3 propSize = prefabRenderer.bounds.size;
                Vector3 centerOffset = prefabRenderer.bounds.center - propPrefab.transform.position;

                for (int j = 0; j < tries; j++)
                {
                    float x = Random.Range(floorBounds.min.x, floorBounds.max.x);
                    float z = Random.Range(floorBounds.min.z, floorBounds.max.z);

                    Vector3 candidatePos = new Vector3(x, 0, z);

                    if (grid.getGridCell(candidatePos).isOccupied() == false)
                    {
                        Vector3 worldCenterOfProp = candidatePos + centerOffset;
                        Bounds virtualPropBounds = new Bounds(worldCenterOfProp, propSize);

                        if (Vector3.Magnitude(grid.getGridCell(candidatePos).getCenter() - worldCenterOfProp) < 2) continue;

                        bool hitWall = false;
                        foreach (Bounds wBounds in wallBoundsList)
                        {
                            if (virtualPropBounds.Intersects(wBounds))
                            {
                                hitWall = true;
                                break;
                            }
                        }

                        if (!hitWall)
                        {
                            GameObject prop = Instantiate(propPrefab, transform.position, Quaternion.identity);
                            prop.transform.position = candidatePos;
                            prop.transform.SetParent(propsParent.transform);
                            props.Append(prop);
                            break;
                        }
                    }
                }
            }
        }
    }
}