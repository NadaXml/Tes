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
    public RectTransform my;
    public RectTransform dc;

    private void Awake()
    {
        d.onValueChanged.AddListener(OnScroll);
    }

    public void OnScroll(Vector2 v2)
    {
        float delta = 932;
        float myY = delta;
        float myHeight = my.rect.height;

        bool top = myY > ctn.anchoredPosition.y - myHeight * 0.5 ;
        bool bottom = myY < ctn.anchoredPosition.y + myHeight * 0.5 + dc.rect.height;

        Debug.Log(top);
        Debug.Log(bottom);

        int i = 3;
        i++;
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
