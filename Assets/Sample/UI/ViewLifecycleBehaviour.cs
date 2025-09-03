using Cysharp.Threading.Tasks;
using Spookline.SPC.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sample.UI {
    public class ViewLifecycleBehaviour : MonoBehaviour {

        public SimpleView escapeView;
        public SimpleView view1;
        public SimpleView view2;

        private void Start() {
            ViewManager.Instance.Push(escapeView).Forget();
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                if (ViewManager.Instance.IsEmpty) {
                    ViewManager.Instance.Push(escapeView).Forget();
                    return;
                }
                ViewManager.Instance.Pop().Forget();
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                ViewManager.Instance.Push(view1).Forget();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2)) {
                ViewManager.Instance.Push(view2).Forget();
            }
        }

    }
}