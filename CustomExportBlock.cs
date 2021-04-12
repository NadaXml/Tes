using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SLua
{

    public class CustomExportBlockEditor : EditorWindow
    {
        [MenuItem("SLua/Custom/BlockWhite")]
        static public void showWnd()
        {
            CustomExportBlockEditor wnd = EditorWindow.GetWindow<CustomExportBlockEditor>("CustomExportBlockEditor");
            wnd.Show();
        }

        Vector2 scrollPosition = Vector2.zero;

        List<string> paramList = new List<string>();
        string className;
        void OnGUI()
        {
            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("className");
                    className = GUILayout.TextField(className, GUILayout.Width(200));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("查看函数"))
                    {
                        paramList.Clear();

                        Type tt = Type.GetType(className);

                        var r = tt;
                        Debug.Log(r.Name);
                        MethodInfo[] s = r.GetMethods();

                        foreach (var info in s)
                        {
                            paramList.Add("--------------------------" + info.Name);
                            ParameterInfo[] pInfos = info.GetParameters();
                            if (pInfos.Length > 0)
                            {
                                foreach (var pInfo in pInfos)
                                {
                                    paramList.Add(pInfo.ParameterType.Name);
                                }
                            }
                            else
                            {
                                paramList.Add("empty");
                            }
                        }
                    }

                    if (GUILayout.Button("查看构造函数"))
                    {
                        paramList.Clear();

                        Type tt = Type.GetType(className);

                        var r = tt;
                        Debug.Log(r.Name);
                        ConstructorInfo[] s = r.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

                        foreach (var info in s)
                        {
                            paramList.Add("--------------------------");
                            ParameterInfo[] pInfos = info.GetParameters();
                            if (pInfos.Length > 0)
                            {
                                foreach (var pInfo in pInfos)
                                {
                                    paramList.Add(pInfo.ParameterType.Name);
                                }
                            }
                            else
                            {
                                paramList.Add("empty");
                            }
                        }
                    }
                }
                GUILayout.EndHorizontal();

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width), GUILayout.ExpandHeight(true));

                foreach (var str in paramList)
                {
                    EditorGUILayout.SelectableLabel(str);
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
        public class BlockConstructor
        {
            public List<string> param;
        }

        [System.Serializable]
        public class BlockMethod
        {
            public string methodName;
            public List<string> param;
        }

        public string className;
        public List<BlockConstructor> constructor;
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

        static public CustomExportBlockTool getInstance()
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
        public void loadJson()
        {
            if (blockMap != null )
            {
                blockMap.Clear();
            }
            else
            {
                blockMap = new Dictionary<string, CustomExportBlock>();
            }

            string jsonStr = File.ReadAllText(Application.dataPath + "/Slua/Editor/test.json");

            datas = JsonUtility.FromJson<CustomExportBlocks>(jsonStr);

            foreach ( var block in datas.allClass)
            {
                blockMap[block.className] = block;
                if (block.method != null )
                {
                    block.method.Clear();
                }
                else
                {
                    block.method = new List<CustomExportBlock.BlockMethod>();
                }
                foreach( CustomExportBlock.BlockMethod method in block.method )
                {
                    block.methodMap[method.methodName] = method;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="className"></param>
        /// <param name="cInfo"></param>
        /// <returns></returns>
        public bool isValidConstructorInfo(string className, ConstructorInfo cInfo)
        {
            CustomExportBlock block = isValidClass(className);
            return isValidConstructor(block, cInfo);
        }

        public bool isValidMethod(string className, MethodInfo info)
        {
            CustomExportBlock block = isValidClass(className);
            return isValidMethod(block, info);
        }
            
        /// <summary>
        /// 需要检查构造函数的类名
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public CustomExportBlock isValidClass(string className)
        {
            if (blockMap == null )
            {
                return null;
            }
            CustomExportBlock block = null;
            blockMap.TryGetValue(className, out block);
            return block;
        }

        public bool isValidMethod(CustomExportBlock block, MethodInfo info)
        {
            bool ret = true;
            if (block == null || info == null)
            {
                return ret;
            }
            ParameterInfo[] pi = info.GetParameters();
            CustomExportBlock.BlockMethod method;
            if (block.methodMap.TryGetValue(info.Name, out method))
            {
                ret = isSameParameter(method.param, pi);
            }
            return ret;
        }
        
        public bool isSameParameter(List<string> param, ParameterInfo[] info)
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

        /// <summary>
        /// 反射出来的合法构造函数
        /// </summary>
        /// <param name="block"></param>
        /// <param name="cInfo"></param>
        /// <returns></returns>
        public bool isValidConstructor(CustomExportBlock block, ConstructorInfo cInfo)
        {
            bool ret = true;
            if(block == null || cInfo == null )
            {
                return ret;
            }

            ParameterInfo[] pi = cInfo.GetParameters();
            foreach (CustomExportBlock.BlockConstructor construct in block.constructor)
            {
                ret = ret && isSameParameter(construct.param, pi);
                if( !ret )
                {
                    break;
                }
            }
            return ret;
        }
    }
}

