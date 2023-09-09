using System;
using System.Reflection;
using System.Xml.Linq;
using UnityEngine;

namespace UnityTool.FileKit
{
    public static class XMLToIns<T> where T : class
    {
        private static string _mPath;
        private static T _mTarget;

        /// <summary>
        /// 将指定路径的xml转化成特定类实例
        /// </summary>
        /// <param name="xmlPath"></param>
        /// <returns></returns>
        public static T ToIns(string xmlPath)
        {
            if (xmlPath == null) return null;

            SetPath(xmlPath);
            XElement xml = LoadXml(xmlPath);
            CreateInitiate();

            Type t = _mTarget.GetType();
            FieldInfo[] fields = t.GetFields();
            string fieldName = String.Empty;

            foreach (FieldInfo f in fields)
            {
                fieldName = f.Name;
                if (xml.Element(fieldName) != null)
                    f.SetValue(_mTarget, Convert.ChangeType(xml.Element(fieldName).Value, f.FieldType));
            }

            return _mTarget;
        }
        
        /// <summary>
        /// 设置路径
        /// </summary>
        /// <param name="path"></param>
        private static void SetPath(string path)
        {
            if (_mPath != null)
                _mPath = path;
        }

        /// <summary>
        /// 获取一个默认实例
        /// </summary>
        private static void CreateInitiate()
        {
            if (_mTarget != null)
                _mTarget = null;

            Type t = typeof(T);
            ConstructorInfo ct = t.GetConstructor(Type.EmptyTypes);
            _mTarget = (T)ct?.Invoke(null);
        }

        /// <summary>
        /// 加载xml文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static XElement LoadXml(string path)
        {
            if (path == null) return null;
            
            XElement xml = XElement.Load(path);

            return xml;
        }
    }
}