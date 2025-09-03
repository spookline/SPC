using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Spookline.SPC.UI {
    public class SimpleView : MonoBehaviour, IView {

        private void Awake() {
            gameObject.SetActive(false);
        }

        public bool IsOpen => gameObject.activeInHierarchy;

        public UniTask Open() {
            gameObject.SetActive(true);
            return UniTask.CompletedTask;
        }

        public UniTask Close() {
            gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }

    }
}