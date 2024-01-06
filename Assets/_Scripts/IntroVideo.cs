using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroVideo : MonoBehaviour
{
    public void OnVideoEnd()
    {
        GameManager.Instance.Player.EnableMainCanvas();
        GameManager.Instance.EnableChat();

        if (GameManager.Instance.IsInMainRoom)
        {
            GameManager.Instance.Player.Resume();
        }
    }
}
