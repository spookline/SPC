using System;
using Spookline.SPC.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Color = UnityEngine.Color;

namespace Spookline.SPC {
    [UxmlElement]
    public partial class Button : VisualElement {

        private readonly VisualElement _iconElement;
        private readonly Label _label;
        private ButtonColor _backgroundColor;

        private float _borderRadius;

        private Texture2D _icon;
        private Vector2 _iconSize = new(8, 8);
        private bool _isDisabled;
        private bool _isHighlighted;
        private bool _isSelected;
        private ButtonColor _outlineColor;
        private Vector2 _padding = new(32, 8);
        private ButtonColor _textColor;
        private float _textFontSize = 14f;
        private float _transitionDuration = 0.15f;

        public Button() {
            _label = new Label("Label") {
                style = {
                    fontSize = _textFontSize
                }
            };
            if (string.IsNullOrEmpty(_label.text)) _label.text = "Button";
            _label.style.display = !Icon ? DisplayStyle.Flex : DisplayStyle.None;
            _label.Padding(0);
            _label.Margin(0);

            Add(_label);

            _iconElement = new VisualElement {
                style = {
                    width = 16,
                    height = 16,
                    backgroundImage = Icon ? Icon : null,
                    display = Icon ? DisplayStyle.Flex : DisplayStyle.None
                }
            };
            Add(_iconElement);

            style.alignSelf = Align.FlexStart;
            this.Padding(Padding);

            RegisterCallback<PointerEnterEvent>(_ => { IsHighlighted = true; });
            RegisterCallback<PointerLeaveEvent>(_ => { IsHighlighted = false; });
            RegisterCallback<ClickEvent>(_ => { OnClick?.Invoke(); });
        }

        [UxmlObjectReference("textColor")]
        public ButtonColor TextColor {
            get => _textColor;
            set {
                _textColor = value;
                Render();
            }
        }

        [UxmlObjectReference("backgroundColor")]
        public ButtonColor BackgroundColor {
            get => _backgroundColor;
            set {
                _backgroundColor = value;
                Render();
            }
        }

        [UxmlObjectReference("outlineColor")]
        public ButtonColor OutlineColor {
            get => _outlineColor;
            set {
                _outlineColor = value;
                Render();
            }
        }

        [UxmlAttribute]
        public float TransitionDuration {
            get => _transitionDuration;
            set {
                _transitionDuration = value;
                Render();
            }
        }

        [UxmlAttribute]
        public string Text {
            get => _label.text;
            set => _label.text = value;
        }

        [UxmlAttribute]
        public Texture2D Icon {
            get => _icon;
            set {
                _icon = value;
                _label.style.display = !value ? DisplayStyle.Flex : DisplayStyle.None;
                _iconElement.style.backgroundImage = value ? value : null;
                _iconElement.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        [UxmlAttribute]
        public Vector2 IconSize {
            get => _iconSize;
            set {
                _iconSize = value;
                _iconElement.style.width = value.x;
                _iconElement.style.height = value.y;
            }
        }

        [UxmlAttribute]
        public float TextFontSize {
            get => _textFontSize;
            set {
                _textFontSize = value;
                _label.style.fontSize = value;
            }
        }

        [UxmlAttribute]
        public Vector2 Padding {
            get => _padding;
            set {
                _padding = value;
                this.Padding(value);
            }
        }

        [UxmlAttribute]
        public bool IsSelected {
            get => _isSelected;
            set {
                _isSelected = value;
                Render();
            }
        }

        [UxmlAttribute]
        public bool IsHighlighted {
            get => _isHighlighted;
            set {
                _isHighlighted = value;
                Render();
            }
        }

        [UxmlAttribute]
        public float BorderRadius {
            get => _borderRadius;
            set {
                _borderRadius = value;
                Render();
            }
        }

        [UxmlAttribute]
        public bool IsDisabled {
            get => _isDisabled;
            set {
                _isDisabled = value;
                Render();
            }
        }

        public event Action OnClick;

        private void Render() {
            this.TransitionProperties("all");
            this.TransitionDuration(_transitionDuration);
            this.BorderRadius(BorderRadius);
            if (BackgroundColor != null) {
                style.backgroundColor = IsSelected
                    ? BackgroundColor.selected
                    : BackgroundColor.normal;
                if (IsHighlighted && !IsSelected) style.backgroundColor = BackgroundColor.highlighted;

                if (IsDisabled) style.backgroundColor = BackgroundColor.disabled;
            } else {
                style.backgroundColor = Color.clear;
            }

            if (TextColor != null) {
                _label.style.color = IsSelected
                    ? TextColor.selected
                    : TextColor.normal;
                if (IsHighlighted && !IsSelected) _label.style.color = TextColor.highlighted;
                if (IsDisabled) style.backgroundColor = TextColor.disabled;
            } else {
                _label.style.color = Color.clear;
            }

            if (OutlineColor != null) {
                this.BorderColor(IsSelected
                    ? OutlineColor.selected
                    : OutlineColor.normal);
                if (IsHighlighted && !IsSelected) this.BorderColor(OutlineColor.highlighted);
                if (IsDisabled) this.BorderColor(OutlineColor.disabled);
                this.BorderWidth(1f);
            } else {
                this.BorderColor(Color.clear);
                this.BorderWidth(0f);
            }
        }

    }

    [UxmlObject]
    public partial class ButtonColor {

        [UxmlAttribute]
        public Color disabled;

        [UxmlAttribute]
        public Color highlighted;

        [UxmlAttribute]
        public Color normal;

        [UxmlAttribute]
        public Color selected;

    }
}