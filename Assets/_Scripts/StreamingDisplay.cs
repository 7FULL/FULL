using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StreamingDisplay : MonoBehaviour
{
    [SerializeField]
    [InspectorName("Streaming name")]
    private TMP_Text streamingName;

    private Inventory inventory;
    private string streamName;
    
    public void Configure(string streamName, Inventory inventory)
    {
        this.inventory = inventory;
        this.streamName = streamName;
        streamingName.text = streamName;
    }

    public void OpenStreaming()
    {
        inventory.StartSpectatingStreaming(streamName);
        GameManager.Instance.Player.Stop();
    }
}
