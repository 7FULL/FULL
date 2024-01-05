using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [Tooltip("The room where the player will be teleported")]
    [InspectorName("Room")]
    [SerializeField]
    private Rooms _room;

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.GetComponentInParent<Entity>() != null)
        {
            if (collision.gameObject.GetComponentInParent<PhotonView>().IsMine)
            {
                if (_room != Rooms.LOBBY)
                {
                    GameManager.Instance.JoinRoom(_room);
                }
                else
                {
                    GameManager.Instance.Player.GoToLobby();
                }
            }
        }
    }
}
