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

    internal class Plugin : BaseScript
    {
        public const string VehicleConfigsFolder = "Gadgets/";
        public const string SoundsFolder = VehicleConfigsFolder + "Sounds/";
        public const string ConditionsFolder = VehicleConfigsFolder + "Conditions/";

        private readonly HashSet<int> vehiclesChecked = new HashSet<int>();
        private readonly List<VehicleGadget> gadgets = new List<VehicleGadget>();
        private readonly HashSet<Vehicle> vehiclesRequiringPoseBounds = new HashSet<Vehicle>();

        public static Dictionary<Model, VehicleConfig> VehicleConfigsByModel = new Dictionary<Model, VehicleConfig>();

        public Plugin()
        {
            // Pattern scan for native functions — non-fatal, we keep going either way.
            // Check F8 console to see which functions were (or weren't) found.
            GameFunctions.Init();

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
                    try
                    {
                        g.Update(g.Vehicle == playerVeh);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.WriteLine($"[VehicleGadgets+] Error updating gadget {g.GetType().Name}: {ex.Message}");
                    }

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

            if (GameFunctions.fragInst_PoseBoundsFromSkeleton != null)
            {
                foreach (Vehicle v in vehiclesRequiringPoseBounds)
                {
                    ApplyPoseBounds(v);
                }
            }
            vehiclesRequiringPoseBounds.Clear();

            await Delay(0);
        }

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
            try
            {
                VehicleGadget[] g = VehicleGadget.GetGadgetsForVehicle(vehicle);
                if (g != null)
                {
                    gadgets.AddRange(g);
                    Debug.WriteLine($"[VehicleGadgets+] Created {g.Length} gadget(s) for model 0x{vehicle.Model.Hash:X}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"[VehicleGadgets+] Error creating gadgets for model 0x{vehicle.Model.Hash:X}: {ex.Message}");
            }

            vehiclesChecked.Add(vehicle.Handle);
        }

        private static void LoadVehicleConfigs()
        {
            if (!Directory.Exists(VehicleConfigsFolder))
            {
                Debug.WriteLine($"[VehicleGadgets+] Config folder '{VehicleConfigsFolder}' not found — creating it.");
                Directory.CreateDirectory(VehicleConfigsFolder);
                return;
            }

            int loaded = 0;
            foreach (string fileName in Directory.EnumerateFiles(VehicleConfigsFolder, "*.xml", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    string modelName = Path.GetFileNameWithoutExtension(fileName);
                    VehicleConfig cfg = Util.Deserialize<VehicleConfig>(fileName);
                    Model m = new Model(modelName);

                    if (cfg.ExtraConditions != null && cfg.ExtraConditions.Length > 0)
                    {
                        Debug.WriteLine($"[VehicleGadgets+] Note: ExtraConditions in {Path.GetFileName(fileName)} are not supported in FiveM and will be skipped.");
                    }

                    VehicleConfigsByModel.Add(m, cfg);
                    Debug.WriteLine($"[VehicleGadgets+] Loaded config for '{modelName}' (hash 0x{m.Hash:X})");
                    loaded++;
                }
                catch (System.Exception ex)
                {
                    Debug.WriteLine($"[VehicleGadgets+] Can't load {Path.GetFileName(fileName)}: {ex.Message}");
                }
            }

            Debug.WriteLine($"[VehicleGadgets+] Loaded {loaded} vehicle config(s).");
            Conditions.Conditions.LoadConditions(null);
        }
    }
}
