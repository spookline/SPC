using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Spookline.SPC.Events;
using Spookline.SPC.Ext;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spookline.SPC.UI {
    public class ViewManager : Singleton<ViewManager> {

        private readonly List<IView> _stacks = new();

        public ViewManager() {
            SceneManager.sceneLoaded += (_, _) => { Clear(); };
        }

        public bool IsEmpty => _stacks.Count == 0;

        public void Clear() {
            _stacks.Clear();
            Debug.Log("[ViewManager] Cleared view stack");
        }

        public bool Contains(IView view) {
            return _stacks.Contains(view);
        }

        /// <summary>
        ///     Pushes a new <see cref="IView" /> onto the stack.
        ///     If the view already exists in the stack and is not the top view, it is removed first.
        ///     Closes the current top view asynchronously before adding the new view.
        ///     Opens the new view asynchronously after adding it to the stack.
        /// </summary>
        /// <param name="view">The view to push and open.</param>
        /// <returns>A <see cref="UniTask" /> representing the asynchronous operation.</returns>
        public async UniTask Push(IView view) {
            if (!IsEmpty) {
                var topView = _stacks[^1];
                if (topView == view) {
                    await Pop(view);
                    return;
                }

                var evt = new ViewPushEvt {
                    View = view
                }.Raise();
                if (evt.IsCancelled) return;

                var existingIndex = _stacks.IndexOf(view);
                if (existingIndex != -1 && existingIndex != _stacks.Count - 1) {
                    _stacks.RemoveAt(existingIndex);
                    Debug.Log("[ViewManager] Removed existing view from stack before pushing: " + GetNameOfView(view));
                }

                if (topView.IsOpen) {
                    Debug.Log("[ViewManager] Closing top view before pushing new view: " + GetNameOfView(topView));
                    await topView.Close();
                }
            }

            Debug.Log("[ViewManager] Pushing new view onto stack: " + GetNameOfView(view));
            _stacks.Add(view);
            await view.Open();
        }

        /// <summary>
        ///     Pops the top <see cref="IView" /> from the stack.
        ///     Closes the current top view asynchronously before removing it.
        ///     Reopens the new top view if available.
        /// </summary>
        public async UniTask Pop() {
            if (IsEmpty) return;

            var topView = _stacks[^1];
            var evt = new ViewPopEvt {
                View = topView
            }.Raise();
            if (evt.IsCancelled) return;
            Debug.Log("[ViewManager] Popping view: " + GetNameOfView(topView));
            await topView.Close();
            _stacks.RemoveAt(_stacks.Count - 1);

            if (_stacks.Count > 0) {
                var view = _stacks[^1];
                Debug.Log("[ViewManager] Reopening top view: " + GetNameOfView(view));
                await view.Open();
            }
        }

        /// <summary>
        ///     Pops the specified <see cref="IView" /> from the stack.
        ///     Closes the given view asynchronously before removing it.
        ///     If the popped view was the top view, reopens the new top view if available.
        /// </summary>
        /// <param name="view">The view to pop and close.</param>
        /// <returns>A <see cref="UniTask" /> representing the asynchronous operation.</returns>
        public async UniTask Pop(IView view) {
            if (IsEmpty || view == null) return;

            var index = _stacks.IndexOf(view);
            if (index == -1) return;
            var evt = new ViewPopEvt {
                View = view
            }.Raise();
            if (evt.IsCancelled) return;
            Debug.Log("[ViewManager] Popping specific view: " + GetNameOfView(view));
            await view.Close();
            _stacks.RemoveAt(index);

            if (index == _stacks.Count && _stacks.Count > 0) await _stacks[^1].Open();
        }

        private string GetNameOfView(IView view) {
            return view is MonoBehaviour mb ? mb.gameObject.name : view.GetType().Name;
        }

    }

    public class ViewEvt<T> : Evt<T> where T : ViewEvt<T> {

        public IView View { get; set; }

    }

    public class ViewPushEvt : ViewEvt<ViewPushEvt> {

        public bool IsCancelled { get; set; } = false;

    }

    public class ViewPopEvt : ViewEvt<ViewPopEvt> {

        public bool IsCancelled { get; set; } = false;

    }
}