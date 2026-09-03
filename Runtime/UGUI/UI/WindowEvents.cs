namespace VADE.DevTools.UI
{
    public readonly struct WindowOpenedEvent
    {
        public readonly Window window;
        public WindowOpenedEvent(Window window) => this.window = window;
    }

    public readonly struct WindowClosedEvent
    {
        public readonly Window window;
        public WindowClosedEvent(Window window) => this.window = window;
    }
}
