using System;
using System.Collections;
using UnityEngine;

namespace UnityStandardAssets.Utility
{
    [Serializable]
    public class FOVKick
    {
        [Tooltip("视场角相机")] public Camera Camera;                           // 可选择设置Camera，如果没有设置，则为主Carmera

        [HideInInspector] public float originalFov;                             // fov的初始角度
        [Tooltip("视场角增长值")] public float FOVIncrease = 3f;                  // 进入奔跑状态时，fov的增长值
        [Tooltip("视场角增长完成的秒数")] public float TimeToIncrease = 1f;        // 完成fov增涨的秒数
        [Tooltip("视场角减少完成的秒数")] public float TimeToDecrease = 1f;        // 这是减少，会回归fov的初始值
        [Tooltip("视场角增涨曲线")] public AnimationCurve IncreaseCurve;            // fov值的增涨曲线

        /// <summary>
        /// 设置Camera
        /// </summary>
        /// <param name="camera"></param>
        public void Setup(Camera camera)
        {
            CheckStatus(camera);

            Camera = camera;
            originalFov = camera.fieldOfView;
        }

        /// <summary>
        /// 检查属性状态
        /// </summary>
        /// <param name="camera"></param>
        private void CheckStatus(Camera camera)
        {
            if (camera == null)
            {
                Debug.LogError("视场角相机未设置");
                //throw new Exception();
            }
            if (IncreaseCurve == null)
            {
                Debug.LogError("视场角增涨曲线未设置");
                //throw new Exception("视场角增涨曲线未设置");
            }
        }

        /// <summary>
        /// 更改视场角相机，不过有个问题，没有改变fov初始值
        /// </summary>
        /// <param name="camera"></param>
        public void ChangeCamera(Camera camera)
        {
            Camera = camera;
        }

        /// <summary>
        /// 视场角增加
        /// </summary>
        /// <returns></returns>
        public IEnumerator FOVKickUp()
        {
            // 计算当前的fov值进行到了多少秒，有的人可能说直接设置为0不就行了，不过他们忘记了fov值没有完全回归初始值的情况，从中途开始增加，就是要通过计算了，当然从初始的fov值开始计算出来的就是0
            float t = Mathf.Abs((Camera.fieldOfView - originalFov)/FOVIncrease);
            while (t < TimeToIncrease)
            {
                // 根据增涨曲线和时间计算当前的fov值，直到t值大于等于完成时间，跳出循环
                Camera.fieldOfView = originalFov + (IncreaseCurve.Evaluate(t/TimeToIncrease)*FOVIncrease);
                t += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        /// <summary>
        /// 视场角减小
        /// </summary>
        /// <returns></returns>
        public IEnumerator FOVKickDown()
        {
            float t = Mathf.Abs((Camera.fieldOfView - originalFov)/FOVIncrease);
            while (t > 0)
            {
                Camera.fieldOfView = originalFov + (IncreaseCurve.Evaluate(t/TimeToDecrease)*FOVIncrease);
                t -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            //保证fov值回归了初始值
            Camera.fieldOfView = originalFov;
        }
    }
}
