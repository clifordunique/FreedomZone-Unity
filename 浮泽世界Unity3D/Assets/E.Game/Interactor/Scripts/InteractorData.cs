// ========================================================
// 作者：E Star
// 创建时间：2019-02-03 00:22:24
// 当前版本：1.0
// 作用描述：
// 挂载目标：
// ========================================================
using HighlightingSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using E.Utility;

namespace E.Game
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public class InteractorData : MonoBehaviour
    {
        //[Header("交互属性")]
        [SerializeField, Tooltip("是否可调查")] private bool m_IsSurveyable = true;
        [SerializeField, Tooltip("是否可拾取")] private bool m_IsPickable = true;
        [SerializeField, Tooltip("是否可破坏")] private bool m_IsDestructible = true;
        [SerializeField, Tooltip("是否可作为容器")] private bool m_IsContainer = false;
        [SerializeField, Tooltip("是否是独立个体")] private bool m_IsIndividual = true;
        //[Header("静态信息")]
        [SerializeField, Tooltip("名称")] private string m_ThisName = "未命名对象";
        [SerializeField, Tooltip("简介")] private string m_Describe = "一个从未见过的东西";
        [SerializeField, Tooltip("图标")] private Sprite m_Icon = null;
        [SerializeField, Tooltip("价值")] private int m_Price = 0;
        [SerializeField, Tooltip("耐久度上限"), Range(0, 10000)] private int m_HealthMax = 1;
        [SerializeField, Tooltip("容量上限，单位dm3"), Range(0, 1000)] private float m_CapacityMax = 0;
        [SerializeField, Tooltip("载重上限，单位kg"), Range(0, 1000)] private float m_LoadMax = 0;
        [SerializeField, Tooltip("组成")] private List<GameObject> m_Component = new List<GameObject>();
        //[Header("动态信息")]
        [SerializeField, Tooltip("当前耐久度/生命值"), Range(0, 10000)] private int m_HealthNow = 1;
        [SerializeField, Tooltip("体积，单位dm3"), Range(0, 1000)] private float m_Volume = 0;
        [SerializeField, Tooltip("质量，单位kg"), Range(0, 1000)] private float m_Quality = 0;
        [SerializeField, Tooltip("容纳的物品")] private List<GameObject> m_CarryingItems = new List<GameObject>();
        [SerializeField, ReadOnly, Tooltip("当前已占用容量"), Range(0, 1000)] private float m_CapacityUsedNow = 0;
        [SerializeField, ReadOnly, Tooltip("当前已占用载重"), Range(0, 1000)] private float m_LoadUsedNow = 0;
        //[Header("临时变量")]
        public Vector3 m_V1 = Vector3.zero;
        private Rigidbody m_Rigidbody;

        //交互属性
        public bool IsSurveyable { get => m_IsSurveyable; }
        public bool IsPickable { get => m_IsPickable; }
        public bool IsDestructible { get => m_IsDestructible; }
        public bool IsContainer { get => m_IsContainer; }
        public bool IsIndividual { get => m_IsIndividual; }
        //静态信息
        public string Name { get => m_ThisName; }
        public string Describe { get => m_Describe; }
        public Sprite Icon { get => m_Icon; }
        public int Price { get => m_Price; }
        public int HealthMax { get => m_HealthMax; }
        public float CapacityMax { get => m_CapacityMax; }
        public float LoadMax { get => m_LoadMax; }
        public List<GameObject> Component { get => m_Component; }
        //动态信息
        public int HealthNow
        {
            get => m_HealthNow;
            set
            {
                if (IsDestructible)
                {
                    if (value <= 0)
                    {
                        m_HealthNow = 0;
                        Destory();
                    }
                    else
                    {
                        m_HealthNow = value;
                    }
                }
                else
                {
                    Debug.LogWarning("对象不可破坏：" + Name);
                }
            }
        }
        public float Volume { get => m_Volume; set => m_Volume = value; }
        public float Quality
        {
            get => m_Quality;
            set
            {
                m_Quality = value;
                m_Rigidbody.mass = value;
            }
        }
        public List<GameObject> CarryingItems
        {
            get
            {
                if (IsContainer)
                {
                    return m_CarryingItems;
                }
                else
                {
                    Debug.LogWarning("对象不是容器：" + Name);
                    return null;
                }
            }
            set
            {
                if (IsContainer)
                {
                    m_CarryingItems = value;

                    List<InteractorData> id = new List<InteractorData>();
                    foreach (GameObject item in CarryingItems)
                    {
                        id.Add(item.GetComponent<InteractorData>());
                    }

                    CapacityUsedNow = 0;
                    foreach (InteractorData item in id)
                    {
                        CapacityUsedNow += item.Volume + item.CapacityUsedNow;
                    }

                    LoadUsedNow = 0;
                    foreach (InteractorData item in id)
                    {
                        LoadUsedNow += item.Quality + item.LoadUsedNow;
                    }
                }
                else
                {
                    Debug.LogWarning("对象不是容器：" + Name);
                }
            }
        }
        public float CapacityUsedNow { get => m_CapacityUsedNow; private set => m_CapacityUsedNow = value; }
        public float LoadUsedNow { get => m_LoadUsedNow; private set => m_LoadUsedNow = value; }



        private void Start()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Rigidbody.mass = Quality;
        }

        public void PickUp()
        {
            StartCoroutine(MoveTo(new Vector3(0, 0, 0)));
        }
        public void Use()
        {
        }
        public void Hit()
        {
        }
        private void Destory()
        {
        }

        public IEnumerator MoveTo(Vector3 destination)
        {
            for (int i = 0; i < 15; i++)
            {
                yield return new WaitForEndOfFrame();
                //SmoothDamp方法的调用：在smoothTime时间内从当前点以当前速度移动到目标点
                transform.localPosition = Vector3.SmoothDamp(transform.localPosition, destination, ref m_V1, 0.25f);
                //transform.GetComponent<Rigidbody>().MovePosition(destination);
            }
            m_V1 = Vector3.zero;
        }
        public IEnumerator RotateTo(Vector3 destination)
        {
            //float y = transform.eulerAngles.y;
            //transform.eulerAngles = new Vector3(0, y, 0);
            for (int i = 0; i < 15; i++)
            {
                yield return new WaitForEndOfFrame();
                //transform.Rotate(destination,Space.Self);
                //transform.GetComponent<Rigidbody>().MoveRotation(destination);
            }
        }
    }
}