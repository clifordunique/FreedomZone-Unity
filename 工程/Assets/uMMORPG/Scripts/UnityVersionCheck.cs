using UnityEngine;

public class UnityVersionCheck : MonoBehaviour
{
    public string recommended = "2017.4.4f1";
    public string download = "https://unity3d.com/unity/qa/lts-releases";

    void Awake()
    {
        if (Application.unityVersion != recommended)
            Debug.LogWarning("uMMORPG 最优工作版本 Unity " + recommended + " LTS! 下载地址: " + download + "\n");
    }
}
