namespace VehicleGadgetsPlus.VehicleGadgets
{
    using System;
    using System.Numerics;

    using CitizenFX.Core;
    using CitizenFX.Core.Native;

    using Vector3 = System.Numerics.Vector3;
    using Quaternion = System.Numerics.Quaternion;

    using VehicleGadgetsPlus.VehicleGadgets.XML;
    using VehicleGadgetsPlus.Memory;

    internal sealed class Ladder : VehicleGadget
    {
        private readonly LadderEntry ladderDataEntry;
        private readonly VehicleBone ladderBase,
                                     ladderMain,
                                     ladderBucket;
        private readonly Extension[] ladderExtensions;

        public override bool RequiresPoseBounds => true;
        public bool HasBase => ladderBase != null;
        public bool HasMain => ladderMain != null;
        public bool HasExtensions => ladderExtensions != null;
        public bool HasBucket => ladderBucket != null;

        private readonly SoundEffect sound;
        private bool shouldPlaySound;

        public Ladder(Vehicle vehicle, VehicleGadgetEntry dataEntry) : base(vehicle, dataEntry)
        {
            ladderDataEntry = (LadderEntry)dataEntry;

            if (ladderDataEntry.HasBase)
            {
                if (!VehicleBone.TryGetForVehicle(vehicle, ladderDataEntry.Base.BoneName, out ladderBase))
                {
                    throw new InvalidOperationException($"The model \"{vehicle.Model.Hash}\" doesn't have the bone \"{ladderDataEntry.Base.BoneName}\" for the Ladder Base");
                }
            }

            if (ladderDataEntry.HasMain)
            {
                if (!VehicleBone.TryGetForVehicle(vehicle, ladderDataEntry.Main.BoneName, out ladderMain))
                {
                    throw new InvalidOperationException($"The model \"{vehicle.Model.Hash}\" doesn't have the bone \"{ladderDataEntry.Main.BoneName}\" for the Ladder Main");
                }
            }

            if (ladderDataEntry.HasExtensions)
            {
                ladderExtensions = new Extension[ladderDataEntry.Extensions.Parts.Length];

                for (int i = 0; i < ladderDataEntry.Extensions.Parts.Length; i++)
                {
                    if (!VehicleBone.TryGetForVehicle(vehicle, ladderDataEntry.Extensions.Parts[i].BoneName, out VehicleBone extensionBone))
                    {
                        throw new InvalidOperationException($"The model \"{vehicle.Model.Hash}\" doesn't have the bone \"{ladderDataEntry.Extensions.Parts[i].BoneName}\" for the Ladder Extension #{i}");
                    }

                    ladderExtensions[i] = new Extension(this, ladderDataEntry.Extensions.Parts[i], ladderDataEntry.Extensions.Direction, extensionBone);
                }
            }

            if (ladderDataEntry.HasBucket)
            {
                if (!VehicleBone.TryGetForVehicle(vehicle, ladderDataEntry.Bucket.BoneName, out ladderBucket))
                {
                    throw new InvalidOperationException($"The model \"{vehicle.Model.Hash}\" doesn't have the bone \"{ladderDataEntry.Bucket.BoneName}\" for the Ladder Bucket");
                }
            }

            if (ladderDataEntry.HasSoundsSet)
            {
                Sound begin = null, loop = null, end = null;

                begin = ladderDataEntry.SoundsSet.HasBegin ?
                            ladderDataEntry.SoundsSet.IsDefaultBegin ?
                                null :
                                new Sound(false, ladderDataEntry.SoundsSet.NormalizedVolume, () => ladderDataEntry.SoundsSet.BeginSoundFilePath) :
                            null;

                loop = ladderDataEntry.SoundsSet.HasLoop ?
                            ladderDataEntry.SoundsSet.IsDefaultLoop ?
                                new Sound(true, ladderDataEntry.SoundsSet.NormalizedVolume, () => (System.IO.Stream)Properties.Resources.default_ladder_loop) :
                                new Sound(true, ladderDataEntry.SoundsSet.NormalizedVolume, () => ladderDataEntry.SoundsSet.LoopSoundFilePath) :
                            null;

                end = ladderDataEntry.SoundsSet.HasEnd ?
                            ladderDataEntry.SoundsSet.IsDefaultEnd ?
                                new Sound(false, ladderDataEntry.SoundsSet.NormalizedVolume, () => (System.IO.Stream)Properties.Resources.default_ladder_end) :
                                new Sound(false, ladderDataEntry.SoundsSet.NormalizedVolume, () => ladderDataEntry.SoundsSet.EndSoundFilePath) :
                            null;

                sound = new SoundEffect(begin, loop, end);
            }
        }

