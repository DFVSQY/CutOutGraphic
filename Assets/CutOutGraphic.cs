using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CutOutGraphic : Graphic, ICanvasRaycastFilter
{
    protected CutOutGraphic() { useLegacyMeshGeneration = false; }

    private RectTransform _mask;
    private RectTransform mask
    {
        get
        {
            if(_mask == null)
            {
                _mask = transform.Find("Mask") as RectTransform;
            }
            return _mask;
        }
    }

    private Vector3 lastPos = Vector3.zero;
    private Vector3 lastSize = Vector3.one;

    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    private void Init()
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.localScale = Vector3.one;
        rectTransform.localPosition = Vector3.zero;

        lastPos = mask.localPosition;
        lastSize = mask.sizeDelta;
    }

    /////////////////////////////////////////
    //         //            //            //
    //         //            //            //
    //         //      2     //            //
    //         //            //            //
    //         ////////////////            //
    //         //            //            //
    //     1   //   镂空区   //      3     //
    //         //            //            //
    //         ////////////////            //
    //         //            //            //
    //         //      4     //            //
    //         //            //            //
    //         //            //            //
    /////////////////////////////////////////
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        base.OnPopulateMesh(vh);

        List<UIVertex> verts = new List<UIVertex>();
        vh.GetUIVertexStream(verts);

        float xMin = mask.localPosition.x + mask.rect.xMin;
        float xMax = mask.localPosition.x + mask.rect.xMax;
        float yMin = mask.localPosition.y + mask.rect.yMin;
        float yMax = mask.localPosition.y + mask.rect.yMax;

        float uMin = (xMin - rectTransform.rect.xMin) / rectTransform.rect.width;
        float uMax = (xMax - rectTransform.rect.xMin) / rectTransform.rect.width;
        float vMin = (yMin - rectTransform.rect.yMin) / rectTransform.rect.height;
        float vMax = (yMax - rectTransform.rect.yMin) / rectTransform.rect.height;

        Vector3 pos01 = verts[0].position;
        Vector3 pos02 = verts[1].position;
        Vector3 pos03 = new Vector3(xMin,verts[1].position.y,0f);
        Vector3 pos04 = new Vector3(xMax,verts[1].position.y,0f);
        Vector3 pos05 = verts[2].position;
        Vector3 pos06 = verts[4].position;
        Vector3 pos07 = new Vector3(xMax,verts[4].position.y,0f);
        Vector3 pos08 = new Vector3(xMin,verts[4].position.y,0f);
        Vector3 pos09 = new Vector3(xMin,yMin,0f);
        Vector3 pos10 = new Vector3(xMin,yMax,0f);
        Vector3 pos11 = new Vector3(xMax,yMax,0f);
        Vector3 pos12 = new Vector3(xMax,yMin,0f);

        vh.Clear();

        // 添加顶点列表
        vh.AddVert(pos01,color,new Vector2(0f,0f));
        vh.AddVert(pos02,color,new Vector2(0f,1f));
        vh.AddVert(pos03,color,new Vector2(uMin,1f));
        vh.AddVert(pos04,color,new Vector2(uMin,1f));
        vh.AddVert(pos05,color,new Vector2(1f,1f));
        vh.AddVert(pos06,color,new Vector2(1f,0f));
        vh.AddVert(pos07,color,new Vector2(uMax,0f));
        vh.AddVert(pos08,color,new Vector2(uMin,0f));

        vh.AddVert(pos09,color,new Vector2(uMin,vMin));
        vh.AddVert(pos10,color,new Vector2(uMin,vMax));
        vh.AddVert(pos11,color,new Vector2(uMax,vMax));
        vh.AddVert(pos12,color,new Vector2(uMax,vMax));

        // 第一个区域
        vh.AddTriangle(0,1,2);
        vh.AddTriangle(2,7,0);

        // 第二个区域
        vh.AddTriangle(9,2,3);
        vh.AddTriangle(3,10,9);

        // 第三个区域
        vh.AddTriangle(6,3,4);
        vh.AddTriangle(4,5,6);

        // 第四个区域
        vh.AddTriangle(7,8,11);
        vh.AddTriangle(11,6,7);
    }

    private void Update()
    {
        if(!mask.localPosition.Equals(lastPos))
        {
            lastPos = mask.localPosition;
            SetAllDirty();
        }
        else if(!mask.sizeDelta.Equals(lastSize))
        {
            lastSize = mask.sizeDelta;
            SetAllDirty();
        }
    }

    public bool IsRaycastLocationValid(Vector2 sp,Camera eventCamera)
    {
        bool isValid = RectTransformUtility.RectangleContainsScreenPoint(mask,sp,eventCamera);
        return !isValid;
    }
}
