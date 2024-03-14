using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyFunctions
{
    public static bool IsInsideCollider(BoxCollider2D c, Vector3 point)
    {
        Vector3 closest = c.ClosestPoint(point);
        return closest == point;
    }
}
