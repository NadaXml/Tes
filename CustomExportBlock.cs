using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SLua
{
    /// <summary>
    /// 根据类型反射查询构造函数的编辑器
    /// </summary>
    public class CustomExportBlockEditor : EditorWindow
    {
        [MenuItem("SLua/Custom/Custom导出白名单过滤")]
        static public void ShowWnd()
        {
            CustomExportBlockEditor wnd = EditorWindow.GetWindow<CustomExportBlockEditor>("CustomExportBlockEditor");
            wnd.Show();
        }

        private void OnEnable()
        {
            CustomExportBlockTool.GetInstance().LoadJson();

        }

        Vector2 scrollPosition = Vector2.zero;
        Vector2 blockPosition = Vector2.zero;

        class ShowInfo
        {
            public Type classType;
            public ConstructorInfo constructorInfo;
            public MethodInfo methodInfo;

            public void OnGUI()
            {
                GUILayout.BeginHorizontal();
                if(constructorInfo != null )
                {
                    renderMethod(constructorInfo);
                }
                else if (methodInfo != null )
                {
                    renderMethod(methodInfo);
                }
                GUILayout.EndHorizontal();
            }

            private void renderMethod(MethodBase methodBase)
            {
                ParameterInfo[] infos = methodBase.GetParameters();
                if (infos.Length > 0)
                {
                    GUILayout.BeginHorizontal();
                    foreach (var pInfo in infos)
                    {
                        EditorGUILayout.SelectableLabel(pInfo.ParameterType.Name);
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.SelectableLabel("无参数");
                }

                bool isBlock = CustomExportBlockTool.GetInstance().IsValidMethodInfo(classType.FullName, methodBase);
                string desc = "屏蔽导出";
                if (isBlock )
                {
                    desc = "取消屏蔽";
                }
                if (GUILayout.Button(desc))
                {
                    if ( isBlock )
                    {
                        CustomExportBlockTool.GetInstance().AddBlockClass(classType, methodBase);
                    }
                    else
                    {
                        CustomExportBlockTool.GetInstance().RemoveBlockClass(classType, methodBase);
                    }
                }
            }
        }


        List<ShowInfo> paramList = new List<ShowInfo>();

        private void FillConstructorData(string classWholeName)
        {
            paramList.Clear();
            Type tt = Type.GetType(classWholeName);

            var r = tt;
            ConstructorInfo[] constructInfoList = r.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

            foreach (var constructorInfo in constructInfoList)
            {
                ShowInfo showInfo = new ShowInfo();
                showInfo.constructorInfo = constructorInfo;
                paramList.Add(showInfo);
            }
        }

        private void FillMethodData(string classWholeName)
        {
            paramList.Clear();
            Type tt = Type.GetType(classWholeName);

            var r = tt;
            MethodInfo[] methodInfoList = r.GetMethods();

            foreach (var methodInfo in methodInfoList)
            {
                ShowInfo showInfo = new ShowInfo();
                showInfo.methodInfo = methodInfo;
                paramList.Add(showInfo);
            }
        }

        string m_uiValueClassName;
        void OnGUI()
        {
            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("请输入类名：",GUILayout.Width(80));
                    m_uiValueClassName = GUILayout.TextField(m_uiValueClassName, GUILayout.Width(350));
                }
                GUILayout.EndHorizontal();

                blockPosition = EditorGUILayout.BeginScrollView(blockPosition, GUILayout.Width(Screen.width), GUILayout.ExpandWidth(true));

                Dictionary<string, CustomExportBlock> blockMap = CustomExportBlockTool.GetInstance().BlockMap;
                foreach( var pair in blockMap)
                {
                    if (GUILayout.Button(pair.Value.className))
                    {
                        m_uiValueClassName = pair.Value.className;
                    }
                }

                EditorGUILayout.EndScrollView();

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("查看构造函数"))
                    {
                        FillConstructorData(m_uiValueClassName);
                    }

                    if (GUILayout.Button("查看成员方法"))
                    {
                        FillMethodData(m_uiValueClassName);
                    }
                }
                GUILayout.EndHorizontal();

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width), GUILayout.ExpandHeight(true));

                foreach (var info in paramList)
                {
                    info.OnGUI();
                }
                EditorGUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
        }
    }

    /// <summary>
    /// 白名单json对应的类
    /// </summary>
    [System.Serializable]
    public class CustomExportBlock
    {
        [System.Serializable]
        public class MethodParams
        {
            public List<string> param = new List<string>();
        }

        [System.Serializable]
        public class BlockMethod
        {
            public string methodName;
            public List<MethodParams> methodParams;
        }

        public string className;
        public List<BlockMethod> method;
        public Dictionary<string, BlockMethod> methodMap;
    }

    [System.Serializable]
    public class CustomExportBlocks
    {
        public List<CustomExportBlock> allClass;
    }


    public class CustomExportBlockTool
    {
        static private CustomExportBlockTool instance;

        static public CustomExportBlockTool GetInstance()
        {
            if (instance == null)
            {
                instance = new CustomExportBlockTool();
            }
            return instance;
        }
        /// <summary>
        /// Json读出的白名单数据
        /// </summary>
        CustomExportBlocks datas;

        /// <summary>
        /// 类名白名单Map
        /// </summary>
        Dictionary<string, CustomExportBlock> blockMap;
        public Dictionary<string, CustomExportBlock>  BlockMap
        {
            get
            {
                return blockMap;
            }
        }
        public void LoadJson()
        {
            if (blockMap != null )
            {
                blockMap.Clear();
            }
            else
            {
                blockMap = new Dictionary<string, CustomExportBlock>();
            }

            string jsonStr = File.ReadAllText(Application.dataPath + "/Slua/Editor/CustomExportBlockWhiteList.json");

            datas = JsonUtility.FromJson<CustomExportBlocks>(jsonStr);

            foreach ( var block in datas.allClass)
            {
                blockMap[block.className] = block;
                if (block.methodMap != null)
                {
                    block.methodMap.Clear();
                }
                else
                {
                    block.methodMap = new Dictionary<string, CustomExportBlock.BlockMethod>();
                }
                foreach (CustomExportBlock.BlockMethod method in block.method)
                {
                    block.methodMap[method.methodName] = method;
                }
            }
        }

        public void AddBlockClass(Type type, MethodBase info)
        {
            CustomExportBlock block;
            if (!blockMap.TryGetValue(type.FullName, out block))
            {
                block = new CustomExportBlock();
                block.className = type.FullName;
                blockMap[type.FullName] = block;
            }
            CustomExportBlock.BlockMethod method;
            if (!block.methodMap.TryGetValue(info.Name, out method))
            {
                method = new CustomExportBlock.BlockMethod();
                method.methodName = info.Name;
                block.methodMap[info.Name] = method;
            }
            CustomExportBlock.MethodParams methodParams = new CustomExportBlock.MethodParams();

            foreach (ParameterInfo param in info.GetParameters())
            {
                methodParams.param.Add(param.ParameterType.Name);
            }
        }

        private void RemoveBlockClass(Type type, MethodBase info)
        {
            CustomExportBlock block;
            if (!blockMap.TryGetValue(type.FullName, out block))
            {
                return;
            }
            CustomExportBlock.BlockMethod method;
            if (!block.methodMap.TryGetValue(info.Name, out method))
            {
                return;
            }
            
            foreach (CustomExportBlock.MethodParams param in method.methodParams)
            {
                if ( IsSameParameter(param.param, info.GetParameters()) )
                {
                    method.methodParams.Remove(param);
                    break;
                }
            }
        }
        /// <summary>
        /// 过滤成员函数
        /// </summary>
        /// <param name="className">类名</param>
        /// <param name="cInfo">反射得到的构造函数信息</param>
        /// <returns></returns>
        public bool IsValidConstructorInfo(string className, ConstructorInfo cInfo)
        {
            CustomExportBlock block = IsValidClass(className);
            return IsValidMethod(block, cInfo);
        }

        /// <summary>
        /// 过滤员方法
        /// </summary>
        /// <param name="className"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool IsValidMethodInfo(string className, MethodBase info)
        {
            CustomExportBlock block = IsValidClass(className);
            return IsValidMethod(block, info);
        }

        /// <summary>
        /// 需要检查的类名
        /// </summary>
        /// <param name="className">类名</param>
        /// <returns></returns>
        public CustomExportBlock IsValidClass(string className)
        {
            if (blockMap == null )
            {
                return null;
            }
            CustomExportBlock block;
            blockMap.TryGetValue(className, out block);
            return block;
        }

        public bool IsValidMethod(CustomExportBlock block, MethodBase methodBase)
        {
            bool ret = true;
            if (block == null || methodBase == null)
            {
                return ret;
            }
            ParameterInfo[] pi = methodBase.GetParameters();
            CustomExportBlock.BlockMethod method;
            if (block.methodMap.TryGetValue(methodBase.Name, out method))
            {
                ret = isSameMethodParams(method.methodParams, pi);
            }
            return ret;
        }

        public bool isSameMethodParams(List<CustomExportBlock.MethodParams> sourceMathodParams, ParameterInfo[] pi)
        {
            bool ret = true;
            foreach (CustomExportBlock.MethodParams param in sourceMathodParams)
            {
                ret = ret && IsSameParameter(param.param, pi);
                if (!ret)
                {
                    break;
                }
            }
            return ret;
        }

        /// <summary>
        /// true不同方法参数，false相同方法参数
        /// </summary>
        /// <param name="param"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool IsSameParameter(List<string> param, ParameterInfo[] info)
        {
            bool ret = true;

            int fitCount = 0;
            if (info.Length == param.Count)
            {
                for (int i = 0; i < info.Length; i++)
                {
                    if (info[i].ParameterType.Name.Equals(param[i]))
                    {
                        fitCount++;
                    }
                }
            }
            else
            {
                fitCount = -1;
            }

            if (fitCount == info.Length)
            {
                ret = false;
            }

            return ret;
        }
    }
}