        protected override void Dispose(bool disposing)
        {
            sound?.Dispose();
            base.Dispose(disposing);
        }

        public override unsafe void Update(bool isPlayerIn)
        {
            if (!isPlayerIn)
                return;

            shouldPlaySound = false;

            // left/right
            if (HasBase)
            {
                if (Input.IsKeyDownRightNow(Input.NumPad6))
                    RotateBaseRight();
                else if (Input.IsKeyDownRightNow(Input.NumPad4))
                    RotateBaseLeft();
            }

            // up/down
            if (HasMain)
            {
                if (Input.IsKeyDownRightNow(Input.NumPad8))
                    RotateMainUp();
                else if (Input.IsKeyDownRightNow(Input.NumPad2))
                    RotateMainDown();
            }

            // extend
            if (HasExtensions)
            {
                if (Input.IsKeyDownRightNow(Input.NumPad9))
                    ExtendLadder();
                else if (Input.IsKeyDownRightNow(Input.NumPad3))
                    RetractLadder();
            }

            // bucket up/down
            if (HasBucket)
            {
                if (Input.IsKeyDownRightNow(Input.NumPad7))
                    RotateBucketUp();
                else if (Input.IsKeyDownRightNow(Input.NumPad1))
                    RotateBucketDown();
            }

            if (sound != null)
            {
                if (shouldPlaySound)
                    sound.Play();
                else
                    sound.Stop();

                sound.Update();
            }
        }

        private void RotateBaseLeft()
        {
            if (!HasBase) return;
            Vector3 axis = ladderDataEntry.Base.RotationAxis;
            float degrees = ladderDataEntry.Base.RotationSpeed * API.GetFrameTime();
            ladderBase.RotateAxis(axis, degrees);
            shouldPlaySound = true;
        }

        private void RotateBaseRight()
        {
            if (!HasBase) return;
            Vector3 axis = ladderDataEntry.Base.RotationAxis;
            float degrees = -ladderDataEntry.Base.RotationSpeed * API.GetFrameTime();
            ladderBase.RotateAxis(axis, degrees);
            shouldPlaySound = true;
        }

        private void RotateMainUp()
        {
            if (!HasMain) return;
            Matrix4x4 m = ladderMain.Matrix;
            float angle = GetAngle(m, ladderMain.OriginalRotation, ladderDataEntry.Main.RotationAxis);
            if (angle < ladderDataEntry.Main.MaxAngle)
            {
                Vector3 axis = ladderDataEntry.Main.RotationAxis;
                float degrees = ladderDataEntry.Main.RotationSpeed * API.GetFrameTime();
                ladderMain.RotateAxis(axis, degrees);
                shouldPlaySound = true;
            }
        }

        private void RotateMainDown()
        {
            if (!HasMain) return;
            Matrix4x4 m = ladderMain.Matrix;
            float angle = GetAngle(m, ladderMain.OriginalRotation, ladderDataEntry.Main.RotationAxis);
            if (angle > ladderDataEntry.Main.MinAngle)
            {
                Vector3 axis = ladderDataEntry.Main.RotationAxis;
                float degrees = -ladderDataEntry.Main.RotationSpeed * API.GetFrameTime();
                ladderMain.RotateAxis(axis, degrees);
                shouldPlaySound = true;
            }
        }

        private void ExtendLadder()
        {
            if (!HasExtensions) return;
            for (int i = 0; i < ladderExtensions.Length; i++)
            {
                if (ladderExtensions[i].Extend())
                    shouldPlaySound = true;
            }
        }

        private void RetractLadder()
        {
            if (!HasExtensions) return;
            for (int i = 0; i < ladderExtensions.Length; i++)
            {
                if (ladderExtensions[i].Retract())
                    shouldPlaySound = true;
            }
        }

