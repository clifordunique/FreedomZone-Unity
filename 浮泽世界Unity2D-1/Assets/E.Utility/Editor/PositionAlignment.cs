// ========================================================
// 作者：E Star
// 创建时间：2018-12-29 00:47:53
// 当前版本：1.0
// 作用描述：
// 挂载目标：
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E.Utility
{
    [ExecuteInEditMode]
    public class PositionAlignment : MonoBehaviour
    {
#if UNITY_EDITOR
        private void OnEnable()
        {
            RaycastHit hit;
            if (Physics.Raycast(gameObject.transform.position + new Vector3(0, 1000, 0), -transform.up, out hit, 2000f, LayerMask.GetMask("Terrain")))
            {
                gameObject.transform.position -= new Vector3(0, hit.distance - 1000, 0);
                //Debug.Log(transform.position.y);
                //Debug.DrawLine(ray.origin, hit.point, Color.red);
            }
        }
#endif
    }
}