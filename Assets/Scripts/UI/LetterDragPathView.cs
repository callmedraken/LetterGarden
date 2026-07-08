using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LetterGarden.UI
{
    public class LetterDragPathView : MonoBehaviour
    {
        [SerializeField] private RectTransform pathContainer;
        [SerializeField] private float segmentThickness = 12f;
        [SerializeField] private Color segmentColor = new Color(0.35f, 0.7f, 0.45f, 0.85f);

        private readonly List<GameObject> segments = new List<GameObject>();

        public void Clear()
        {
            foreach (GameObject segment in segments)
            {
                if (segment != null)
                {
                    Destroy(segment);
                }
            }

            segments.Clear();
        }

        public void AddConnection(RectTransform from, RectTransform to)
        {
            RectTransform container = GetPathContainer();
            if (container == null || from == null || to == null)
            {
                return;
            }

            Vector2 startPosition = container.InverseTransformPoint(from.position);
            Vector2 endPosition = container.InverseTransformPoint(to.position);
            Vector2 direction = endPosition - startPosition;
            float distance = direction.magnitude;

            if (distance <= 0f)
            {
                return;
            }

            GameObject segment = new GameObject("Letter Drag Segment", typeof(RectTransform), typeof(Image));
            segment.transform.SetParent(container, false);
            segment.transform.SetAsFirstSibling();
            segments.Add(segment);

            RectTransform segmentTransform = segment.GetComponent<RectTransform>();
            segmentTransform.anchorMin = new Vector2(0.5f, 0.5f);
            segmentTransform.anchorMax = new Vector2(0.5f, 0.5f);
            segmentTransform.pivot = new Vector2(0f, 0.5f);
            segmentTransform.anchoredPosition = startPosition;
            segmentTransform.sizeDelta = new Vector2(distance, segmentThickness);
            segmentTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

            Image image = segment.GetComponent<Image>();
            image.color = segmentColor;
            image.raycastTarget = false;
        }

        private RectTransform GetPathContainer()
        {
            if (pathContainer != null)
            {
                return pathContainer;
            }

            return transform as RectTransform;
        }
    }
}
