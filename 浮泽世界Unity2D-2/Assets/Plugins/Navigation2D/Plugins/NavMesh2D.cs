// NavMesh.Raycast etc. ported to 2D
using UnityEngine;
using UnityEngine.AI;

public struct NavMeshHit2D
{
    public Vector2 position;
    public Vector2 normal;
    public float distance;
    public int mask;
    public bool hit;
}

public static class NavMesh2D
{
    public const int AllAreas = -1;

    public static bool Raycast(Vector2 sourcePosition, Vector2 targetPosition, out NavMeshHit2D hit, int areaMask)
    {
        if (NavMesh.Raycast(NavMeshUtils2D.ProjectTo3D(sourcePosition),
                            NavMeshUtils2D.ProjectTo3D(targetPosition),
                            out NavMeshHit hit3D,
                            areaMask))
        {
            hit = new NavMeshHit2D{position = NavMeshUtils2D.ProjectTo2D(hit3D.position),
                                   normal = NavMeshUtils2D.ProjectTo2D(hit3D.normal),
                                   distance = hit3D.distance,
                                   mask = hit3D.mask,
                                   hit = hit3D.hit};
            return true;
        }
        hit = new NavMeshHit2D();
        return false;
    }
}