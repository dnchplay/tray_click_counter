using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

class Program
{
    private const int WH_MOUSE_LL = 14;
    private const int WM_LBUTTONDOWN = 0x0201;

    private static NotifyIcon notifyIcon;
    private static ulong clicks = 0;

    private static IntPtr hookId = IntPtr.Zero;
    private static LowLevelMouseProc mouseProc = HookCallback;

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        hookId = SetHook(mouseProc);

        notifyIcon = new NotifyIcon()
        {
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath),
            Visible = true,
            Text = $"Clicks: {clicks}"
        };

        notifyIcon.MouseClick += NotifyIcon_MouseClick;

        Application.Run();

        UnhookWindowsHookEx(hookId);
    }

    private static void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            notifyIcon.Visible = false;
            Application.Exit();
        }
    }

    private static IntPtr SetHook(LowLevelMouseProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_LBUTTONDOWN)
        {
            clicks++;
            notifyIcon.Text = $"Clicks: {clicks}";
        }

        return CallNextHookEx(hookId, nCode, wParam, lParam);
    }

    #region Native Methods

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    #endregion
}
