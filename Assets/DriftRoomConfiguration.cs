using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriftRoomConfiguration : RoomConfiguration
{
     [SerializeField]
     [InspectorName("Skidmarks")]
     private Skidmarks skidmarksController;

     public override void HandlePlayer(Player player)
     {
          base.HandlePlayer(player);

          player.GetComponentInParent<WheelController>().UpdateWheels(skidmarksController);
     }
}
