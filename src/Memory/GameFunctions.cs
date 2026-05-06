namespace VehicleGadgetsPlus.Memory
{
    using System.Runtime.InteropServices;

    internal static unsafe class GameFunctions
    {
        public delegate int fragInst_GetBoundIndexForBone_Delegate(fragInstGta* inst, int boneIndex);
        public delegate void fragInst_PoseBoundsFromSkeleton_Delegate(fragInstGta* inst, bool a2, bool a3, bool a4, sbyte a5, long a6);

        public static fragInst_GetBoundIndexForBone_Delegate fragInst_GetBoundIndexForBone { get; private set; }
        public static fragInst_PoseBoundsFromSkeleton_Delegate fragInst_PoseBoundsFromSkeleton { get; private set; }

        // Returns true only if both functions were found. Either way, delegates that
        // were found are still set so partial functionality works.
        internal static bool Init() => false;
    }
}
