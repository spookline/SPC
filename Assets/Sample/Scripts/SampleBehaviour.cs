using Spookline.SPC.Events;
using Spookline.SPC.Ext;
using UnityEngine;

public class SampleBehaviour : SpookBehaviour {

    public ConfigValue<int> configInt = ConfigValue<int>.Ref<SampleModule>(nameof(SampleModule.configInt));

    protected override void Awake() {
        base.Awake();
        Debug.Log($"SampleBehaviour awake with configInt: {configInt}");
    }

    [ContextMenu("Check Current")]
    public void CheckCurrent() {
        Debug.Log($"SampleBehaviour checking current configInt: {configInt}");
    }

    [EventHandler]
    public void OnCustomEvent(MyCustomEvent evt) {
        Debug.Log($"Received custom event with value: {evt.MyValue}");
    }

}