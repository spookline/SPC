using System;
using Cysharp.Threading.Tasks;
using Sample.Audio;
using Spookline.SPC;
using Spookline.SPC.Audio;
using Spookline.SPC.Events;
using Spookline.SPC.Ext;
using UnityEngine;

[CreateAssetMenu(fileName = "SampleModule", menuName = "Modules/Sample Module")]
public class SampleModule : Module<SampleModule> {

    public override void Load() {
        base.Load();
        Debug.Log($"SampleModule loaded"); 
        SpookAudioRegistry.Instance.Load(typeof(AudioKeys)).Forget();

        On<GlobalStartEvt>().AsyncDo(ChainFirst);
        On<GlobalStartEvt>().AsyncDo(ChainSecond, priority: 10);
    }

    private async UniTask ChainFirst(GlobalStartEvt arg) {
        Debug.Log($"ChainFirst executed with arg: {arg}");
        await UniTask.Delay(1000); // Simulate some async work
        Debug.Log($"ChainFirst completed after delay");
    }

    private UniTask ChainSecond(GlobalStartEvt arg) {
        Debug.Log($"ChainSecond executed with arg: {arg}");
        return UniTask.CompletedTask; // No async work here, just a placeholder
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