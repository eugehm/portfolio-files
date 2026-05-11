using UnityEngine;

public class VisibleMarkerInEditor : MonoBehaviour
{
    public Color color = Color.red;
    public float radius = 0.5f;

    void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position, radius);
    }
}
