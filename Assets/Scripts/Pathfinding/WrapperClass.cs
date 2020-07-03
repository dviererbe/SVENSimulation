using UnityEngine;
using UnityEditor;
using Assets.Scripts.Pathfinding;

public class WrapperClass
{
    public WrapperClass(Vertex vertex, float cost)
    {
        this.Vertex = vertex;
        this.Cost = cost;
    }

    public Vertex Vertex { get; private set; }
    public float Cost { get; private set; }
}