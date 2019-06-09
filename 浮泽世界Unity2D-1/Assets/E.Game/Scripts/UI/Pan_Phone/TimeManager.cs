// ========================================================
// 作者：E Star
// 创建时间：2019-01-27 17:38:50
// 当前版本：1.0
// 作用描述：
// 挂载目标：
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace E.Game
{
    public class TimeManager : AppManager
    {
        [Header("日历页")]
        [SerializeField] private GameObject m_CalendarPage;
        [SerializeField] private Transform m_PanDay;
        [SerializeField] private Text m_Year;
        [SerializeField] private Text m_Month;
        [SerializeField] private Text m_Info;
        [SerializeField] private Button m_BtnToday;
        private int m_CurrentYear;
        private int m_CurrentMonth;
        [Header("时钟页")]
        [SerializeField] private GameObject m_ClockPage;
        [SerializeField] private Text m_Time;
        [Header("闹钟页")]
        [SerializeField] private GameObject m_AlarmClockPage;
        [SerializeField] private Transform m_AlarmClockList;
        [SerializeField] private GameObject m_AlarmClockEditPage;
        [SerializeField] private GameObject m_AlarmClockItemTemp;
        [SerializeField] private GameObject m_CurrentAlarmClock;
        [Header("秒表页")]
        [SerializeField] private GameObject m_StopWatchPage;
        [SerializeField] private Transform m_TimeSpanList;
        [SerializeField] private Scrollbar m_TimeSpanListScrollbar;
        [SerializeField] private GameObject m_TimeSpanItemTemp;
        [SerializeField] private Button m_Start;
        [SerializeField] private Button m_Pause;
        [SerializeField] private Button m_Check;
        [SerializeField] private Button m_Clear;
        [SerializeField] private Text m_TimingTime;
        private DateTime m_StartTime;
        private bool m_IsTiming;

        private void Update()
        {
            RefreshClockPage();

            if (m_IsTiming)
            {
                Timing();
            }
        }

        //日历相关
        /// <summary>
        /// 获取今天日期
        /// </summary>
        public void GetCurrentDate()
        {
            //获取年月
            m_CurrentYear = DateTime.Now.Year;
            m_CurrentMonth = DateTime.Now.Month;
        }
        /// <summary>
        /// 高亮今天按钮
        /// </summary>
        public void HighlightToday()
        {
            //高亮日期按钮
            m_BtnToday = m_PanDay.GetChild(DateTime.Now.Day - 1).GetComponent<Button>();
            m_BtnToday.Select();
        }
        /// <summary>
        /// 改变年份
        /// </summary>
        public void ChangeYear(int years)
        {
            m_CurrentYear += years;
            ResetCalendarPage();
            if (m_CurrentMonth == DateTime.Now.Month & m_CurrentYear == DateTime.Now.Year)
            {
                HighlightToday();
            }
            RefreshCalendarPage();
        }
        /// <summary>
        /// 改变月份
        /// </summary>
        public void ChangeMonth(int months)
        {
            m_CurrentMonth += months;
            if (m_CurrentMonth > 12)
            {
                m_CurrentMonth = 1;
                m_CurrentYear++;
            }
            if (m_CurrentMonth < 1)
            {
                m_CurrentMonth = 12;
                m_CurrentYear--;
            }
            ResetCalendarPage();
            if (m_CurrentMonth == DateTime.Now.Month & m_CurrentYear == DateTime.Now.Year)
            {
                HighlightToday();
            }
            RefreshCalendarPage();
        }
        /// <summary>
        /// 刷新日历页
        /// </summary>
        public void RefreshCalendarPage()
        {
            //隐藏多余天
            int dayCount = DateTime.DaysInMonth(m_CurrentYear, m_CurrentMonth);
            int extraDayCount = 31 - dayCount;
            while (extraDayCount > 0)
            {
                m_PanDay.GetChild(31 - extraDayCount).gameObject.SetActive(false);
                extraDayCount--;
            }

            //月初
            DateTime newDateTime = new DateTime(m_CurrentYear, m_CurrentMonth, 1);
            int startWeek = (int)newDateTime.DayOfWeek;
            //添加空物体以对齐周
            for (int i = 0; i < startWeek; i++)
            {
                GameObject empty = new GameObject();
                empty.name = "Empty";
                empty.AddComponent<RectTransform>();
                empty.transform.SetParent(m_PanDay);
                empty.transform.SetAsFirstSibling();
            }

            m_Year.text = m_CurrentYear + "年";
            m_Month.text = m_CurrentMonth + "月";
        }
        /// <summary>
        /// 重置日历页
        /// </summary>
        public void ResetCalendarPage()
        {
            //删除空物体
            List<GameObject> emptys = new List<GameObject>();
            for (int i = 0; i < 7; i++)
            {
                if (m_PanDay.GetChild(i).name == "Empty")
                {
                    emptys.Add(m_PanDay.GetChild(i).gameObject);
                }
                else
                {
                    break;
                }
            }
            for (int i = 0; i < emptys.Count; i++)
            {
                DestroyImmediate(emptys[i]);
            }
            //显示所有天
            for (int i = 0; i < m_PanDay.childCount; i++)
            {
                m_PanDay.GetChild(i).gameObject.SetActive(true);
            }
        }
        /// <summary>
        /// 重置日历页为今天
        /// </summary>
        public void ResetCalendarPageToToday()
        {
            ResetCalendarPage();
            GetCurrentDate();
            HighlightToday();
            RefreshCalendarPage();
        }
        public override void OpenNewPage(GameObject page)
        {
            base.OpenNewPage(page);
            if (page == null)
            {
                return;
            }
            if (page.name == "Page_Calendar")
            {
                ResetCalendarPageToToday();
            }
        }

        //时钟相关
        public void RefreshClockPage()
        {
            m_Time.text = DateTime.Now.ToString("HH:mm:ss");
        }

        //闹钟相关
        /// <summary>
        /// 新建闹钟
        /// </summary>
        public void AddAlarmClockItem()
        {
            GameObject alarmClockItem = Instantiate(m_AlarmClockItemTemp);
            alarmClockItem.transform.SetParent(m_AlarmClockList);
            alarmClockItem.GetComponent<Button>().onClick.AddListener
            (delegate ()
            {
            //载入并打开
            m_CurrentAlarmClock = alarmClockItem;
                LoadAlarmClock(alarmClockItem);
                OpenNewPage(m_AlarmClockEditPage);
            });
            alarmClockItem.transform.Find("Btn_Switch").GetComponent<Button>().onClick.AddListener
            (delegate ()
            {
                SetActiveOfAlarmClock();
            });

            //载入并打开
            LoadAlarmClock(alarmClockItem);
            OpenNewPage(m_AlarmClockEditPage);
        }
        /// <summary>
        /// 载入闹钟
        /// </summary>
        public void LoadAlarmClock(GameObject alarmClockItem)
        {
            m_CurrentAlarmClock = alarmClockItem;
        }
        /// <summary>
        /// 储存闹钟
        /// </summary>
        public void SaveAlarmClock()
        {
        }
        /// <summary>
        /// 移除闹钟
        /// </summary>
        public void RemoveCurrentAlarmClock()
        {
            Destroy(m_CurrentAlarmClock);
            m_PhoneManager.ShowMessage("已移除一个闹钟");

            BackLastPage();
        }

        public void SetActiveOfAlarmClock()
        {

        }

        //秒表相关
        /// <summary>
        /// 开始计时
        /// </summary>
        public void StartTiming()
        {
            m_IsTiming = true;
            m_Start.interactable = false;
            m_Pause.interactable = true;
            m_Check.interactable = true;
            m_Clear.interactable = false;
            if (m_TimingTime.text == "00:00:00.000")
            {
                m_StartTime = DateTime.Now;
            }
        }
        /// <summary>
        /// 计时
        /// </summary>
        public void Timing()
        {
            TimeSpan ts = DateTime.Now - m_StartTime;
            int h = ts.Hours;
            int m = ts.Minutes;
            int s = ts.Seconds;
            int ms = ts.Milliseconds;
            string H;
            string M;
            string S;
            string MS;
            if (h < 10)
            {
                H = "0" + h;
            }
            else
            {
                H = h.ToString();
            }
            if (m < 10)
            {
                M = "0" + m;
            }
            else
            {
                M = m.ToString();
            }
            if (s < 10)
            {
                S = "0" + s;
            }
            else
            {
                S = s.ToString();
            }
            if (ms < 10)
            {
                MS = "00" + ms.ToString();
            }
            else if (ms < 100)
            {
                MS = "0" + ms.ToString();
            }
            else
            {
                MS = ms.ToString();
            }
            m_TimingTime.text = H + ":" + M + ":" + S + "." + MS;
        }
        /// <summary>
        /// 中止计时
        /// </summary>
        public void PauseTiming()
        {
            m_IsTiming = false;
            m_Start.interactable = true;
            m_Pause.interactable = false;
            m_Check.interactable = false;
            m_Clear.interactable = true;
        }
        /// <summary>
        /// 记录
        /// </summary>
        public void CheckTimeSpan()
        {
            GameObject timeSpanItem = Instantiate(m_TimeSpanItemTemp);
            timeSpanItem.transform.SetParent(m_TimeSpanList);
            timeSpanItem.transform.Find("Txt_ID").GetComponent<Text>().text = (timeSpanItem.transform.GetSiblingIndex() + 1).ToString();
            timeSpanItem.transform.Find("Txt_Value").GetComponent<Text>().text = m_TimingTime.text;
            StartCoroutine(SetScrollbar());
        }
        IEnumerator SetScrollbar()
        {
            yield return new WaitForSeconds(0.1f);
            m_TimeSpanListScrollbar.value = 0;
        }
        /// <summary>
        /// 清除数据
        /// </summary>
        public void ClearTimeSpan()
        {
            m_TimingTime.text = "00:00:00.000";
            for (int i = 0; i < m_TimeSpanList.childCount; i++)
            {
                Destroy(m_TimeSpanList.GetChild(i).gameObject);
            }
        }
    }
}