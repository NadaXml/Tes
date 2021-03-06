using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using System.Text;

[System.Serializable]
public class BezierPathLogicData
{
    public Vector3 pBegin = Vector3.zero;
    public Vector3 pBeginTangent = Vector3.zero;
    public Vector3 pEndTangent = Vector3.zero;
    public Vector3 pEnd = Vector3.zero;

    //3次贝塞尔曲线
    public Vector3 getRectV3(float t)
    {
        return (1 - t) * (1 - t) * (1 - t) * pBegin
                + 3 * t * (1 - t) * (1 - t) * pBeginTangent
                + 3 * t * t * (1 - t) * pEndTangent
                + pEnd * t * t * t;
    }

    //3次贝塞尔曲线
    public Vector3 getRectV2(float t)
    {
        return (1 - t) * (1 - t) * pBegin
                + 2 * t * (1 - t) * pBeginTangent
                + pEnd * t * t;
    }

    public float BezierLength(int pointCount = 1000)
    {
        if (pointCount < 2)
        {
            return 0;
        }

        //取点 默认 30个
        float length = 0.0f;
        Vector3 lastPoint = getRectV3(0.0f / (float)pointCount);
        for (int i = 1; i <= pointCount; i++)
        {
            Vector3 point = getRectV3((float)i / (float)pointCount);
            float deltaDis = Vector3.Distance(point, lastPoint);
            length += deltaDis;
            lastPoint = point;
        }
        return length;
    }

    public string exportLuastring()
    {
        //generate(Application.dataPath + "Assets/Berizer/luaTemplate.txt");
        return string.Empty;
    }

    //public void generate(string templatePath, string pathName)
    //{
    //    UTF8Encoding encoding = new UTF8Encoding(true, false);
    //    string templateText = "";
    //    if (File.Exists(templatePath))
    //    {
    //        /// Read procedures.
    //        StreamReader reader = new StreamReader(templatePath);
    //        templateText = reader.ReadToEnd();
    //        reader.Close();

    //        templateText = templateText.Replace("#SCRIPTNAME#", className);
    //        templateText = templateText.Replace("#NOTRIM#", string.Empty);
    //        /// You can replace as many tags you make on your templates, just repeat Replace function
    //        /// e.g.:
    //        /// templateText = templateText.Replace("#NEWTAG#", "MyText");

    //        /// Write procedures.

    //        StreamWriter writer = new StreamWriter(Path.GetFullPath(pathName), false, encoding);
    //        writer.Write(templateText);
    //        writer.Close();

    //        AssetDatabase.ImportAsset(pathName);
    //        return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
    //    }
    //    else
    //    {
    //        Debug.LogError(string.Format("The template file was not found: {0}", templatePath));
    //        return null;
    //    }
    //}

    //public float t2rt(float t)
    //{
    //    // 定义真实时间与差时变量
    //    float realTime;
    //    float deltaTime = 0f;

    //    // 曲线上的 x 坐标
    //    float bezierX;

    //    // 计算 t 对应曲线上匀速的 x 坐标
    //    float x = (pEnd.x - pBegin.x) * t;

    //    realTime = 1f;
    //    do
    //    {
    //        // 半分
    //        if (deltaTime > 0)
    //        {
    //            realTime -= realTime / 2;
    //        }
    //        else
    //        {
    //            realTime += realTime / 2;
    //        }

    //        // 计算本此 "rt" 对应的曲线上的 x 坐标
    //        bezierX = getRectV3(realTime).x;

    //        // 计算差时
    //        deltaTime = bezierX - x;
    //    }
    //    // 差时逼近为0时跳出循环
    //    while (Mathf.Abs(deltaTime) > 0.0000001f);

    //    return realTime;
    //}
}

[System.Serializable]
public class BerzierLine
{
    [HideInInspector]
    public BezierPathLogicData gizmos = new BezierPathLogicData();
    public BezierPathLogicData pLocal = new BezierPathLogicData();

    public bool isShow;

    [HideInInspector]
    public float percent;


    public void calcuatePercent(float ls, float allY)
    {
        float length = pLocal.BezierLength();
        percent = ls + length / allY;
    }
}


public class BerzierPath : MonoBehaviour
{
    public List<BerzierLine> lineList = new List<BerzierLine>();

    [Range(1, 50)]
    public int segment = 10;

    [Range(0, 1)]
    public float processT;

    public float second = 0f;
    public float deltaS = 0;

    public void Update()
    {
        if (second > 0)
        {
            processT = processT + deltaS * Time.deltaTime;
            second -= Time.deltaTime;
            second = Mathf.Max(0, second);
        }
    }

    private void OnDrawGizmosSelected()
    {
        RectTransform rc = GetComponent<RectTransform>();
        float allY = 0;

        for (int i = 0; i < lineList.Count; i++)
        {
            allY += lineList[i].pLocal.BezierLength();
        }

        for (int i = 0; i < lineList.Count; i++)
        {
            if (lineList[i].isShow)
            {
                BezierPathLogicData gizmos = lineList[i].gizmos;
                BezierPathLogicData pLocal = lineList[i].pLocal;

                float lt = 0;
                if (i > 0)
                {
                    lt = lineList[i - 1].percent;
                }
                lineList[i].calcuatePercent(lt, allY);

                gizmos.pBegin = rc.TransformPoint(pLocal.pBegin);
                gizmos.pBeginTangent = rc.TransformPoint(pLocal.pBeginTangent);
                gizmos.pEndTangent = rc.TransformPoint(pLocal.pEndTangent);
                gizmos.pEnd = rc.TransformPoint(pLocal.pEnd);

                Vector3 last = gizmos.pBegin;
                for (int j = 0; j <= segment; j++)
                {
                    Vector3 lb = gizmos.getRectV3(j / (float)segment);
                    //Vector3 lb = gizmos.getRectV2(j / (float)segment);
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(last, lb);
                    last = lb;
                }

                //Vector3 lb2 = gizmos.getRectV3(processT);
                float lastRange = 0;
                if (i > 0)
                {
                    lastRange = lineList[i - 1].percent;
                }
                if (processT == 0 && i == 0)
                {
                    Vector3 lb2 = gizmos.getRectV3(0);
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(lb2, 10);
                }
                else if (processT > lastRange && processT <= lineList[i].percent)
                {
                    float delta = (processT - lastRange) / (lineList[i].percent - lastRange);
                    Vector3 lb2 = gizmos.getRectV3(delta);
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(lb2, 10);
                }

                if (lineList[i].isShow)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(gizmos.pBegin, 10);

                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(gizmos.pBeginTangent, 10);

                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(gizmos.pEndTangent, 10);

                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(gizmos.pEnd, 10);
                }
            }
        }

    }
}
