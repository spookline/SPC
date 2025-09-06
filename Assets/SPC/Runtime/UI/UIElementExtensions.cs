using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spookline.SPC.UI {
    public static class UIElementExtensions {

        public static Vector2 GetPanelSize(this VisualElement element) {
            return element.panel?.visualTree?.contentRect.size ?? new Vector2(1080, 720);
        }

        public static Vector2 InverseTransform(this VisualElement element, Vector2 position) {
            var worldBound = element.worldBound;
            var x = position.x - worldBound.xMin;
            var y = position.y - worldBound.yMin;
            return new Vector2(x, y);
        }

        public static Vector2 Transform(this VisualElement element, Vector2 position) {
            var worldBound = element.worldBound;
            var x = position.x + worldBound.xMin;
            var y = position.y + worldBound.yMin;
            return new Vector2(x, y);
        }


        public static VisualElement Root(this VisualElement element) {
            var root = element;
            while (root.parent != null) root = root.parent;
            return root;
        }

        public static Vector2 TopLeft(this Rect rect) {
            return new Vector2(rect.x, rect.y);
        }

        public static Vector2 TopRight(this Rect rect) {
            return new Vector2(rect.x + rect.width, rect.y);
        }

        public static Vector2 BottomLeft(this Rect rect) {
            return new Vector2(rect.x, rect.y + rect.height);
        }

        public static Vector2 BottomRight(this Rect rect) {
            return new Vector2(rect.x + rect.width, rect.y + rect.height);
        }

        public static Vector2 Center(this Rect rect) {
            return new Vector2(rect.x + rect.width / 2, rect.y + rect.height / 2);
        }


        public static Rect Inflate(this Rect rect, float value) {
            return new Rect(rect.x - value, rect.y - value, rect.width + value * 2, rect.height + value * 2);
        }

        public static Rect Deflate(this Rect rect, float value) {
            return new Rect(rect.x + value, rect.y + value, rect.width - value * 2, rect.height - value * 2);
        }

        public static Rect Move(this Rect rect, Vector2 value) {
            return new Rect(rect.x + value.x, rect.y + value.y, rect.width, rect.height);
        }

        public static Rect Move(this Rect rect, float x, float y) {
            return new Rect(rect.x + x, rect.y + y, rect.width, rect.height);
        }

        public static Rect Move(this Rect rect, float value) {
            return new Rect(rect.x + value, rect.y + value, rect.width, rect.height);
        }

        public static Rect WithX(this Rect rect, float x) {
            return new Rect(x, rect.y, rect.width, rect.height);
        }

        public static Rect WithY(this Rect rect, float y) {
            return new Rect(rect.x, y, rect.width, rect.height);
        }

        public static Rect WithPosition(this Rect rect, Vector2 position) {
            return new Rect(position.x, position.y, rect.width, rect.height);
        }

        public static Rect ClampPosition(this Rect rect, Vector2 min, Vector2 max) {
            return new Rect(Mathf.Clamp(rect.x, min.x, max.x), Mathf.Clamp(rect.y, min.y, max.y), rect.width,
                rect.height);
        }


        public static Rect WithWidth(this Rect rect, float width) {
            return new Rect(rect.x, rect.y, width, rect.height);
        }

        public static Rect WithHeight(this Rect rect, float height) {
            return new Rect(rect.x, rect.y, rect.width, height);
        }

        public static Rect SubWidth(this Rect rect, float width) {
            return new Rect(rect.x, rect.y, rect.width - width, rect.height);
        }

        public static Rect SubHeight(this Rect rect, float height) {
            return new Rect(rect.x, rect.y, rect.width, rect.height - height);
        }

        public static Rect Scale(this Rect rect, float value) {
            return new Rect(rect.x, rect.y, rect.width * value, rect.height * value);
        }

        public static Rect Scale(this Rect rect, float x, float y) {
            return new Rect(rect.x, rect.y, rect.width * x, rect.height * y);
        }

        public static Rect Scale(this Rect rect, Vector2 vector2) {
            return new Rect(rect.x, rect.y, rect.width * vector2.x, rect.height * vector2.y);
        }

        public static Rect WithSize(this Rect rect, Vector2 size) {
            return new Rect(rect.x, rect.y, size.x, size.y);
        }

        public static Rect ClampSize(this Rect rect, Vector2 min, Vector2 max) {
            return new Rect(rect.x, rect.y, Mathf.Clamp(rect.width, min.x, max.x),
                Mathf.Clamp(rect.height, min.y, max.y));
        }


        public static void Rect(this Painter2D painter, Rect rect) {
            painter.BeginPath();
            painter.MoveTo(rect.position);
            painter.LineTo(rect.position + new Vector2(rect.width, 0));
            painter.LineTo(rect.position + new Vector2(rect.width, rect.height));
            painter.LineTo(rect.position + new Vector2(0, rect.height));
            painter.ClosePath();
        }

        public static void RRect(this Painter2D painter, Rect rect, float radius) {
            painter.BeginPath();
            painter.MoveTo(new Vector2(rect.x + radius, rect.y));
            painter.LineTo(new Vector2(rect.x + rect.width - radius, rect.y));
            painter.ArcTo(new Vector2(rect.x + rect.width, rect.y), new Vector2(rect.x + rect.width, rect.y + radius),
                radius);
            painter.LineTo(new Vector2(rect.x + rect.width, rect.y + rect.height - radius));
            painter.ArcTo(new Vector2(rect.x + rect.width, rect.y + rect.height),
                new Vector2(rect.x + rect.width - radius, rect.y + rect.height), radius);
            painter.LineTo(new Vector2(rect.x + radius, rect.y + rect.height));
            painter.ArcTo(new Vector2(rect.x, rect.y + rect.height), new Vector2(rect.x, rect.y + rect.height - radius),
                radius);
            painter.LineTo(new Vector2(rect.x, rect.y + radius));
            painter.ArcTo(new Vector2(rect.x, rect.y), new Vector2(rect.x + radius, rect.y), radius);
            painter.ClosePath();
        }


        public static Rect Canvas(this MeshGenerationContext context) {
            return context.visualElement.layout.WithPosition(Vector2.zero);
        }

        public static Vector2 CanvasSize(this MeshGenerationContext context) {
            return context.visualElement.layout.size;
        }

        public static Color WithAlpha(this Color color, float alpha) {
            return new Color(color.r, color.g, color.b, alpha);
        }

        public static Color FromHex(string hex) {
            ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }

        public static void FullExpand(this VisualElement element, float inset = 0) {
            element.style.position = Position.Absolute;
            element.style.left = inset;
            element.style.top = inset;
            element.style.right = inset;
            element.style.bottom = inset;
        }


        public static void NoPadding(this VisualElement element) {
            element.style.paddingTop = 0;
            element.style.paddingLeft = 0;
            element.style.paddingRight = 0;
            element.style.paddingBottom = 0;
        }

        public static void NoMargin(this VisualElement element) {
            element.style.marginTop = 0;
            element.style.marginLeft = 0;
            element.style.marginRight = 0;
            element.style.marginBottom = 0;
        }

        public static void NoBorder(this VisualElement element) {
            element.style.borderTopWidth = 0;
            element.style.borderLeftWidth = 0;
            element.style.borderRightWidth = 0;
            element.style.borderBottomWidth = 0;
        }

        public static void NoPaddingAndMargin(this VisualElement element) {
            element.NoPadding();
            element.NoMargin();
        }

        public static void Padding(this VisualElement element, float value) {
            element.style.paddingTop = value;
            element.style.paddingLeft = value;
            element.style.paddingRight = value;
            element.style.paddingBottom = value;
        }

        public static void Padding(this VisualElement element, Vector2 value) {
            element.style.paddingTop = value.y;
            element.style.paddingLeft = value.x;
            element.style.paddingRight = value.x;
            element.style.paddingBottom = value.y;
        }

        public static void Margin(this VisualElement element, float value) {
            element.style.marginTop = value;
            element.style.marginLeft = value;
            element.style.marginRight = value;
            element.style.marginBottom = value;
        }

        public static void BorderRadius(this VisualElement element, float value) {
            element.style.borderTopLeftRadius = value;
            element.style.borderTopRightRadius = value;
            element.style.borderBottomLeftRadius = value;
            element.style.borderBottomRightRadius = value;
        }

        public static void BorderColor(this VisualElement element, Color color) {
            element.style.borderTopColor = color;
            element.style.borderLeftColor = color;
            element.style.borderRightColor = color;
            element.style.borderBottomColor = color;
        }

        public static void BorderWidth(this VisualElement element, float value) {
            element.style.borderTopWidth = value;
            element.style.borderLeftWidth = value;
            element.style.borderRightWidth = value;
            element.style.borderBottomWidth = value;
        }

        public static void TransitionProperties(this VisualElement element, params string[] propertyNames) {
            var stylePropertyNames = new StylePropertyName[propertyNames.Length];
            for (var i = 0; i < propertyNames.Length; i++)
                stylePropertyNames[i] = new StylePropertyName(propertyNames[i]);
            element.style.transitionProperty = new List<StylePropertyName>(stylePropertyNames);
        }

        public static void TransitionDuration(this VisualElement element, float seconds) {
            element.style.transitionDuration = new List<TimeValue>(new[] { new TimeValue(seconds, TimeUnit.Second) });
        }

    }
}