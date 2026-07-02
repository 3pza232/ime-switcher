using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;

namespace ImeSwitcher;

class App
{
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")]
    static extern uint GetWindowThreadProcessId(IntPtr h, IntPtr p);
    [DllImport("user32.dll")]
    static extern IntPtr GetKeyboardLayout(uint tid);
    [DllImport("user32.dll")]
    static extern int GetKeyboardLayoutList(int n, IntPtr[]? l);
    [DllImport("user32.dll")]
    static extern bool PostMessage(IntPtr h, uint m, IntPtr wp, IntPtr lp);
    [DllImport("user32.dll")]
    static extern IntPtr ActivateKeyboardLayout(IntPtr h, uint f);
    const uint WM_INPUTLANGCHANGEREQUEST = 0x0050;

    static IntPtr ActiveLayout() =>
        GetKeyboardLayout(GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero));

    static ushort LangId(string name)
    {
        try { return (ushort)CultureInfo.GetCultureInfo(name).LCID; }
        catch { return 0; }
    }

    static bool SwitchTo(string culture)
    {
        var langId = LangId(culture);
        if (langId == 0) return false;

        var count = GetKeyboardLayoutList(0, null);
        if (count <= 0) return false;
        var layouts = new IntPtr[count];
        GetKeyboardLayoutList(count, layouts);

        var before = ActiveLayout();
        foreach (var hkl in layouts)
        {
            if ((hkl.ToInt64() & 0xFFFF) != langId) continue;
            if (hkl == before) return true;
            PostMessage(GetForegroundWindow(), WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, hkl);
            Thread.Sleep(8);
            if (ActiveLayout() != before) return true;
            ActivateKeyboardLayout(hkl, 0);
            Thread.Sleep(8);
            return ActiveLayout() != before;
        }
        return false;
    }

    static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            SwitchTo(args[0]);
            return;
        }

        string? last = null;
        string? line;
        while ((line = Console.ReadLine()) != null)
        {
            line = line.Trim().ToLower();
            if (line is "" or "quit") return;
            if (line == last) continue;
            SwitchTo(line);
            last = line;
        }
    }
}
