using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawSphere : MonoBehaviour
{
    [SerializeField] private float radius;

    public float Radius { get { return radius; } set { radius = value; } }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
        
    }
}
