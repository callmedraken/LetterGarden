using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LetterGarden.UI
{
    [RequireComponent(typeof(Button))]
    public class LetterButtonInput : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler
    {
        private DevGameController controller;

        public int LetterIndex { get; private set; }

        public void Initialize(int index, DevGameController controller)
        {
            LetterIndex = index;
            this.controller = controller;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (controller != null)
            {
                controller.BeginLetterDrag(LetterIndex);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (controller != null)
            {
                controller.ContinueLetterDrag(LetterIndex);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (controller != null)
            {
                controller.EndLetterDrag();
            }
        }
    }
}
