namespace VehicleGadgetsPlus
{
    using System;
    using System.IO;
    using System.Numerics;
    using System.Xml.Serialization;

    using CitizenFX.Core;

    using Vector3 = System.Numerics.Vector3;
    using Quaternion = System.Numerics.Quaternion;

    using VehicleGadgetsPlus.Memory;

    internal static unsafe class Util
    {
        // Unlike the native/RPH method, this one works with bones that have custom names
        // (e.g. "ladder_base") that the native GET_ENTITY_BONE_INDEX_BY_NAME can't find.
        public static int GetBoneIndex(Vehicle vehicle, string boneName)
        {
            if (vehicle == null || !vehicle.Exists())
                throw new ArgumentException("Vehicle is null or does not exist", nameof(vehicle));

            CVehicle* veh = (CVehicle*)vehicle.MemoryAddress;
            crSkeletonData* skelData = veh->Inst->CacheEntry->Skeleton->Data;
            uint boneCount = skelData->NumBones;

            for (uint i = 0; i < boneCount; i++)
            {
                if (skelData->GetBoneNameForIndex(i) == boneName)
                    return unchecked((int)i);
            }

            return -1;
        }

        public static Vector3 GetBoneOriginalTranslation(Vehicle vehicle, int index)
        {
            CVehicle* veh = (CVehicle*)vehicle.MemoryAddress;
            NativeVector3 v = veh->Inst->CacheEntry->Skeleton->Data->Bones[index].Translation;
            return v;
        }

        public static Quaternion GetBoneOriginalRotation(Vehicle vehicle, int index)
        {
            CVehicle* veh = (CVehicle*)vehicle.MemoryAddress;
            NativeVector4 v = veh->Inst->CacheEntry->Skeleton->Data->Bones[index].Rotation;
            return v;
        }

        public static void Serialize<T>(string fileName, T data)
        {
            using (StreamWriter w = new StreamWriter(fileName, false))
            {
                XmlSerializer ser = new XmlSerializer(typeof(T));
                ser.Serialize(w, data);
            }
        }

        public static T Deserialize<T>(string fileName)
        {
            using (StreamReader r = new StreamReader(fileName))
            {
                XmlSerializer ser = new XmlSerializer(typeof(T));
                return (T)ser.Deserialize(r);
            }
        }
    }

    internal static class MatrixUtils
    {
        public static bool Decompose(Matrix4x4 matrix, out Vector3 scale, out Quaternion rotation, out Vector3 translation)
        {
            return Matrix4x4.Decompose(matrix, out scale, out rotation, out translation);
        }

        public static Vector3 DecomposeScale(Matrix4x4 matrix)
        {
            Matrix4x4.Decompose(matrix, out Vector3 scale, out _, out _);
            return scale;
        }

        public static Quaternion DecomposeRotation(Matrix4x4 matrix)
        {
            Matrix4x4.Decompose(matrix, out _, out Quaternion rotation, out _);
            return rotation;
        }

        public static Vector3 DecomposeTranslation(Matrix4x4 matrix)
        {
            Matrix4x4.Decompose(matrix, out _, out _, out Vector3 translation);
            return translation;
        }
    }

    internal static class QuaternionUtils
    {
        // based on MyQuaternion.SlerpUnclamped from https://gist.github.com/aeroson/043001ca12fe29ee911e
        public static Quaternion Slerp(Quaternion a, Quaternion b, float t, bool longestPath)
        {
            return Slerp(ref a, ref b, t, longestPath);
        }

        public static Quaternion Slerp(ref Quaternion a, ref Quaternion b, float t, bool longestPath)
        {
            if (a.LengthSquared() == 0.0f)
            {
                if (b.LengthSquared() == 0.0f)
                    return Quaternion.Identity;
                return b;
            }
            else if (b.LengthSquared() == 0.0f)
            {
                return a;
            }

            float cosHalfAngle = a.W * b.W + Vector3.Dot(new Vector3(a.X, a.Y, a.Z), new Vector3(b.X, b.Y, b.Z));

            if (cosHalfAngle >= 1.0f || cosHalfAngle <= -1.0f)
                return a;

            if (longestPath || (!longestPath && cosHalfAngle < 0.0f))
            {
                b.X = -b.X; b.Y = -b.Y; b.Z = -b.Z; b.W = -b.W;
                cosHalfAngle = -cosHalfAngle;
            }

            float blendA, blendB;
            if (cosHalfAngle < 0.99f)
            {
                float halfAngle = (float)Math.Acos(cosHalfAngle);
                float sinHalfAngle = (float)Math.Sin(halfAngle);
                float oneOverSinHalfAngle = 1.0f / sinHalfAngle;
                blendA = (float)Math.Sin(halfAngle * (1.0f - t)) * oneOverSinHalfAngle;
                blendB = (float)Math.Sin(halfAngle * t) * oneOverSinHalfAngle;
            }
            else
            {
                blendA = 1.0f - t;
                blendB = t;
            }

            Quaternion result = new Quaternion(
                blendA * new Vector3(a.X, a.Y, a.Z) + blendB * new Vector3(b.X, b.Y, b.Z),
                blendA * a.W + blendB * b.W);

            if (result.LengthSquared() > 0.0f)
                return Quaternion.Normalize(result);
            else
                return Quaternion.Identity;
        }
    }
}
