namespace VehicleGadgetsPlus
{
    using System.IO;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using CitizenFX.Core;
    using CitizenFX.Core.Native;

    using VehicleGadgetsPlus.Memory;
    using VehicleGadgetsPlus.Conditions.XML;
    using VehicleGadgetsPlus.VehicleGadgets;
    using VehicleGadgetsPlus.VehicleGadgets.XML;

    // Not marked unsafe at class level — async methods cannot be in an unsafe context.
    // Pointer work is isolated in the static unsafe helpers below.
    internal class Plugin : BaseScript
    {
        public const string VehicleConfigsFolder = "Vehicle Gadgets+/";
        public const string SoundsFolder = VehicleConfigsFolder + "Sounds/";
        public const string ConditionsFolder = VehicleConfigsFolder + "Conditions/";

        private readonly HashSet<int> vehiclesChecked = new HashSet<int>();
        private readonly List<VehicleGadget> gadgets = new List<VehicleGadget>();
        private readonly HashSet<Vehicle> vehiclesRequiringPoseBounds = new HashSet<Vehicle>();

        public static Dictionary<Model, VehicleConfig> VehicleConfigsByModel = new Dictionary<Model, VehicleConfig>();

        public Plugin()
        {
            bool gameFnInit = GameFunctions.Init();
            if (!gameFnInit)
            {
                Debug.WriteLine($"[VehicleGadgets+] [ERROR] Failed to initialize {nameof(GameFunctions)}, gadgets will not function.");
                return;
            }

            Debug.WriteLine($"[VehicleGadgets+] Successful {nameof(GameFunctions)} init");
            LoadVehicleConfigs();
            Tick += OnTick;
        }

        private async Task OnTick()
        {
            Input.Update();

            Ped playerPed = Game.PlayerPed;
            Vehicle playerVeh = (playerPed != null && playerPed.Exists()) ? playerPed.CurrentVehicle : null;

            if (playerVeh != null && !vehiclesChecked.Contains(playerVeh.Handle))
            {
                CreateGadgetsForVehicle(playerVeh);
            }

            for (int i = gadgets.Count - 1; i >= 0; i--)
            {
                VehicleGadget g = gadgets[i];
                if (g.Vehicle != null && g.Vehicle.Exists())
                {
                    g.Update(g.Vehicle == playerVeh);
                    if (g.RequiresPoseBounds)
                    {
                        vehiclesRequiringPoseBounds.Add(g.Vehicle);
                    }
                }
                else
                {
                    vehiclesChecked.Remove(g.Vehicle.Handle);
                    g.Dispose();
                    gadgets.RemoveAt(i);
                }
            }

            foreach (Vehicle v in vehiclesRequiringPoseBounds)
            {
                ApplyPoseBounds(v);
            }
            vehiclesRequiringPoseBounds.Clear();

            await Delay(0);
        }

        // Isolated here so the async OnTick above is never in an unsafe context.
        private static unsafe void ApplyPoseBounds(Vehicle v)
        {
            CVehicle* cveh = (CVehicle*)v.MemoryAddress;
            fragInstGta* inst = cveh->Inst;
            if (inst == null)
                return;

            GameFunctions.fragInst_PoseBoundsFromSkeleton(inst, true, true, true, 0, 0);
        }

        private void CreateGadgetsForVehicle(Vehicle vehicle)
        {
            VehicleGadget[] g = VehicleGadget.GetGadgetsForVehicle(vehicle);
            if (g != null)
            {
                gadgets.AddRange(g);
            }
            vehiclesChecked.Add(vehicle.Handle);
        }

        private static void LoadVehicleConfigs()
        {
            if (!Directory.Exists(VehicleConfigsFolder))
                Directory.CreateDirectory(VehicleConfigsFolder);

            Dictionary<Model, ConditionEntry[]> extraConditions = null;
            foreach (string fileName in Directory.EnumerateFiles(VehicleConfigsFolder, "*.xml", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    string modelName = Path.GetFileNameWithoutExtension(fileName);
                    Debug.WriteLine($"[VehicleGadgets+] Loading config for {modelName}...");
                    VehicleConfig cfg = Util.Deserialize<VehicleConfig>(fileName);
                    Model m = new Model(modelName);
                    if (cfg.ExtraConditions != null && cfg.ExtraConditions.Length > 0)
                    {
                        if (extraConditions == null)
                            extraConditions = new Dictionary<Model, ConditionEntry[]>();
                        extraConditions.Add(m, cfg.ExtraConditions);
                    }
                    VehicleConfigsByModel.Add(m, cfg);
                    Debug.WriteLine($"[VehicleGadgets+] Loaded config for {modelName}");
                }
                catch (System.InvalidOperationException ex)
                {
                    Debug.WriteLine($"[VehicleGadgets+] Can't load {Path.GetFileName(fileName)}: {ex}");
                }
                catch (System.Xml.XmlException ex)
                {
                    Debug.WriteLine($"[VehicleGadgets+] Can't load {Path.GetFileName(fileName)}: {ex}");
                }
            }

            Conditions.Conditions.LoadConditions(extraConditions);
        }
    }
}
