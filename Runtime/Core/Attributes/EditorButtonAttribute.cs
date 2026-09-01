using System;

namespace VADE.DevTools.Attributes
{
    public enum ButtonMode
    {

        Always,

        EditModeOnly,

        PlayModeOnly
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class EditorButtonAttribute : Attribute
    {
        public string Label { get; }
        public ButtonMode Mode { get; }

        public EditorButtonAttribute(string label = null, ButtonMode mode = ButtonMode.Always)
        {
            Label = label;
            Mode = mode;
        }
    }
}
