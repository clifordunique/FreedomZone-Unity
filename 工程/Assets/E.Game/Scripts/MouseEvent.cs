using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using E.Utility;
//using E.Tool;

namespace E.Game
{
    public class MouseEvent : MonoBehaviour
    {
        private void OnMouseEnter()
        {
            GetComponent<SpriteRenderer>().color = new Color(0.8f, 0.8f, 0.8f);
        }
        private void OnMouseOver()
        {
            if (Input.GetMouseButtonUp(1))
            {
                Debug.Log("右键点击了：" + name);
            }
        }
        private void OnMouseDown()
        {
            GetComponent<SpriteRenderer>().color = new Color(0.6f, 0.6f, 0.6f);
        }
        private void OnMouseDrag()
        {
            
        }
        private void OnMouseUp()
        {
            GetComponent<SpriteRenderer>().color = new Color(0.8f, 0.8f, 0.8f);
        }
        private void OnMouseExit()
        {
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);
        }
    }
}
