using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


public class ssc : MonoBehaviour
{

    [MenuItem("EamTools/T3", false, 0)]
    static void T3()
    {
        GameObject[] ss = Selection.gameObjects;
        if ( ss.Length != 2 )
            {
            return;
        }

        Vector3 d = ss[0].transform.TransformPoint(Vector3.zero);
        d = ss[1].transform.InverseTransformPoint(d);

        print(d);
    }

    public ScrollRect d;
    public RectTransform ctn;
    public RectTransform myRoot;
    public RectTransform singleCtnRoot;

    public RectTransform viewPort;

    public RectTransform myShow;

    private void Awake()
    {
        d.onValueChanged.AddListener(OnScroll);
    }

    public void OnScroll(Vector2 v2)
    {
        //按照Content的坐标系下进行换算
        //内部元素都按照上对其，减少局部坐标到世界坐标的转化
        //Pivot(*,1)
        //单个页签先对
        float pointOffset = 0;
        float pointHeight = myRoot.rect.height;
        float mapAnchorY = -singleCtnRoot.anchoredPosition.y;

        Transform anchor = myRoot.transform.Find("anchor");
        Vector3 oo = anchor.TransformPoint (anchor.localPosition);
        oo = singleCtnRoot.InverseTransformPoint(oo);

        pointOffset = -oo.y;
        
        float viewportAnchorY = ctn.anchoredPosition.y;
        float myCtnAnchoredY = pointOffset + mapAnchorY;

        Debug.Log("--------------------begin");
        Debug.Log("oo " + oo.y);
        Debug.Log(" pointoffset " + pointOffset + " mapAnchorY " + mapAnchorY + " pointHeight  " + pointHeight + "  viewportAnchorY  " + viewportAnchorY);
        Debug.Log("--------------------end");

        //这个界面的锚点是0.5左右要加
        float myCtnTop = myCtnAnchoredY;
        float myCtnBottom = myCtnAnchoredY + pointHeight;

        bool top = myCtnBottom > viewportAnchorY;
        bool bottom = myCtnTop < viewportAnchorY + viewPort.rect.height;

        myShow.gameObject.SetActive(top && bottom);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
