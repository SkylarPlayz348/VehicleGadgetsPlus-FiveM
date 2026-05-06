namespace VehicleGadgetsPlus.Memory
{
    using System.Numerics;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeMatrix4x4
    {
        public float M11; public float M12; public float M13; public float M14;
        public float M21; public float M22; public float M23; public float M24;
        public float M31; public float M32; public float M33; public float M34;
        public float M41; public float M42; public float M43; public float M44;

        public NativeVector4 M1 => new NativeVector4(M11, M12, M13, M14);
        public NativeVector4 M2 => new NativeVector4(M21, M22, M23, M24);
        public NativeVector4 M3 => new NativeVector4(M31, M32, M33, M34);
        public NativeVector4 M4 => new NativeVector4(M41, M42, M43, M44);

        public static implicit operator NativeMatrix4x4(Matrix4x4 m) => new NativeMatrix4x4
        {
            M11 = m.M11, M12 = m.M12, M13 = m.M13, M14 = m.M14,
            M21 = m.M21, M22 = m.M22, M23 = m.M23, M24 = m.M24,
            M31 = m.M31, M32 = m.M32, M33 = m.M33, M34 = m.M34,
            M41 = m.M41, M42 = m.M42, M43 = m.M43, M44 = m.M44,
        };

        public static implicit operator Matrix4x4(NativeMatrix4x4 m) => new Matrix4x4(
            m.M11, m.M12, m.M13, m.M14,
            m.M21, m.M22, m.M23, m.M24,
            m.M31, m.M32, m.M33, m.M34,
            m.M41, m.M42, m.M43, m.M44);
    }
}
