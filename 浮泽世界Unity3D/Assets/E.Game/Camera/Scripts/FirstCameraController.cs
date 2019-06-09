// ========================================================
// 作者：E Star 
// 创建时间：2018-12-21 13:33:17 
// 当前版本：1.0 
// 作用描述：第一人称摄像机控制器
// 挂载目标：FirstCamera
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class FirstCameraController : MonoBehaviour
{
    #region 公开字段
    [Tooltip("跟随的目标坐标")]
    public Transform m_Target;
    [Tooltip("摄像机相对视角的前偏移系数")]
    public float m_TargetDistanceZ = 0.1f;
    [Tooltip("摄像机相对视角的上偏移系数")]
    public float m_TargetDistanceY = 0.1f;

    [Tooltip("转向模式")]
    public RotationMode m_Axes = RotationMode.MouseY;
    [Tooltip("X轴转向灵敏度（左右转向）")]
    public float m_SensitivityX = 2f;
    [Tooltip("Y轴转向灵敏度（上下转向）")]
    public float m_SensitivityY = 2f;
    [Tooltip("X轴最小转向范围（最左视角）")]
    public float m_MinimumX = -85f;
    [Tooltip("X轴最大转向范围（最右视角）")]
    public float m_MaximumX = 85f;
    [Tooltip("Y轴最小转向范围（最下视角）")]
    public float m_MinimumY = -75f;
    [Tooltip("Y轴最大转向范围（最上视角）")]
    public float m_MaximumY = 75f;
    [Tooltip("X轴镜头转向角度")]
    private float m_RotationX = 0f;
    [Tooltip("Y轴镜头转向角度")]
    private float m_RotationY = 0f;

    #endregion


    #region 私有字段
    /// <summary>
    /// 平滑运动速度
    /// </summary>
    private Vector3 m_Velocity = Vector3.zero;
    #endregion


    #region MonoBehaviour方法
    void Start()
    {
    }
    void Update()
    {

        //锁定光标时
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            if (Input.GetKeyUp(KeyCode.LeftAlt))
            {
                m_Axes = RotationMode.MouseY;
            }
            if (Input.GetKey(KeyCode.LeftAlt))
            {
                if (m_Axes != RotationMode.MouseXAndY)
                { m_Axes = RotationMode.MouseXAndY; m_RotationX = 0f; }
            }
            else
            {
                //恢复视角
                if (transform.localEulerAngles.y > 0 & transform.localEulerAngles.y <= 180)
                {
                    transform.localEulerAngles = Vector3.SmoothDamp(transform.localEulerAngles, Vector3.zero, ref m_Velocity, 0.2f);
                }
                else if (transform.localEulerAngles.y > 180)
                {
                    Vector3 temp_01 = new Vector3(transform.localEulerAngles.x, 360f - transform.localEulerAngles.y, 0);
                    Vector3 temp_02 = Vector3.SmoothDamp(temp_01, Vector3.zero, ref m_Velocity, 0.2f);
                    transform.localEulerAngles = new Vector3(temp_02.x, 360f - temp_02.y, 0);
                }
            }

            //旋转XY轴
            if (m_Axes == RotationMode.MouseXAndY)
            {
                //实时同步跟随的目标位置
                transform.position = m_Target.position + m_Target.up * m_TargetDistanceZ + -m_Target.right * m_TargetDistanceY;
                //检测XY轴视角旋转
                m_RotationX += Input.GetAxis("Mouse X") * m_SensitivityX;
                m_RotationX = Mathf.Clamp(m_RotationX, m_MinimumX, m_MaximumX);
                m_RotationY += Input.GetAxis("Mouse Y") * m_SensitivityY;
                m_RotationY = Mathf.Clamp(m_RotationY, m_MinimumY, m_MaximumY);
                transform.localEulerAngles = new Vector3(-m_RotationY, m_RotationX, 0);
            }
            //旋转X轴
            else if (m_Axes == RotationMode.MouseX)
            {
                //检测X轴视角旋转
                transform.Rotate(0, Input.GetAxis("Mouse X") * m_SensitivityX, 0);
            }
            //旋转Y轴
            else
            {
                //实时同步跟随的目标位置
                transform.position =m_Target.position + m_Target.up * m_TargetDistanceZ + -m_Target.right * m_TargetDistanceY;
                //检测Y轴视角旋转
                m_RotationY += Input.GetAxis("Mouse Y") * m_SensitivityY;
                m_RotationY = Mathf.Clamp(m_RotationY, m_MinimumY, m_MaximumY);
                transform.localEulerAngles = new Vector3(-m_RotationY, transform.localEulerAngles.y, 0);
            }
        }
    }
    #endregion


    #region 公开方法
    #endregion


    #region 私有方法
    #endregion
}


/// <summary>
/// 视角旋转模式
/// </summary>
public enum RotationMode
{
    /// <summary>
    /// 旋转X轴和Y轴
    /// </summary>
    MouseXAndY = 0,
    /// <summary>
    /// 旋转X轴
    /// </summary>
    MouseX = 1,
    /// <summary>
    /// 旋转Y轴
    /// </summary>
    MouseY = 2
}