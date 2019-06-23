// ========================================================
// 作者：E Star
// 创建时间：2019-03-01 01:52:49
// 当前版本：1.0
// 作用描述：
// 挂载目标：
// ========================================================
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E.Utility
{
    public class ReadOnlyAttribute : PropertyAttribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public class RenameAttribute : PropertyAttribute
    {
        /// <summary>
        /// 用来显示变量名的自定义字符串
        /// </summary>
        public string Name;

        public RenameAttribute(string name)
        {
            Name = name;
        }
    }
}