using Spookline.SPC.Events;
using Spookline.SPC.Ext;
using UnityEngine;

public class SampleBehaviour : SpookBehaviour {

    public void Awake() {
        On<MyCustomEvent>().Do(OnCustomEvent);
        On<MyCustomEvent>().Do(OnCustomEvent2);
        On<MyCustomEvent>().Do(OnCustomEventHigh, -5);
        On<MyCustomEvent>().Do(OnCustomEventLow, 5);
    }

    public void OnCustomEvent(MyCustomEvent evt) {
        Debug.Log($"Received custom event with value: {evt.MyValue}");
    }

    public void OnCustomEvent2(MyCustomEvent evt) {
        Debug.Log($"Received custom event 2 with value: {evt.MyValue}");
    }

    public void OnCustomEventHigh(MyCustomEvent evt) {
        Debug.Log($"Received custom event with high priority: {evt.MyValue}");
    }

    public void OnCustomEventLow(MyCustomEvent evt) {
        Debug.Log($"Received custom event with low priority: {evt.MyValue}");
    }
}