        private void RotateBucketUp()
        {
            if (!HasBucket) return;
            Matrix4x4 m = ladderBucket.Matrix;
            float angle = GetAngle(m, ladderBucket.OriginalRotation, ladderDataEntry.Bucket.RotationAxis);
            if (angle < ladderDataEntry.Bucket.MaxAngle)
            {
                Vector3 axis = ladderDataEntry.Bucket.RotationAxis;
                float degrees = ladderDataEntry.Bucket.RotationSpeed * API.GetFrameTime();
                ladderBucket.RotateAxis(axis, degrees);
                shouldPlaySound = true;
            }
        }

        private void RotateBucketDown()
        {
            if (!HasBucket) return;
            Matrix4x4 m = ladderBucket.Matrix;
            float angle = GetAngle(m, ladderBucket.OriginalRotation, ladderDataEntry.Bucket.RotationAxis);
            if (angle > ladderDataEntry.Bucket.MinAngle)
            {
                Vector3 axis = ladderDataEntry.Bucket.RotationAxis;
                float degrees = -ladderDataEntry.Bucket.RotationSpeed * API.GetFrameTime();
                ladderBucket.RotateAxis(axis, degrees);
                shouldPlaySound = true;
            }
        }

        private float GetAngle(Matrix4x4 matrix, Quaternion originalRotation, Vector3 axis)
        {
            Matrix4x4.Decompose(matrix, out _, out Quaternion rotation, out _);

            // Extract the imaginary (XYZ) part of each quaternion as direction vectors
            Vector3 rotationVector = new Vector3(rotation.X, rotation.Y, rotation.Z);
            Vector3 origRotationVector = new Vector3(originalRotation.X, originalRotation.Y, originalRotation.Z);
            Vector3 planeNormal = Vector3.Cross(axis, origRotationVector);

            float angle = (float)(Math.Asin(Vector3.Dot(planeNormal, rotationVector) /
                (planeNormal.Length() * rotationVector.Length()))
                * (180.0 / Math.PI));

            return angle;
        }


        private class Extension
        {
            private readonly LadderEntry.LadderExtension extensionData;
            private readonly Ladder owner;
            private readonly VehicleBone bone;
            private readonly Vector3 direction;

            public VehicleBone Bone => bone;
            public float MaxDistanceSqr { get; }

            public float CurrentDistanceSqr
            {
                get
                {
                    Matrix4x4 matrix = bone.Matrix;
                    Vector3 translation = MatrixUtils.DecomposeTranslation(matrix);
                    return Vector3.DistanceSquared(translation, bone.OriginalTranslation);
                }
            }

            public Extension(Ladder owner, LadderEntry.LadderExtension data, Vector3 direction, VehicleBone bone)
            {
                this.owner = owner;
                extensionData = data;
                this.direction = direction;
                this.bone = bone;
                MaxDistanceSqr = extensionData.ExtensionDistance * extensionData.ExtensionDistance;
            }

            public bool Extend()
            {
                if (CurrentDistanceSqr >= MaxDistanceSqr)
                    return false;

                float moveDist = extensionData.MoveSpeed * API.GetFrameTime();
                Vector3 translation = direction * moveDist;

                Matrix4x4 newMatrix = Matrix4x4.CreateScale(1.0f, 1.0f, 1.0f)
                    * Matrix4x4.CreateTranslation(translation)
                    * bone.Matrix;
                float newDistanceSqr = Vector3.DistanceSquared(MatrixUtils.DecomposeTranslation(newMatrix), bone.OriginalTranslation);

                if (newDistanceSqr > MaxDistanceSqr)
                    return false;

                bone.Translate(translation);
                return true;
            }

            public bool Retract()
            {
                float currentDistanceSqr = CurrentDistanceSqr;
                if (currentDistanceSqr < 0.015f * 0.015f)
                    return false;

                float moveDist = extensionData.MoveSpeed * API.GetFrameTime();
                Vector3 translation = -direction * moveDist;

                Matrix4x4 newMatrix = Matrix4x4.CreateScale(1.0f, 1.0f, 1.0f)
                    * Matrix4x4.CreateTranslation(translation)
                    * bone.Matrix;
                float newDistanceSqr = Vector3.DistanceSquared(MatrixUtils.DecomposeTranslation(newMatrix), bone.OriginalTranslation);

                if (newDistanceSqr > currentDistanceSqr)
                    return false;

                bone.Translate(translation);
                return true;
            }
        }
    }
}
