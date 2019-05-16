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
            GetComponent<SpriteRenderer>().color = new Color(0.7f, 0.7f, 0.7f);
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
            GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f);
        }
        private void OnMouseDrag()
        {
            
        }
        private void OnMouseUp()
        {
            GetComponent<SpriteRenderer>().color = new Color(0.7f, 0.7f, 0.7f);
        }
        private void OnMouseExit()
        {
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);
        }
    }
}
