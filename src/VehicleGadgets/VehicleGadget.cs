namespace VehicleGadgetsPlus.VehicleGadgets
{
    using System;

    using CitizenFX.Core;

    using VehicleGadgetsPlus.VehicleGadgets.XML;

    internal abstract class VehicleGadget : IDisposable
    {
        public Vehicle Vehicle { get; }
        public VehicleGadgetEntry DataEntry { get; }
        public bool IsDisposed { get; private set; }
        public virtual bool RequiresPoseBounds => false;

        protected VehicleGadget(Vehicle vehicle, VehicleGadgetEntry dataEntry)
        {
            Vehicle = vehicle;
            DataEntry = dataEntry;
        }

        ~VehicleGadget()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public abstract void Update(bool isPlayerIn);

        public static VehicleGadget[] GetGadgetsForVehicle(Vehicle vehicle)
        {
            if (!Plugin.VehicleConfigsByModel.TryGetValue(vehicle.Model, out VehicleConfig config))
                return null;

            if (config.Gadgets == null || config.Gadgets.Length == 0)
                return null;

            var list = new System.Collections.Generic.List<VehicleGadget>(config.Gadgets.Length);
            for (int i = 0; i < config.Gadgets.Length; i++)
            {
                VehicleGadgetEntry entry = config.Gadgets[i];
                try
                {
                    list.Add((VehicleGadget)Activator.CreateInstance(entry.GadgetType, vehicle, entry));
                }
                catch (System.Exception ex)
                {
                    CitizenFX.Core.Debug.WriteLine($"[VehicleGadgets+] Failed to create gadget {entry.GadgetType?.Name} for model 0x{vehicle.Model.Hash:X}: {ex.Message}");
                }
            }
            return list.Count > 0 ? list.ToArray() : null;
        }
    }
}
