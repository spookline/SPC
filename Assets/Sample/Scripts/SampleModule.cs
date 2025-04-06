using System;
using Spookline.SPC.Events;
using Spookline.SPC.Ext;
using UnityEngine;

[CreateAssetMenu(fileName = "SampleModule", menuName = "Modules/Sample Module")]
public class SampleModule : Module<SampleModule> {

    public override void Load() {
        base.Load();
        Debug.Log($"SampleModule loaded");
    }

    public override void Unload() {
        Debug.Log($"SampleModule unloaded");
        base.Unload();
    }

    private void OnEnable() {
        var counter = 0;
        On<MyCustomEvent>().Stream(x => {
            var i = counter++;
            Debug.Log($"$$> Stream event {i} received with value: {x.MyValue}");
            return i > 1;
        });
        On<MyCustomEvent>().DoOnce(x => {
            Debug.Log($"$$> Single subscription event received with value: {x.MyValue}");
        });
        
    }

    [ContextMenu("Raise Event")]
    public void RaiseEvent() {
        new MyCustomEvent { MyValue = 42 }.Raise();
    }

}

public class MyCustomEvent : Evt<MyCustomEvent> {

    public int MyValue { get; set; }

}