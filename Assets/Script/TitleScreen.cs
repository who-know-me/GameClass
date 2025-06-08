using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
 
public class TitleScreen : MonoBehaviour
{
    //响应游戏开始事按钮件
    public void OnButtonGameStart1()
    {
        SceneManager.LoadScene("Level 1");  //读取关卡level1
    }
    public void OnButtonGameStart2()
    {
        SceneManager.LoadScene("Level 2");  //读取关卡level1
    }
    public void OnButtonGameStart3()
    {
        SceneManager.LoadScene("Level 3");  //读取关卡level1
    }
    public void OnButtonGameStart4()
    {
        SceneManager.LoadScene("Start");  //读取关卡level1
    }
}
