namespace SuperNetNanny
{
    using System;
    using System.Runtime.InteropServices;

    using FMOD;

    static class AudioManager
    {
        private static System s_FMODSystem;

        private static Channel s_Channel;

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        public static void Init()
        {
            if (Environment.Is64BitProcess)
            {
                Console.WriteLine("Loading 64bit FMOD Library");
                LoadLibrary("FMOD\\64\\fmod.dll");
            }
            else
            {
                Console.WriteLine("Loading 32bit FMOD Library");
                LoadLibrary("FMOD\\32\\fmod.dll");
            }

            Console.WriteLine("Creating System... Result: " + Factory.System_Create(out s_FMODSystem));

            Console.WriteLine("Setting DSP Buffer Size... Result: " + s_FMODSystem.setDSPBufferSize(1024, 4));
            Console.WriteLine(
                "Initializing System... Result: " + s_FMODSystem.init(32, INITFLAGS.NORMAL, (IntPtr)0));

        }
    }
}
