using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace Enlishing
{
    public static class ConsoleEx
    {
        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        private static extern bool ReadConsoleInput(
            IntPtr hConsoleInput,
            [Out] INPUT_RECORD[] lpBuffer,
            uint nLength,
            out uint lpNumberOfEventsRead
        );

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern bool ReadConsoleOutputCharacter(
            IntPtr hConsoleOutput,
            [Out] char[] lpCharacter,
            uint nLength,
            COORD dwReadCoord,
            out uint lpNumberOfCharsRead
        );

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool bMaximumWindow, ref FontInfo lpConsoleCurrentFontEx);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool GetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool bMaximumWindow, ref FontInfo lpConsoleCurrentFontEx);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleOutputCP(uint wCodePageID);

        [DllImport("user32.dll")]
        private static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        private static extern bool DrawMenuBar(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private const int STD_INPUT_HANDLE = -10;
        private const int STD_OUTPUT_HANDLE = -11;

        private const uint ENABLE_PROCESSED_INPUT = 0x0001;
        private const uint ENABLE_MOUSE_INPUT = 0x0010;
        private const uint ENABLE_EXTENDED_FLAGS = 0x0080;

        private const uint ENABLE_PROCESSED_OUTPUT = 0x0001;
        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        private const int MF_BYCOMMAND = 0x00000000;
        private const int SC_CLOSE = 0xf060;
        private const int SC_MINIMIZE = 0xf020;
        private const int SC_MAXIMIZE = 0xf030;
        private const int SC_SIZE = 0xf000;

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        [StructLayout(LayoutKind.Sequential)]
        public struct COORD
        {
            public short X;
            public short Y;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct MOUSE_EVENT_RECORD
        {
            [FieldOffset(0)] public COORD dwMousePosition;
            [FieldOffset(4)] public uint dwButtonState;
            [FieldOffset(8)] public uint dwControlKeyState;
            [FieldOffset(12)] public uint dwEventFlags;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct INPUT_RECORD
        {
            [FieldOffset(0)] public ushort EventType;
            [FieldOffset(4)] public MOUSE_EVENT_RECORD MouseEvent;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct FontInfo
        {
            public int cbSize;
            public int FontIndex;
            public COORD FontSize;
            public int FontFamily;
            public int FontWeight;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string FontName;
        }

        [Flags]
        public enum Button : uint
        {
            None = 0x0000,
            Left = 0x0001,
            Right = 0x0002,
            Middle = 0x0004,
            XButton1 = 0x0008,
            XButton2 = 0x0010
        }

        [Flags]
        public enum ControlKey : uint
        {
            None = 0x0000,
            RightAlt = 0x0001,
            LeftAlt = 0x0002,
            RightCtrl = 0x0004,
            LeftCtrl = 0x0008,
            Shift = 0x0010,
            NumLock = 0x0020,
            ScrollLock = 0x0040,
            CapsLock = 0x0080,
            Enhanced = 0x0100
        }

        [Flags]
        public enum EventFlags : uint
        {
            None = 0x0000,
            Move = 0x0001,
            DoubleClick = 0x0002,
            Wheel = 0x0004,
            HWheel = 0x0008
        }

        public static void FocusConsole()
        {
            IntPtr handle = GetConsoleWindow();
            SetForegroundWindow(handle);
        }

        public static void ShowConsole(bool show)
        {
            IntPtr handle = GetConsoleWindow();
            ShowWindow(handle, show ? SW_SHOW : SW_HIDE);
        }

        public static void RemoveMenus(bool size, bool minimize, bool maximize, bool close)
        {
            IntPtr handle = GetConsoleWindow();
            IntPtr sysMenu = GetSystemMenu(handle, false);

            if (handle != IntPtr.Zero)
            {
                if (size) DeleteMenu(sysMenu, SC_SIZE, MF_BYCOMMAND);
                if (minimize) DeleteMenu(sysMenu, SC_MINIMIZE, MF_BYCOMMAND);
                if (maximize) DeleteMenu(sysMenu, SC_MAXIMIZE, MF_BYCOMMAND);
                if (close) DeleteMenu(sysMenu, SC_CLOSE, MF_BYCOMMAND);
                DrawMenuBar(handle);
            }
        }

        public static void SetFont(string font, COORD? fontSize = null)
        {
            IntPtr outputHandle = GetStdHandle(STD_OUTPUT_HANDLE);

            FontInfo before = new() { cbSize = Marshal.SizeOf<FontInfo>() };
            if (GetCurrentConsoleFontEx(outputHandle, false, ref before))
            {
                FontInfo set = new()
                {
                    cbSize = Marshal.SizeOf<FontInfo>(),
                    FontIndex = 0,
                    FontFamily = 0x30,
                    FontName = font,
                    FontWeight = 400,
                    FontSize = fontSize ?? before.FontSize
                };

                if (!SetCurrentConsoleFontEx(outputHandle, true, ref set))
                    Console.WriteLine(Marshal.GetLastWin32Error());
            }
        }

        public static void InitConsole()
        {
            IntPtr outputHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            GetConsoleMode(outputHandle, out uint mode);
            SetConsoleMode(outputHandle, mode | ENABLE_PROCESSED_OUTPUT | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
            SetConsoleOutputCP(65001);
        }

        public static string ColorPrefix(Color color, bool fgOrBg)
        {
            return $"\x1b[{(fgOrBg ? 38 : 48)};2;{color.R};{color.G};{color.B}m";
        }

        public static string ColorPostfix() => "\x1b[0m";

        public static string Color(this string str, Color? fg = null, Color? bg = null, bool reset = true)
        {
            if (fg == null && bg == null)
                return str; // why?
            StringBuilder builder = new();
            if (fg != null)
                builder.Append(ColorPrefix(fg.Value, true));
            if (bg != null)
                builder.Append(ColorPrefix(bg.Value, false));
            builder.Append(str);
            if (reset)
                builder.Append(ColorPostfix());
            return builder.ToString();
        }

        public delegate bool OnMouseClick(short x, short y, Button btn, ControlKey ctrl, EventFlags flags, char clicked);

        public static void WaitFor(OnMouseClick onClick, bool retrieveClicked = true, Action onRepeat = null)
        {
            IntPtr inputHandle = GetStdHandle(STD_INPUT_HANDLE);
            IntPtr outputHandle = retrieveClicked ? GetStdHandle(STD_OUTPUT_HANDLE) : 0;

            GetConsoleMode(inputHandle, out uint mode);
            SetConsoleMode(inputHandle, ENABLE_PROCESSED_INPUT | ENABLE_MOUSE_INPUT | ENABLE_EXTENDED_FLAGS);

            while (true)
            {
                onRepeat?.Invoke();

                INPUT_RECORD[] inputRecords = new INPUT_RECORD[1];

                ReadConsoleInput(inputHandle, inputRecords, 1, out _);

                var record = inputRecords[0];
                if (record.EventType == 2)
                {
                    var mouseEvent = record.MouseEvent;
                    COORD clickPos = mouseEvent.dwMousePosition;

                    char chr = default;
                    if (retrieveClicked)
                    {
                        char[] character = new char[1];
                        ReadConsoleOutputCharacter(
                            outputHandle,
                            character,
                            1,
                            clickPos,
                            out _
                        );
                        chr = character[0];
                    }
                    bool stop = onClick(clickPos.X,
                            clickPos.Y,
                            (Button)mouseEvent.dwButtonState,
                            (ControlKey)mouseEvent.dwControlKeyState,
                            (EventFlags)mouseEvent.dwEventFlags,
                            chr);
                    if (stop)
                        break;
                }
            }

            SetConsoleMode(inputHandle, mode);
        }
    }
}
