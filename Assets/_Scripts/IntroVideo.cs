using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroVideo : MonoBehaviour
{
    public void OnVideoEnd()
    {
        GameManager.Instance.Player.EnableMainCanvas();
        GameManager.Instance.EnableChat();
        GameManager.Instance.Player.Resume();
    }
}
