using System;
using UnityEngine;

namespace Spookline.SPC.Ext
{
    public abstract class SpookManagerBehaviour<T> : SpookBehaviour where T : SpookManagerBehaviour<T> {

        public static T Instance { get; private set; }

        public static bool HasInstance => Instance;

        protected virtual void Awake() {
            if (HasInstance) {
                Debug.LogError($"Instance of {typeof(T).Name} already exists. Destroying this instance.");
                Destroy(gameObject);
            }
            
            Instance = (T)this;
        }

    }
}