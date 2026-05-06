namespace VehicleGadgetsPlus.Memory
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    using CitizenFX.Core;

    internal static unsafe class GameFunctions
    {
        public delegate int fragInst_GetBoundIndexForBone_Delegate(fragInstGta* inst, int boneIndex);
        public delegate void fragInst_PoseBoundsFromSkeleton_Delegate(fragInstGta* inst, bool a2, bool a3, bool a4, sbyte a5, long a6);

        public static fragInst_GetBoundIndexForBone_Delegate fragInst_GetBoundIndexForBone { get; private set; }
        public static fragInst_PoseBoundsFromSkeleton_Delegate fragInst_PoseBoundsFromSkeleton { get; private set; }

        internal static bool Init()
        {
            IntPtr address = FindPattern("85 D2 78 44 4C 8B 49 68 4D 85 C9 74 29 49 8B 81");
            if (AssertAddress(address, nameof(fragInst_GetBoundIndexForBone)))
            {
                fragInst_GetBoundIndexForBone = Marshal.GetDelegateForFunctionPointer<fragInst_GetBoundIndexForBone_Delegate>(address);
            }

            address = FindPattern("48 8B C4 48 89 58 18 44 88 48 20 88 50 10 55 56 57");
            if (AssertAddress(address, nameof(fragInst_PoseBoundsFromSkeleton)))
            {
                fragInst_PoseBoundsFromSkeleton = Marshal.GetDelegateForFunctionPointer<fragInst_PoseBoundsFromSkeleton_Delegate>(address);
            }

            return !anyAssertFailed;
        }

        private static bool anyAssertFailed = false;
        private static bool AssertAddress(IntPtr address, string name)
        {
            if (address == IntPtr.Zero)
            {
                CitizenFX.Core.Debug.WriteLine($"[VehicleGadgets+] Incompatible game version, couldn't find {name} function address.");
                anyAssertFailed = true;
                return false;
            }
            return true;
        }

        private static IntPtr FindPattern(string pattern)
        {
            string[] parts = pattern.Split(' ');
            byte[] bytes = new byte[parts.Length];
            bool[] wildcards = new bool[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "??" || parts[i] == "?")
                    wildcards[i] = true;
                else
                    bytes[i] = Convert.ToByte(parts[i], 16);
            }

            ProcessModule module = Process.GetCurrentProcess().MainModule;
            byte* moduleBase = (byte*)module.BaseAddress;
            int moduleSize = module.ModuleMemorySize;

            for (int i = 0; i < moduleSize - bytes.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < bytes.Length; j++)
                {
                    if (!wildcards[j] && moduleBase[i + j] != bytes[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                    return new IntPtr(moduleBase + i);
            }

            return IntPtr.Zero;
        }
    }
}
