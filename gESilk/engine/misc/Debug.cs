using System.Diagnostics;
using System.Runtime.Intrinsics.Arm;
using gESilk.engine;
using Silk.NET.OpenGL;
namespace garEngine.render.utility;
using static Globals;
using System.Runtime.InteropServices;


    internal static class GLDebug
    {
         //DebugProc dp = (source, type, id, severity, length, message, param) => 

        public static void Init()
        {
            Console.WriteLine("DEBUG MODE");
            var dp = DebugProc.CreateDelegate();
            gl.DebugMessageCallback(dp, IntPtr.Zero);
            gl.Enable(EnableCap.DebugOutput);
        }

        public static void Debug(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            Console.WriteLine("Debug: " + Marshal.PtrToStringAnsi(message, length));  
        }

    }
