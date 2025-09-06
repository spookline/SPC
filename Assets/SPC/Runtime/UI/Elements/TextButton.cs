using System;
using Spookline.SPC.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spookline.SPC {
    [UxmlElement]
    public partial class TextButton : Label {

        private bool _isHighlighted;
        private bool _isSelected;
        private readonly Label _label;
        private Color _normalColor;

        private float _transitionDuration = 0.15f;

        public TextButton() {
            _label = this;
            RegisterCallback<PointerEnterEvent>(_ => { IsHighlighted = true; });
            RegisterCallback<PointerLeaveEvent>(_ => { IsHighlighted = false; });
            RegisterCallback<ClickEvent>(_ => { OnClick?.Invoke(); });
        }

        [UxmlAttribute]
        public Color NormalColor {
            get => _normalColor;
            set {
                _normalColor = value;
                if (IsSelected) return;
                style.color = value;
            }
        }

        [UxmlAttribute]
        public Color SelectedColor { get; set; }

        [UxmlAttribute]
        public Color HighlightColor { get; set; }

        [UxmlAttribute]
        public float TransitionDuration {
            get => _transitionDuration;
            set {
                _transitionDuration = value;
                _label.TransitionDuration(value);
                _label.TransitionProperties("color");
            }
        }

        [UxmlAttribute]
        public bool IsSelected {
            get => _isSelected;
            set {
                _isSelected = value;
                style.color = value
                    ? SelectedColor
                    : NormalColor;
            }
        }

        [UxmlAttribute]
        public bool IsHighlighted {
            get => _isHighlighted;
            set {
                _isHighlighted = value;
                if (IsSelected) return;
                style.color = value
                    ? HighlightColor
                    : NormalColor;
            }
        }

        public event Action OnClick;

    }
}