using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace E.Utility
{
    public class ScriptableObjectDictionary<T> : ScriptableObject where T : ScriptableObject
    {
        private static Dictionary<int, T> dictionary;
        public static Dictionary<int, T> Dictionary
        {
            get
            {
                if (dictionary == null)
                {
                    LoadDictionary();
                }
                return dictionary;
            }
        }

        public static Dictionary<int, T> ReGetDictionary()
        {
            LoadDictionary();
            return Dictionary;
        }
        public static List<T> GetDictionaryValues()
        {
            List<T> values = new List<T>();
            values.AddRange(Dictionary.Values);
            return values;
        }
        public static List<T> ReGetDictionaryValues()
        {
            List<T> values = new List<T>();
            values.AddRange(ReGetDictionary().Values);
            return values;
        }

        private static void LoadDictionary()
        {
            T[] items = Resources.LoadAll<T>("");

            // 检查同名对象
            List<string> duplicates = items.ToList().FindDuplicates(item => item.name);
            if (duplicates.Count == 0)
            {
                dictionary = items.ToDictionary(item => item.name.GetStableHashCode(), item => item);
            }
            else
            {
                foreach (string duplicate in duplicates)
                {
                    Debug.LogError("Resources文件夹内包含多个同名的 {" + typeof(T).Name + "} {" + duplicate + "}，如果它们在不同的子文件夹，请将它们的名称前加上 “（子文件夹名）”以区分。");
                }
            }
        }
    }
}
