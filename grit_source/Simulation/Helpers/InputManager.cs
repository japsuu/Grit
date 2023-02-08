using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;

namespace Grit.Simulation;

public static class InputManager
{
    public static MouseStateExtended Mouse { get; private set; }
    public static KeyboardStateExtended Keyboard { get; private set; }
    public static Vector2 MouseAbsoluteWorldPosition => Grit.MainCamera.ScreenToWorld(Mouse.Position.X, Mouse.Position.Y);
    public static Point MousePixelWorldPosition
    {
        get
        {
            Vector2 absolute = MouseAbsoluteWorldPosition;
            Point floored = new((int)Math.Floor(absolute.X), (int)Math.Floor(absolute.Y));
            return floored;
        }
    }

    public static bool IsMouseButtonDown(MouseButton button) => Mouse.IsButtonDown(button);
    public static bool IsMouseButtonUp(MouseButton button) => Mouse.IsButtonUp(button);
    public static bool WasKeyJustDown(Keys key) => Keyboard.WasKeyJustDown(key);
    public static bool WasKeyJustUp(Keys key) => Keyboard.WasKeyJustUp(key);
    public static bool IsKeyDown(Keys key) => Keyboard.IsKeyDown(key);
    public static bool IsKeyUp(Keys key) => Keyboard.IsKeyUp(key);
    
    public static void Update()
    {
        Mouse = MouseExtended.GetState();
        Keyboard = KeyboardExtended.GetState();
    }
}