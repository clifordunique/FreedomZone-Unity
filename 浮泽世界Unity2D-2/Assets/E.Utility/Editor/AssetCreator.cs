// ========================================================
// 作者：E Star
// 创建时间：2019-01-19 10:46:38
// 当前版本：1.0
// 作用描述：对话配置管理员
// 挂载目标：无
// ========================================================
using UnityEngine;
using System.Collections;
using System;
using System.IO;
using UnityEditor;

namespace E.Utility
{
    public class AssetCreator<T> where T : ScriptableObject
    {
        private static AssetCreator<T> instance;
        public static AssetCreator<T> Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AssetCreator<T>();
                }
                return instance;
            }
        }

        /// <summary>
        /// 将类实例化为配置文件
        /// </summary>
        public static T CreateAsset()
        {
            ScriptableObject so = ScriptableObject.CreateInstance(typeof(T));
            if (so == null)
            {
                Debug.LogError("资源创建失败：无法将对象实例化");
                return null;
            }
            string path = FileController.SaveProject();
            if (path == null)
            {
                return null;
            }
            else
            {
                string[] strs = path.Split(new string[] { "Assets" }, StringSplitOptions.None);
                if (strs[1] == null)
                {
                    return null;
                }
                path = strs[1];
            }
            string filePath = "Assets" + path;
            //Debug.Log(filePath);
            if (File.Exists(filePath))
            {
                Debug.LogError("资源创建失败：同名文件已存在");
                return null;
            }

            //按指定路径生成配置文件
            AssetDatabase.CreateAsset(so, filePath);
            Debug.Log("资源创建成功：" + filePath);
            return (T)so;
        }
        /// <summary>
        /// 将类实例化为配置文件，指定路径和名称
        /// </summary>
        /// <param name="path">相对工程目录Assets文件夹下的路径</param>
        /// <returns></returns>
        public static T CreateAsset(string path, string name)
        {
            ScriptableObject so = ScriptableObject.CreateInstance(typeof(T));
            if (so == null)
            {
                Debug.LogError("资源创建失败：无法将对象实例化");
                return null;
            }
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            int n = 1;
            string filePath = string.Format("{0}/{1}-1.asset", path, name);
            while (File.Exists(filePath))
            {
                n++;
                filePath = string.Format("{0}/{1}-{2}.asset", path, name, n);
            }

            //按指定路径生成配置文件
            AssetDatabase.CreateAsset(so, filePath);
            Debug.Log("资源创建成功：" + filePath);
            return (T)so;
        }

        /// <summary>
        /// 将配置文件转化为对象
        /// </summary>
        public static T GetAsset(string path)
        {
            // 将配置文件转化为对象
            T obj = AssetDatabase.LoadAssetAtPath<T>(path);
            return obj;
        }
    }
}