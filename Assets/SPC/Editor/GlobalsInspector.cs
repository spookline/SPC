using UnityEditor;
using UnityEngine;

namespace Spookline.SPC.Editor {
    [CustomEditor(typeof(Globals))]
    public class GlobalsInspector : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
   
        }
    }
}