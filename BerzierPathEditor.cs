using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(BerzierPath)), CanEditMultipleObjects]
public class BezierPathEditor : Editor
{
    public void OnEnable()
    {
        tempSelf = target as BerzierPath;
    }

    private BerzierPath tempSelf;
    public override void OnInspectorGUI()
    {
        
        base.OnInspectorGUI();

        if (GUILayout.Button("生成"))
        {
            tempSelf.processT = 0f;
            tempSelf.second = 5f;
            tempSelf.deltaS = 1f / 5f;
        }

        if (GUILayout.Button("增加一条曲线"))
        {
            if( tempSelf.lineList.Count > 0 )
            {
                BerzierLine bdd = new BerzierLine();
                bdd.pLocal.pBegin = tempSelf.lineList[tempSelf.lineList.Count-1].pLocal.pEnd;
                bdd.pLocal.pEnd = tempSelf.lineList[tempSelf.lineList.Count-1].pLocal.pEnd;
                bdd.pLocal.pBeginTangent = tempSelf.lineList[tempSelf.lineList.Count - 1].pLocal.pEnd;
                bdd.isShow = true;
                tempSelf.lineList.Add(bdd);
            }
            else
            {
                BerzierLine bdd = new BerzierLine();
                tempSelf.lineList.Add(bdd);
            }
        }
    }

    public Vector3 Do4Position(Vector3 p, out bool isChange)
    {
        EditorGUI.BeginChangeCheck();
        Vector3 worldPos = tempSelf.transform.TransformPoint(p);
        Vector3 newTargetPosition = Handles.PositionHandle(worldPos, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(tempSelf, "Change Look At Target Position");
            newTargetPosition = tempSelf.transform.InverseTransformPoint(newTargetPosition);
            isChange = true;
        }
        else
        {
            isChange = false;
            newTargetPosition = p;
        }
        return newTargetPosition;
    }

    public void OnSceneGUI()
    {
        //Handles.DrawBezier(tempSelf.gizmos.pBegin, tempSelf.gizmos.pEnd, tempSelf.gizmos.pBeginTangent, tempSelf.gizmos.pEndTangent, Color.red, null, 10); ;

        Vector3 tmp1 = Vector3.zero;
        Vector3 tmp2 = Vector3.zero;
        for (int i = 0; i < tempSelf.lineList.Count; i++)
        {
            BerzierLine bl = tempSelf.lineList[i];
            if (bl.isShow)
            {
                bool begin = false;
                bool end = false;
                bool middle = false;
                bl.pLocal.pBegin = Do4Position(bl.pLocal.pBegin, out begin);
                bl.pLocal.pBeginTangent = Do4Position(bl.pLocal.pBeginTangent, out middle);
                bl.pLocal.pEndTangent = Do4Position(bl.pLocal.pEndTangent, out middle);
                bl.pLocal.pEnd = Do4Position(bl.pLocal.pEnd, out end);

                if ( begin && i > 0)
                {
                    tempSelf.lineList[i -1].pLocal.pEnd = bl.pLocal.pBegin;
                }
                else if ( end && i < tempSelf.lineList.Count - 1)
                {
                    tempSelf.lineList[i + 1].pLocal.pBegin = bl.pLocal.pEnd;
                }
            }
        }
    }
}
