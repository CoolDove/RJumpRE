using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{ 
public class MRopeShaker : MonoBehaviour
{
    public Transform ropeHandle;
    public float radius;
    public float angVelocity;

    public float length;

    private float angle = 0;

    private void Update()
    {
        if (ropeHandle == null) return;

        Vector3 targetPos = Vector3.up * length;
        Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
        targetPos = rot * targetPos;
        ropeHandle.localPosition = targetPos;

        angle += angVelocity * Time.deltaTime;
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position - transform.forward * 0.5f, transform.position + transform.forward * 0.5f);
        Gizmos.DrawSphere(transform.position, 0.05f);

        if (ropeHandle != null) {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(ropeHandle.position, 0.1f);
        }
    }

}
}
