using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{ 
[RequireComponent(typeof(LineRenderer))]
public class MRope : MonoBehaviour
{
    [SerializeField]
    private bool generateOnStart = true;

    public Transform SideA;
    public Transform SideB;

    private LineRenderer lineRenderer;

    public Rope rope;

    private RopeSystem.Point pA;
    private RopeSystem.Point pB;

    void Start()
    {
        if (SideA == null || SideB == null) return;
        rope = new Rope();
        rope.Generate(SideA.position, SideB.position, Vector3.Distance(SideA.position, SideB.position), 16);
        pA = rope.Points[0];
        pB = rope.Points[1];
        rope.Register();

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 16;
    }

    private void Update()
    {
        pA.position = SideA.position;
        pB.position = SideB.position;
        for (int i = 0; i < lineRenderer.positionCount; i++) {
            lineRenderer.SetPosition(i, rope.Points[i].position);
        }
    }

    void Destroy() {
        rope.Unregister();
    }
}
}
