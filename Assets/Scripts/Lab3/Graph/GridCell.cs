using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PathFinding;

public class GridCell : Node 
{
    protected float xMin;
    protected float xMax;
    protected float zMin;
    protected float zMax;

    protected bool occupied;
    protected Vector3 center;

    protected int count;

    public GridCell(int i):base(i) {
        occupied = false;
    }
    public GridCell(GridCell n):base(n) {
		xMin = n.xMin;
        xMax = n.xMax;
        zMin = n.zMin;
        zMax = n.zMax;
        occupied = n.occupied;
        center = n.center;
        count = 0;
    }

    public int getCount() { return count; }
    public void setCount(int c) { count = c; }
    public void incrementCount() { count++; }
    public void decrementCount() { count--; }
    public bool isOccupied() { return occupied; }
    public Vector3 getCenter() { return center; }
    public void setOccupied(bool occ) { occupied = occ; }
    public void setCenter(Vector3 c) { center = c; }
    public void setBounds(float xMin, float xMax, float zMin, float zMax) {
        this.xMin = xMin;
        this.xMax = xMax;
        this.zMin = zMin;
        this.zMax = zMax;

        setCenter(new Vector3((xMax + xMin) / 2f, 0, (zMax + zMin) / 2f));
    }
    public void getBounds(out float xMin, out float xMax, out float zMin, out float zMax) {
        xMin = this.xMin;
        xMax = this.xMax;
        zMin = this.zMin;
        zMax = this.zMax;
    }

    // Your class that represents a grid cell node derives from Node

    // You add any data needed to represent a grid cell node


    // You also add any constructors and methods to implement your grid cell node class

    // TO IMPLEMENT
};
