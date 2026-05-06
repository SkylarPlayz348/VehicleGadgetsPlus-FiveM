namespace VehicleGadgetsPlus.Memory
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    internal static unsafe class GameFunctions
    {
        public delegate int fragInst_GetBoundIndexForBone_Delegate(fragInstGta* inst, int boneIndex);
        public delegate void fragInst_PoseBoundsFromSkeleton_Delegate(fragInstGta* inst, bool a2, bool a3, bool a4, sbyte a5, long a6);

        public static fragInst_GetBoundIndexForBone_Delegate fragInst_GetBoundIndexForBone { get; private set; }
        public static fragInst_PoseBoundsFromSkeleton_Delegate fragInst_PoseBoundsFromSkeleton { get; private set; }

        // Returns true only if both functions were found. Either way, delegates that
        // were found are still set so partial functionality works.
        internal static bool Init()
        {
            bool ok = true;

            IntPtr address = FindPattern("85 D2 78 44 4C 8B 49 68 4D 85 C9 74 29 49 8B 81");
            if (address != IntPtr.Zero)
            {
                fragInst_GetBoundIndexForBone = (fragInst_GetBoundIndexForBone_Delegate)Marshal.GetDelegateForFunctionPointer(address, typeof(fragInst_GetBoundIndexForBone_Delegate));
                CitizenFX.Core.Debug.WriteLine($"[VehicleGadgets+] Found {nameof(fragInst_GetBoundIndexForBone)} at 0x{address.ToInt64():X}");
            }
            else
            {
                CitizenFX.Core.Debug.WriteLine($"[VehicleGadgets+] WARNING: Could not find {nameof(fragInst_GetBoundIndexForBone)} — HideablePart bound hiding will be disabled.");
                ok = false;
            }

            address = FindPattern("48 8B C4 48 89 58 18 44 88 48 20 88 50 10 55 56 57");
            if (address != IntPtr.Zero)
            {
                fragInst_PoseBoundsFromSkeleton = (fragInst_PoseBoundsFromSkeleton_Delegate)Marshal.GetDelegateForFunctionPointer(address, typeof(fragInst_PoseBoundsFromSkeleton_Delegate));
                CitizenFX.Core.Debug.WriteLine($"[VehicleGadgets+] Found {nameof(fragInst_PoseBoundsFromSkeleton)} at 0x{address.ToInt64():X}");
            }
            else
            {
                CitizenFX.Core.Debug.WriteLine($"[VehicleGadgets+] WARNING: Could not find {nameof(fragInst_PoseBoundsFromSkeleton)} — collision bounds won't update after bone moves.");
                ok = false;
            }

            return ok;
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
