using Spookline.SPC.Ext;
using UnityEditor;
using UnityEngine;

namespace Spookline.SPC.Editor {
    [CustomEditor(typeof(Module), editorForChildClasses: true)]
    public class ModuleInspector : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            var module = (Module)target;
        }

    }
}