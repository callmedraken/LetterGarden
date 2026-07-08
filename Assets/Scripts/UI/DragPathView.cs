using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LetterGarden.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class DragPathView : MaskableGraphic
    {
        [SerializeField] private float lineThickness = 14f;
        [SerializeField] private bool showDebugPath;

        private readonly List<Vector2> pathPoints = new List<Vector2>();

        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            lineThickness = Mathf.Max(1f, lineThickness);
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (showDebugPath)
            {
                AddSegment(vh, new Vector2(-150f, -50f), new Vector2(0f, 100f));
                AddSegment(vh, new Vector2(0f, 100f), new Vector2(150f, -50f));
                return;
            }

            if (pathPoints.Count < 2)
            {
                return;
            }

            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                AddSegment(vh, pathPoints[i], pathPoints[i + 1]);
            }
        }

        protected override void Reset()
        {
            base.Reset();
            raycastTarget = false;
        }

        public void SetPath(IReadOnlyList<Vector2> points)
        {
            if (PathMatches(points))
            {
                return;
            }

            pathPoints.Clear();

            if (points != null)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    pathPoints.Add(points[i]);
                }
            }

            SetVerticesDirty();
        }

        public void Clear()
        {
            if (pathPoints.Count == 0)
            {
                return;
            }

            pathPoints.Clear();
            SetVerticesDirty();
        }

        public void SetPathFromTransforms(IReadOnlyList<RectTransform> pointTransforms)
        {
            if (pointTransforms == null || pointTransforms.Count == 0)
            {
                Clear();
                return;
            }

            List<Vector2> localPoints = new List<Vector2>(pointTransforms.Count);

            for (int i = 0; i < pointTransforms.Count; i++)
            {
                RectTransform pointTransform = pointTransforms[i];
                if (pointTransform == null)
                {
                    continue;
                }

                Vector3 worldCenter = pointTransform.TransformPoint(pointTransform.rect.center);
                localPoints.Add(rectTransform.InverseTransformPoint(worldCenter));
            }

            SetPath(localPoints);
        }

        private bool PathMatches(IReadOnlyList<Vector2> points)
        {
            int pointCount = points == null ? 0 : points.Count;
            if (pathPoints.Count != pointCount)
            {
                return false;
            }

            for (int i = 0; i < pointCount; i++)
            {
                if (pathPoints[i] != points[i])
                {
                    return false;
                }
            }

            return true;
        }

        private void AddSegment(VertexHelper vh, Vector2 start, Vector2 end)
        {
            Vector2 direction = end - start;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            Vector2 normal = new Vector2(-direction.y, direction.x).normalized * (lineThickness * 0.5f);
            int vertexIndex = vh.currentVertCount;
            Color32 vertexColor = color;

            vh.AddVert(start - normal, vertexColor, Vector2.zero);
            vh.AddVert(start + normal, vertexColor, Vector2.zero);
            vh.AddVert(end + normal, vertexColor, Vector2.zero);
            vh.AddVert(end - normal, vertexColor, Vector2.zero);

            vh.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + 2);
            vh.AddTriangle(vertexIndex + 2, vertexIndex + 3, vertexIndex);
        }
    }
}
