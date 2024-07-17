namespace CSkyL.Game.Control
{
    using UnityEngine;
    public class InputManager
    {
        public static float MouseMoveHori => Input.GetAxis("Mouse X");  // +/-: right/left
        public static float MouseMoveVert => Input.GetAxis("Mouse Y");  // +/-: up/down
        public static float MouseScroll => Input.GetAxisRaw("Mouse ScrollWheel");  // +/i: up/down

        public static bool MouseTriggered(MouseButton btn)
            => Input.GetMouseButtonDown((int) btn);
        public static bool MousePressed(MouseButton btn)
            => Input.GetMouseButton((int) btn);
        public static bool KeyTriggered(KeyCode key) => Input.GetKeyDown(key);
        public static bool KeyPressed(KeyCode key) => Input.GetKey(key);

        public enum MouseButton : int { Primary = 0, Secondary = 1, Middle = 2 }
        public static void ToggleCursor(bool visibility) => Cursor.visible = visibility;
    }
}

