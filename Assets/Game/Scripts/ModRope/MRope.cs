using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MRope : MonoBehaviour
{
    public Transform root;
    public Transform target;

    public float gravityScale = 0.5f;

    [HideInInspector]
    public List<Point> points = new List<Point>();
    [HideInInspector]
    public List<Stick> sticks = new List<Stick>();

    public void Generate(float _length, int _segs) {
        if (root == null || target == null) return;
        points.Clear();
        sticks.Clear();

        float seglength = _length / (float)_segs;
        for (int i = 0; i < _segs; i++) {
            Vector3 pos = root.position + (target.position - root.position) * (float)i / (float)_segs;
            bool locked = i == 0 || i == _segs - 1;
            points.Add(new Point(pos, locked));
            
            if (i != 0) sticks.Add(new Stick(points[i - 1], points[i], seglength));
        }
    }

    public void GenSquare() {
        points.Add(new Point(new Vector3(0, 5, 0)));
        points.Add(new Point(new Vector3(0, 6, 0)));
        points.Add(new Point(new Vector3(1, 5, 0)));
        points.Add(new Point(new Vector3(1, 6, 0.1f)));

        sticks.Add(new Stick(points[0], points[1]));
        sticks.Add(new Stick(points[1], points[2]));
        sticks.Add(new Stick(points[2], points[3]));
        sticks.Add(new Stick(points[3], points[0]));

        sticks.Add(new Stick(points[0], points[2]));
        sticks.Add(new Stick(points[1], points[3]));
    }

    private void Start() {
        Generate(4.0f, 64);
        // GenSquare();
    }

    private void Update() {
        System.Action<Point> collision_check = (Point p) => {
            if (p.locked) return;

            var ray_dir = Vector3.Normalize(p.position - p.prev_position);
            var ray_length = Vector3.Distance(p.position, p.prev_position);
            Ray ray = new Ray(p.prev_position, ray_dir);

            RaycastHit[] hits = Physics.SphereCastAll(ray, 0.02f, ray_length);

            float min_distance = ray_length;
            Vector3 hit_normal = Vector3.up;
            foreach (var hit in hits) {
                if (Vector3.Dot(hit.normal, ray_dir) > 0) continue;
                if (hit.distance < min_distance) {
                    min_distance = hit.distance;
                    hit_normal = hit.normal;
                }
            }
            if (min_distance < ray_length) {
                Vector3 R = 2 * Vector3.Dot(hit_normal, -ray_dir) * hit_normal + ray_dir;
                Vector3 reflection = R * ray_length;
                p.position = p.prev_position + ray_dir * min_distance;
                p.position += (ray_length - min_distance) * 0.5f * R;
                p.prev_position = p.position - reflection * 0.5f;
            }

        };
        
        for (int i = 0; i < points.Count; i++) {
            var p = points[i];

            if (p.locked) continue;

            Vector3 posbefore = p.position;
            Vector3 v = (p.position - p.prev_position) * 0.9f + Vector3.down * gravityScale * Time.deltaTime;
            p.position += v;
            p.prev_position = posbefore;

        }

        for (int i = 0; i < 6; i++) {
            foreach (var s in sticks) {
                if (s.start == null || s.end == null) continue;
                Vector3 center = (s.start.position + s.end.position) * 0.5f;
                Vector3 start2end = Vector3.Normalize(s.end.position - s.start.position);
                if (!s.start.locked) s.start.position = center - start2end * s.length * 0.5f;
                if (!s.end.locked) s.end.position = center + start2end * s.length * 0.5f;
            }
        }

        points.ForEach(p => collision_check(p));
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        for (int i = 0; i < sticks.Count; i++) {
            Gizmos.DrawLine(sticks[i].start.position, sticks[i].end.position);
        }
    }

    public class Point
    {
        public Vector3 position;
        public Vector3 prev_position;
        public bool locked;

        public Point(Vector3 _pos, bool _locked = false) {
            prev_position = position = _pos;
            locked = _locked;
        }
    }
    public class Stick
    {
        public Point start;
        public Point end;
        public float length;

        public Stick(Point _start, Point _end) {
            start = _start;
            end = _end;
            length = Vector3.Distance(_start.position, _end.position);
        }

        public Stick(Point _start, Point _end, float _length) {
            start = _start;
            end = _end;
            length = _length;
        }
    }

    [CustomEditor(typeof(MRope))]
    public class RopeEditor : Editor
    {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (GUILayout.Button("Generate")) {
                (target as MRope).Generate(16.0f, 32);
            }
        }
    }

}
