//人们应该能够非常轻松地查看并向开发人员报告错误。
// Unity的Developer Console仅适用于开发版本，它只显示错误。 此类提供了一个适用于所有构建的控制台，还显示了开发构建中的日志和警告。
//注意：我们不包括堆栈跟踪，因为如果需要，也可以从日志文件中获取它。
//注意：没有“隐藏”按钮，因为我们希望人们看到这些错误并将其报告给我们。
//注意：通过在Debug / Development模式下构建，可以显示正常的Debug.Log消息。
using UnityEngine;
using System.Collections.Generic;

namespace E.Utility
{
    class LogEntry
    {
        public string message;
        public LogType type;
        public LogEntry(string message, LogType type)
        {
            this.message = message;
            this.type = type;
        }
    }

    public class GUIConsole : MonoBehaviour
    {
        public int height = 25;
        List<LogEntry> log = new List<LogEntry>();
        Vector2 scroll = Vector2.zero;

#if !UNITY_EDITOR
    void Awake()
    {
        Application.logMessageReceived += OnLog;
    }
#endif

        void OnLog(string message, string stackTrace, LogType type)
        {
            // show everything in debug builds and only errors/exceptions in release
            if (Debug.isDebugBuild || type == LogType.Error || type == LogType.Exception)
            {
                log.Add(new LogEntry(message, type));
                scroll.y = 99999f; // autoscroll
            }
        }

        void OnGUI()
        {
            if (log.Count == 0) return;

            scroll = GUILayout.BeginScrollView(scroll, "Box", GUILayout.Width(Screen.width), GUILayout.Height(height));
            foreach (LogEntry entry in log)
            {
                if (entry.type == LogType.Error || entry.type == LogType.Exception)
                    GUI.color = Color.red;
                else if (entry.type == LogType.Warning)
                    GUI.color = Color.yellow;
                GUILayout.Label(entry.message);
                GUI.color = Color.white;
            }
            GUILayout.EndScrollView();
        }
    }
}