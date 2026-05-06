using CitizenFX.Core.Native;

namespace VehicleGadgetsPlus.Memory
{
    using System;
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
            return CitizenFX.Core.Native.API.FindPattern(pattern);
        }
    }
}
