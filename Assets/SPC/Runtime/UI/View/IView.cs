using Cysharp.Threading.Tasks;

namespace Spookline.SPC.UI {
    public interface IView {

        public bool IsOpen { get; }

        public UniTask Open();

        public UniTask Close();

    }
}