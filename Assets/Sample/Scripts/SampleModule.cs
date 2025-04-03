using System;
using Spookline.SPC.Events;
using Spookline.SPC.Ext;
using UnityEngine;

[CreateAssetMenu(fileName = "SampleModule", menuName = "Modules/Sample Module")]
public class SampleModule : Module {

    [ConfigValue]
    public int configInt = 48;

    public override void Load() {
        base.Load();
        Debug.Log($"SampleModule loaded with configInt: {configInt}");
    }

    public override void Unload() {
        Debug.Log($"SampleModule unloaded");
        base.Unload();
    }
    
    [ContextMenu("Raise Event")]
    public void RaiseEvent() {
        var myEvent = new MyCustomEvent { MyValue = 42 };
        myEvent.Raise();
    }

}

public class MyCustomEvent : EventBase {

    public int MyValue { get; set; }

}