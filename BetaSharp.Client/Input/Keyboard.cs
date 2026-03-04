using Veldrid;

namespace BetaSharp.Client.Input;

public static class Keyboard
{
    private static bool created;

    private static readonly bool[] keyDownBuffer = new bool[512];
    private static readonly Queue<KeyEventData> eventQueue = new();

    private static KeyEventData currentEvent;
    private static bool repeatEnabled;

    public static void Create()
    {
        if (created) return;
        created = true;
    }

    public static void Update(InputSnapshot snapshot)
    {
        if (!created) return;

        foreach (var evt in snapshot.KeyEvents)
        {
            int keyCode = (int)evt.Virtual; // use logical key

            if (keyCode >= 0 && keyCode < keyDownBuffer.Length)
            {
                keyDownBuffer[keyCode] = evt.Down;
            }

            eventQueue.Enqueue(new KeyEventData
            {
                Key = keyCode,
                Character = '\0',
                State = evt.Down,
                Repeat = evt.Repeat,
                Modifiers = evt.Modifiers,
                Nanos = evt.Timestamp * 1_000_000 // SDL timestamp is ms
            });
        }

        //foreach (char c in snapshot.KeyCharPresses) TODO: Reimplement
        //{
        //    eventQueue.Enqueue(new KeyEventData
        //    {
        //        Key = 0,
        //        Character = c,
        //        State = true,
        //        Repeat = false,
        //        Modifiers = 0,
        //        Nanos = GetNanos()
        //    });

        //    OnCharacterTyped?.Invoke(c);
        //}
    }

    public static event Action<char>? OnCharacterTyped;

    public static bool Next()
    {
        while (eventQueue.Count > 0)
        {
            var evt = eventQueue.Dequeue();

            if (evt.Repeat && !repeatEnabled)
                continue;

            currentEvent = evt;
            return true;
        }

        return false;
    }

    public static bool GetEventKeyState() => currentEvent.State;
    public static int GetEventKey() => currentEvent.Key;
    public static char GetEventCharacter() => currentEvent.Character;
    public static long GetEventNanoseconds() => currentEvent.Nanos;
    public static bool IsRepeatEvent() => currentEvent.Repeat;
    public static ModifierKeys GetEventModifiers() => currentEvent.Modifiers;

    public static bool IsKeyDown(VKey key)
    {
        return keyDownBuffer[(int)key];
    }

    public static void EnableRepeatEvents(bool enable)
    {
        repeatEnabled = enable;
    }

    public static void Destroy()
    {
        created = false;
        eventQueue.Clear();
    }

    private static long GetNanos()
    {
        return DateTime.UtcNow.Ticks * 100;
    }

    private struct KeyEventData
    {
        public int Key;
        public char Character;
        public bool State;
        public bool Repeat;
        public ModifierKeys Modifiers;
        public long Nanos;
    }
}
