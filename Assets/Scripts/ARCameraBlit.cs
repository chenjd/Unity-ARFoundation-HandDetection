using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARCameraBlit : MonoBehaviour
{
    public ARFoundationHandDetectDemo m_ARDemo;
    public Material m_BlitMat;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if(m_ARDemo.textureFromNative == null)
        {
            Graphics.Blit(src, dest);
            return;
        }
        m_BlitMat.SetTexture("_DetectionTex", m_ARDemo.textureFromNative);
        Graphics.Blit(src, dest, m_BlitMat);
    }
}
