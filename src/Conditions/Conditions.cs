namespace VehicleGadgetsPlus.Conditions
{
    using System.Collections.Generic;

    using CitizenFX.Core;
    using CitizenFX.Core.Native;

    using VehicleGadgetsPlus.Conditions.XML;

    public delegate bool? ConditionDelegate(Vehicle vehicle, bool isPlayerInsideVehicle);

    internal static class Conditions
    {
        // Built-in conditions. ExtraConditions from vehicle XMLs are silently skipped
        // because CSharpCodeProvider (runtime C# compilation) is not available in FiveM.
        private static readonly Dictionary<string, ConditionDelegate> conditionsByName = new Dictionary<string, ConditionDelegate>
        {
            ["EngineOn"]  = (v, _) => (bool?)API.GetIsVehicleEngineRunning(v.Handle),
            ["EngineOff"] = (v, _) => !API.GetIsVehicleEngineRunning(v.Handle),
            ["SirenOn"]   = (v, _) => (bool?)API.IsVehicleSirenOn(v.Handle),
            ["SirenOff"]  = (v, _) => !API.IsVehicleSirenOn(v.Handle),
        };

        public static void LoadConditions(Dictionary<Model, ConditionEntry[]> extraConditionsByModel = null)
        {
            if (extraConditionsByModel != null && extraConditionsByModel.Count > 0)
            {
                Debug.WriteLine("[VehicleGadgets+] ExtraConditions in XML are not supported in the FiveM port and will be skipped.");
            }
        }

        public static ConditionDelegate[] GetConditionsFromString(Model model, string conditions)
        {
            if (string.IsNullOrWhiteSpace(conditions))
                return new ConditionDelegate[0];

            string[] parts = conditions.Replace(" ", "").Split(',');
            var result = new List<ConditionDelegate>();
            foreach (string name in parts)
            {
                if (conditionsByName.TryGetValue(name, out ConditionDelegate del))
                    result.Add(del);
                else
                    Debug.WriteLine($"[VehicleGadgets+] The condition '{name}' doesn't exist.");
            }
            return result.ToArray();
        }
    }
}
