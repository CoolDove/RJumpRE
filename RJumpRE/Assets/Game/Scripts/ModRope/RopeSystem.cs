using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using dove;

namespace Game
{ 
[GameSystem]
public class RopeSystem : IGameSystem
{
    public float gravityScale = 0.5f;
    public int iteratingTimes = 6;

    public List<Point> Points { get => points; }
    public List<Stick> Sticks { get => sticks; }

    private List<Point> points = new List<Point>();
    private List<Stick> sticks = new List<Stick>();

    public GameMain Game { get; set; }

    private List<Rope> ropes = new List<Rope>();

    public void OnInit() {

    }
    public void OnRelease() {

    }

    public void RegisterRope(Rope rope) {
        if (!ropes.Contains(rope)) {
            ropes.Add(rope);
        }
    }
    public void UnregisterRope(Rope rope) {
        if (ropes.Contains(rope)) {
            ropes.Remove(rope);
        }
    }
    
    private void Update() {
        foreach (var r in ropes) {
            r.Update();
        }
    }

    public void OnDrawGizmos() {
        foreach (var r in ropes) {
            r.OnDrawGizmos();
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
}
public class Rope {
    public List<RopeSystem.Point> Points { get => points; }
    public List<RopeSystem.Stick> Sticks { get => sticks; }

    private List<RopeSystem.Point> points = new List<RopeSystem.Point>();
    private List<RopeSystem.Stick> sticks = new List<RopeSystem.Stick>();

    private RopeSystem ropeSys { get => GameMain.GetSystem<RopeSystem>(); }

    public Rope() {
    }

    public void Generate(Vector3 start, Vector3 end, float _length, int _segs) {
        points.Clear();
        sticks.Clear();

        float seglength = _length / _segs;
        for (int i = 0; i < _segs; i++) {
            Vector3 pos = start + (end - start) * i / _segs;
            bool locked = i == 0 || i == _segs - 1;
            points.Add(new RopeSystem.Point(pos, locked));
            if (i != 0) sticks.Add(new RopeSystem.Stick(points[i - 1], points[i], seglength));
        }
    }
    
    public void Update() {
        for (int i = 0; i < points.Count; i++) {
            var p = points[i];
            if (p.locked) continue;
            Vector3 posbefore = p.position;
            Vector3 v = (p.position - p.prev_position) * 0.9f + Vector3.down * ropeSys.gravityScale * Time.deltaTime;
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
    }

    public void OnDrawGizmos() {
        Gizmos.color = Color.green;
        for (int i = 0; i < sticks.Count; i++) {
            Gizmos.DrawLine(sticks[i].start.position, sticks[i].end.position);
        }
    }

    public void Register() {
        ropeSys.RegisterRope(this);
    }
    public void Unregister() {
        ropeSys.UnregisterRope(this);
    }

}
}
