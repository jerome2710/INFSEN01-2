using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GUIapp {

    public abstract class GuiMenuCreator {
        public abstract GuiManager Instantiate(string option, System.Action exit);
    }

    public class GuiConstructor : GuiMenuCreator {

        public override GuiManager Instantiate(string option, System.Action exit) {

            GuiManager guiManager = new GUIapp.GuiManager();

            switch (option) {
                case "1":

                    LabelConstructor labelConstructor = new LabelConstructor();
                    ButtonConstructor buttonConstructor = new ButtonConstructor();

                    guiManager.elements = new ElementList<GuiElement>();
                    guiManager.elements.Add(labelConstructor.Instantiate("Hi Ahmed!", new Point(0, 0), 10, Colour.Black));
                    guiManager.elements.Add(buttonConstructor.Instantiate("Click me", new Point(0, 100), 10, Colour.Black, 100, 30,
                        () => {
                            guiManager.elements = new ElementList<GuiElement>();
                            guiManager.elements.Add(buttonConstructor.Instantiate("Exit", new Point(0, 0), 10, Colour.Black, 100, 30,
                              () => {
                                  exit();
                              }
                             ));
                        }
                   ));
                    break;

                default:
                    throw new NotImplementedException();
            }

            return guiManager;
        }
    }

    public abstract class GuiElementCreator {
        public abstract GuiElement Instantiate(string text, Point top_left_corner, int size, Colour color);
        public abstract GuiElement Instantiate(string text, Point top_left_corner, int size, Colour color, float width, float height, Action action);
    }

    public class ButtonConstructor : GuiElementCreator {
        public override GuiElement Instantiate(string text, Point top_left_corner, int size, Colour color) {
            return new Button(text, top_left_corner, size, color, 40, 40, () => { });
        }

        public override GuiElement Instantiate(string text, Point top_left_corner, int size, Colour color, float width, float height, Action action) {
            return new Button(text, top_left_corner, size, color, width, height, action);
        }
    }

    public class LabelConstructor : GuiElementCreator
    {
        public override GuiElement Instantiate(string text, Point top_left_corner, int size, Colour color) {
            return new Label(text, top_left_corner, size, color);
        }

        public override GuiElement Instantiate(string text, Point top_left_corner, int size, Colour color, float width, float height, Action action) {
            return new Label(text, top_left_corner, size, color);
        }
    }

    public interface GuiElement : Drawable, Updateable { }

    public class Label : GuiElement {
        public string content;

        public int size;
        public Colour color;
        public Point top_left_corner;

        public Label(string content, Point top_left_corner, int size, Colour color) {
            this.size = size;
            this.color = color;
            this.content = content;
            this.top_left_corner = top_left_corner;

        }

        public void Draw(DrawVisitor visitor) {
            visitor.DrawLabel(this);
        }

        public void Update(UpdateVisitor visitor, float dt) { }
    }

    public class Button : GuiElement {
        public float width, height;
        public Action action;
        public Colour color;
        public Label label;
        public Point top_left_corner;

        public Button(string text, Point top_left_corner, int size, Colour color, float width, float height, Action action) {
            this.action = action;
            this.width = width;
            this.height = height;
            this.color = color;
            this.top_left_corner = top_left_corner;
            label = new Label(text, top_left_corner, size, color);
        }

        public void Draw(DrawVisitor visitor) {
            visitor.DrawButton(this);
        }

        public bool is_intersecting(Point point) {
            return point.X > top_left_corner.X && point.Y > top_left_corner.Y &&
                   point.X < top_left_corner.X + width && point.Y < top_left_corner.Y + height;
        }

        public void Update(UpdateVisitor visitor, float dt) {
            visitor.UpdateButton(this, dt);
        }
    }


    public class GuiManager : Updateable, Drawable {
        public ElementList<GuiElement> elements;

        public void Draw(DrawVisitor visitor) {
            visitor.DrawGui(this);
        }

        public void Update(UpdateVisitor visitor, float dt) {
            visitor.UpdateGui(this, dt);
        }
    }
}