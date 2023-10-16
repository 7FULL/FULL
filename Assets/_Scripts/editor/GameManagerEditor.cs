using Photon.Pun.Demo.PunBasics;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor: Editor
{
    /*public override void OnInspectorGUI()
    {
        GameManager myScript = (GameManager) target;
        if (GUILayout.Button("Open all doors"))
        {
            myScript.OpenAllDoors();
        }
    }*/
}