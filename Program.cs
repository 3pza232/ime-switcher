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

    static string Describe(IntPtr hkl)
    {
        var id = (int)(hkl.ToInt64() & 0xFFFF);
        try { var c = CultureInfo.GetCultureInfo(id); return $"{c.IetfLanguageTag} ({c.EnglishName})"; }
        catch { return $"0x{hkl:X8}"; }
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
            if (hkl == before) return false;

            PostMessage(GetForegroundWindow(), WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, hkl);
            Thread.Sleep(8);
            if (ActiveLayout() != before) return true;

            ActivateKeyboardLayout(hkl, 0);
            Thread.Sleep(8);
            return ActiveLayout() != before;
        }
        return false;
    }

    static void Main()
    {
        string? last = null;
        Console.Error.WriteLine("ime-switcher");

        string? line;
        while ((line = Console.ReadLine()) != null)
        {
            line = line.Trim().ToLower();
            if (line is "" or "quit") break;
            if (line == "status") { Console.Error.WriteLine(Describe(ActiveLayout())); continue; }
            if (line == last) { Console.Error.WriteLine("dup"); continue; }

            var switched = SwitchTo(line);
            if (switched)
            {
                last = line;
                Console.Error.WriteLine("ok");
            }
            else
            {
                var langId = LangId(line);
                var currentLangId = (ushort)(ActiveLayout().ToInt64() & 0xFFFF);
                if (langId == currentLangId)
                {
                    last = line;
                    Console.Error.WriteLine("dup");
                }
                else
                {
                    Console.Error.WriteLine("fail");
                }
            }
        }
    }
}
