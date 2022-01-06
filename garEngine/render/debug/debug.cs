using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace garEngine.render.debug
{
    internal static class GLDebug
    {
        private static DebugProc dp = Debug;

        public static void Init()
        {
            Console.WriteLine("DEBUG MODE");
            GL.DebugMessageCallback(dp, IntPtr.Zero);
            GL.Enable(EnableCap.DebugOutput);
        }

        public static void Debug(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            Console.WriteLine("Debug: " + Marshal.PtrToStringAnsi(message, length));  
        }

    }
}