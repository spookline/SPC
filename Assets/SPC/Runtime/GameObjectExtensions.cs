using UnityEngine;

namespace Spookline.SPC {
    public static class GameObjectExtensions {
        public static void ChangeLayerRecursively(this GameObject obj, int newLayer) {
            if (!obj) return;
            obj.layer = newLayer;
            foreach (Transform child in obj.transform) {
                ChangeLayerRecursively(child.gameObject, newLayer);
            }
        }

    }
}