using Spookline.SPC.Ext;
using UnityEditor;
using UnityEngine;

namespace Spookline.SPC.Editor {
    [CustomEditor(typeof(IModule), editorForChildClasses: true)]
    public class ModuleInspector : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            var module = (IModule)target;
        }

    }
}