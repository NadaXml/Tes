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
        [MenuItem("SLua/Unity/BlockWhite")]
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

            GUILayout.BeginHorizontal();
            GUILayout.Label("className");
            className = GUILayout.TextField(className, GUILayout.Width(200));

            if (GUILayout.Button("读取"))
            {
                CustomExportBlockTool d = new CustomExportBlockTool();
                d.loadJson();
                Type tt = Type.GetType(className);
                CustomExportBlock b = d.isValidClass(tt.Name);
                if ( b != null )
                {
                    ConstructorInfo[] cInfos = tt.GetConstructors();
                    foreach ( var cc in cInfos)
                    {
                        bool isValid = d.isValidConstructor(b, cc);
                        Debug.Log(" dd  " + isValid);
                    }
                }
            }
            if ( GUILayout.Button("查看") )
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
                            Debug.Log(pInfo.ParameterType.Name);
                            paramList.Add(pInfo.ParameterType.Name);
                        }
                    }
                    else
                    {
                        paramList.Add("empty");
                    }
                }
            }

            GUILayout.EndHorizontal();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width), GUILayout.ExpandHeight(true));
            
            foreach( var str in paramList)
            {
                EditorGUILayout.SelectableLabel(str);
            }
            EditorGUILayout.EndScrollView();


            GUILayout.EndVertical();
        }
    }

    [System.Serializable]
    public class CustomExportBlock
    {
        [System.Serializable]
        public class BlockConstructor
        {
            public List<string> param;
        }

        public string className;
        public List<BlockConstructor> constructor;
    }

    [System.Serializable]
    public class CustomExportBlocks
    {
        public List<CustomExportBlock> allClass;
    }


    public class CustomExportBlockTool
    {
        CustomExportBlocks datas;

        Dictionary<string, CustomExportBlock> blockMap = new Dictionary<string, CustomExportBlock>();
        public void loadJson()
        {
            string jsonStr = File.ReadAllText(Application.dataPath + "/test.json");

            datas = JsonUtility.FromJson<CustomExportBlocks>(jsonStr);

            foreach ( var block in datas.allClass)
            {
                blockMap[block.className] = block;
            }

        }

        public CustomExportBlock isValidClass(string className)
        {
            CustomExportBlock block = null;
            blockMap.TryGetValue(className, out block);
            return block;
        }

        public bool isValidConstructor(CustomExportBlock block, ConstructorInfo cInfo)
        {
            bool ret = true;
            
            ParameterInfo[] pi = cInfo.GetParameters();
            int paramSuit = 0;
            foreach (var construct in block.constructor)
            {
                if (pi.Length == construct.param.Count)
                {

                    for (int i = 0; i < pi.Length; i++)
                    {
                        if (pi[i].ParameterType.Name.Equals(construct.param[i]))
                        {
                            paramSuit++;
                            break;
                        }
                    }
                }
                else
                {
                    paramSuit = -1;
                }
            }
            if (paramSuit == pi.Length)
            {
                ret = false;
            }
                
            return ret;
        }
    }
}

