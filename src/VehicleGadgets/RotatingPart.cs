namespace VehicleGadgetsPlus.VehicleGadgets
{
    using System;
    using System.Numerics;

    using CitizenFX.Core;
    using CitizenFX.Core.Native;

    using Vector3 = System.Numerics.Vector3;
    using Quaternion = System.Numerics.Quaternion;

    using VehicleGadgetsPlus.Conditions;
    using VehicleGadgetsPlus.VehicleGadgets.XML;

    internal sealed class RotatingPart : VehicleGadget
    {
        private readonly RotatingPartEntry rotatingPartDataEntry;
        private readonly ConditionDelegate[] conditions;
        private readonly VehicleBone bone;
        private bool rotating;
        private readonly bool hasRange;
        private readonly Quaternion rangeMin, rangeMax;
        private bool rangeIncreasing;
        private float rangePercentage;

        public override bool RequiresPoseBounds => true;

        public RotatingPart(Vehicle vehicle, VehicleGadgetEntry dataEntry) : base(vehicle, dataEntry)
        {
            rotatingPartDataEntry = (RotatingPartEntry)dataEntry;

            if (!VehicleBone.TryGetForVehicle(vehicle, rotatingPartDataEntry.BoneName, out bone))
            {
                throw new InvalidOperationException($"The model \"{vehicle.Model.Hash}\" doesn't have the bone \"{rotatingPartDataEntry.BoneName}\" for the {RotatingPartEntry.XmlName}");
            }

            conditions = Conditions.GetConditionsFromString(vehicle.Model, rotatingPartDataEntry.Conditions);

            if (rotatingPartDataEntry.HasRange)
            {
                hasRange = true;

                float toRad = (float)Math.PI / 180f;
                Quaternion min = Quaternion.CreateFromAxisAngle(rotatingPartDataEntry.RotationAxis, rotatingPartDataEntry.Range.Min * toRad);
                Quaternion max = Quaternion.CreateFromAxisAngle(rotatingPartDataEntry.RotationAxis, rotatingPartDataEntry.Range.Max * toRad);

                rangeMin = bone.OriginalRotation * min;
                rangeMax = bone.OriginalRotation * max;
            }
        }

        public override void Update(bool isPlayerIn)
        {
            if (bone != null)
            {
                if (rotatingPartDataEntry.IsToggle)
                {
                    bool? value = CheckConditions(isPlayerIn);
                    if (value.HasValue && value.Value)
                    {
                        rotating = !rotating;
                    }
                }
                else
                {
                    bool? value = CheckConditions(isPlayerIn);
                    if (value.HasValue)
                    {
                        rotating = value.Value;
                    }
                }
            }

            if (rotating)
            {
                if (hasRange)
                {
                    rangePercentage += rotatingPartDataEntry.RotationSpeed * API.GetFrameTime() * (rangeIncreasing ? 1.0f : -1.0f);
                    Quaternion newRotation = QuaternionUtils.Slerp(rangeMin, rangeMax, rangePercentage, rotatingPartDataEntry.Range.LongestPath);

                    bone.SetRotation(newRotation);

                    if ((rangeIncreasing && rangePercentage >= 1.0f) ||
                        (!rangeIncreasing && rangePercentage <= 0.0f))
                    {
                        rangeIncreasing = !rangeIncreasing;
                    }
                }
                else
                {
                    Vector3 axis = rotatingPartDataEntry.RotationAxis;
                    float degrees = rotatingPartDataEntry.RotationSpeed * API.GetFrameTime();
                    bone.RotateAxis(axis, degrees);
                }
            }
        }

        private bool? CheckConditions(bool isPlayerIn)
        {
            if (conditions.Length <= 0)
                return null;

            for (int i = 0; i < conditions.Length; i++)
            {
                bool? v = conditions[i].Invoke(Vehicle, isPlayerIn);
                if (!v.HasValue) return null;
                if (!v.Value) return false;
            }

            return true;
        }
    }
}
