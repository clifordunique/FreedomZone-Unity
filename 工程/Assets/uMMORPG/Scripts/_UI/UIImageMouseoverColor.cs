using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace E.Game
{
    public class UIImageMouseoverColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Image image;
        public Color highlightColor = Color.white;
        private Color defaultColor;

        public void OnPointerEnter(PointerEventData pointerEventData)
        {
            defaultColor = image.color;
            image.color = highlightColor;
        }

        public void OnPointerExit(PointerEventData pointerEventData)
        {
            image.color = defaultColor;
        }
    }
}