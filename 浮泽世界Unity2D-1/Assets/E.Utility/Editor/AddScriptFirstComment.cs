// ========================================================
// 作者：E Star 
// 创建时间：2018-12-21 13:33:17 
// 当前版本：1.0 
// 作用描述：创建脚本时自动添加脚本文件信息
// 挂载目标：无
// ========================================================
using System.IO;

namespace E.Utility
{
    public class AddScriptFirstComment : UnityEditor.AssetModificationProcessor
    {
        /// <summary>
        /// 在资源创建时调用
        /// </summary>
        /// <param name="path">自动传入资源路径</param>
        public static void OnWillCreateAsset(string path)
        {
            path = path.Replace(".meta", "");
            if (!path.EndsWith(".cs")) return;
            string allText = "// ========================================================\r\n"
                             + "// 作者：E Star\r\n"
                             + "// 创建时间：#CreateTime#\r\n"
                             + "// 当前版本：1.0\r\n"
                             + "// 作用描述：\r\n"
                             + "// 挂载目标：\r\n"
                             + "// ========================================================\r\n";
            allText += File.ReadAllText(path);
            allText = allText.Replace("#CreateTime#", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            File.WriteAllText(path, allText);
        }
    }
}