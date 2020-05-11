using UnityEngine;
using UnityEditor;

public class WallDetails
{
    public static readonly float wallThickness = 1.0f;
    public static readonly int wallsPerGrid = 3;
    public static readonly int height = 30;
    public static readonly int width = 30;
    public static readonly float wallSize = wallThickness / wallsPerGrid;
}