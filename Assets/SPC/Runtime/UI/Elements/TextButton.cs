using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spookline.SPC {
    [UxmlElement]
    public partial class TextButton : Label {

        private bool _isHighlighted;
        private bool _isSelected;
        private Color _normalColor;

        private float _transitionDuration = 0.15f;

        public TextButton() {
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
                style.transitionProperty = new List<StylePropertyName>(new[] { new StylePropertyName("color") });
                style.transitionDuration = new List<TimeValue>(new[] { new TimeValue(value, TimeUnit.Second) });
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