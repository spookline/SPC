using Cysharp.Threading.Tasks;
using Spookline.SPC.UI;
using UnityEngine;

namespace Sample.UI {
    public class ViewSampleBehaviour : MonoBehaviour {

        public SimpleView escapeView;
        public SimpleView view1;
        public SimpleView view2;

        private void Start() {
            ViewManager.Instance.Push(escapeView).Forget();
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                if (ViewManager.Instance.IsEmpty) {
                    escapeView.Push().Forget();
                    return;
                }
                ViewManager.Instance.Pop().Forget();
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) view1.Push().Forget();
            if (Input.GetKeyDown(KeyCode.Alpha2)) view2.Push().Forget();
        }

    }
}