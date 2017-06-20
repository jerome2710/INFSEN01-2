using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GUIapp {

    public interface Option<T> {
        U Visit<U>(Func<U> onNone, Func<T, U> onSome);
        void Visit(Action onNone, Action<T> onSome);
    }

    public class None<T> : Option<T> {

        public U Visit<U>(Func<U> onNone, Func<T, U> onSome) {
            return onNone();
        }

        public void Visit(Action onNone, Action<T> onSome) {
            onNone();
        }
    }

    public class Some<T> : Option<T> {
        T value;

        public Some(T value) {
            this.value = value;
        }

        public U Visit<U>(Func<U> onNone, Func<T, U> onSome) {
            return onSome(value);
        }

        public void Visit(Action onNone, Action<T> onSome) {
            onSome(value);
        }
    }

    public interface Iterator<T> {
        Option<T> GetNext();
        Option<T> GetCurrent();
        void Reset();
    }

    public class ElementList<T> : Iterator<T> {
        private List<T> list = new List<T>();
        private int current = -1;

        public ElementList(List<T> list) {
            this.list = list;
        }

        public ElementList() { }

        public void Add(T option) {
            this.list.Add(option);
        }

        public Option<T> GetCurrent() {

            // ugly AF
            if (current == -1) { return new None<T>(); }

            if (current < list.Count) {
                return new Some<T>(list[current]);
            }

            return new None<T>();
        }

        public Option<T> GetNext() {
            current++;
            return this.GetCurrent();
        }

        public void Reset() {
            this.current = -1;
        }
    }

    public class Point {
        public Point(float x, float y) {
            this.X = x;
            this.Y = y;
        }

        public float X { get; set; }
        public float Y { get; set; }
    }


    public enum Colour { White, Black, Blue };

    public interface InputManager {
        Option<Point> Click();
    }

    public class MonogameMouse : InputManager {
        public Option<Point> Click() {

			var mouse = Mouse.GetState();

			if (mouse.LeftButton == ButtonState.Pressed) {
                return new Some<Point>(new Point(mouse.X, mouse.Y));
			} else {
                return new None<Point>();
			}
        }
    }

    public interface DrawingManager {
		void DrawRectangle(Point top_left_coordinate, float width, float height, Colour color);
		void DrawString(string text, Point top_left_coordinate, int size, Colour color);
	}

    public class MonogameDrawingAdapter : DrawingManager {
        SpriteBatch sprite_batch;
        ContentManager content_manager;

        Texture2D white_pixel;
        SpriteFont default_font;
        Game game;

        public MonogameDrawingAdapter(SpriteBatch sprite_batch, ContentManager content_manager) {
            this.sprite_batch = sprite_batch;
            this.content_manager = content_manager;

            white_pixel = this.content_manager.Load<Texture2D>("white_pixel");
            default_font = this.content_manager.Load<SpriteFont>("arial");
        }

        public void DrawRectangle(Point top_left_coordinate, float width, float height, Colour color) {
            sprite_batch.Draw(
                white_pixel,
                new Rectangle(
                    (int)top_left_coordinate.X,
                    (int)top_left_coordinate.Y,
                    (int)width,
                    (int)height
                ),
                this.ConvertColor(color)
            );
        }

        public void DrawString(string text, Point top_left_coordinate, int size, Colour color) {
            sprite_batch.DrawString(
                default_font,
                text,
                new Vector2(
                    top_left_coordinate.X,
                    top_left_coordinate.Y
                ),
                this.ConvertColor(color)
            );
        }

        private Color ConvertColor(Colour color) {
            switch (color) {
                case Colour.White:
                    return new Color(255, 255, 255);
                case Colour.Blue:
                    return new Color(0, 0, 255);
                default:
                    return new Color(0, 0, 0);
            }
        }
    }

    public interface Updateable {
        void Update(UpdateVisitor visitor, float dt);
    }

    public interface Drawable {
        void Draw(DrawVisitor visitor);
    }


    public interface DrawVisitor {
        void DrawButton(Button element);
        void DrawLabel(Label element);
        void DrawGui(GuiManager element);
    }

    public class DefaultDrawVisitor : DrawVisitor {

        DrawingManager drawing_manager;

        public DefaultDrawVisitor(DrawingManager drawing_manager) {
            this.drawing_manager = drawing_manager;
        }

        public void DrawButton(Button element) {
            drawing_manager.DrawRectangle(
                element.top_left_corner,
                element.width,
                element.height,
                element.color
            );

            element.element.Draw(this);
        }

        public void DrawLabel(Label element) {
            drawing_manager.DrawString(
                element.content,
                element.top_left_corner,
                element.size,
                element.color
            );
        }

        public void DrawGui(GuiManager gui_manager) {
            gui_manager.elements.Reset();
            while (gui_manager.elements.GetNext().Visit(() => false, _ => true)) {
                gui_manager.elements.GetCurrent().Visit(() => { }, item => { item.Draw(this); });
            }
        }
    }

    public interface UpdateVisitor {
        void UpdateButton(Button element, float dt);
        void UpdateLabel(Label element, float dt);
        void UpdateGui(GuiManager gui_manager, float dt);
    }

    public class DefaultUpdateVisitor : UpdateVisitor {

        InputManager input_manager;

        public DefaultUpdateVisitor(InputManager input_manager) {
            this.input_manager = input_manager;
        }

        public void UpdateButton(Button element, float dt) {

            input_manager.Click().Visit(
                () => { element.color = Colour.White; },
                position => { if (element.is_intersecting(position)) {
                        element.color = Colour.Blue;
                        element.action();
                    }
                }
            );
        }

        public void UpdateLabel(Label element, float dt) {

        }

        public void UpdateGui(GuiManager gui_manager, float dt) {
            gui_manager.elements.Reset();

            while (gui_manager.elements.GetNext().Visit(() => false, _ => true)) {
                gui_manager.elements.GetCurrent().Visit(() => { }, item => { item.Update(this, dt); });
            }
        }
    }
}