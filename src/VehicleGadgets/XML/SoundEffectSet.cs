namespace VehicleGadgetsPlus.VehicleGadgets.XML
{
    using System;
    using System.IO;
    using System.Xml.Serialization;

    public sealed class SoundEffectSet
    {
        public static readonly string Default = "default";

        // 0 - 100
        public int Volume { get; set; }
        // "default" or filename with extension from "Vehicle Gadgets+\Sounds\"
        [XmlElement(IsNullable = true)] public string Begin { get; set; }
        [XmlElement(IsNullable = true)] public string Loop { get; set; }
        [XmlElement(IsNullable = true)] public string End { get; set; }

        [XmlIgnore] public float NormalizedVolume => Math.Max(0, Math.Min(100, Volume)) / 100.0f;

        [XmlIgnore] public bool HasBegin => Begin != null;
        [XmlIgnore] public bool HasLoop => Loop != null;
        [XmlIgnore] public bool HasEnd => End != null;

        [XmlIgnore] public bool IsDefaultBegin => HasBegin && Begin.Equals(Default, StringComparison.InvariantCultureIgnoreCase);
        [XmlIgnore] public bool IsDefaultLoop => HasLoop && Loop.Equals(Default, StringComparison.InvariantCultureIgnoreCase);
        [XmlIgnore] public bool IsDefaultEnd => HasEnd && End.Equals(Default, StringComparison.InvariantCultureIgnoreCase);

        [XmlIgnore] public string BeginSoundFilePath => IsDefaultBegin ? null : Path.Combine(Plugin.SoundsFolder, Begin);
        [XmlIgnore] public string LoopSoundFilePath => IsDefaultLoop ? null : Path.Combine(Plugin.SoundsFolder, Loop);
        [XmlIgnore] public string EndSoundFilePath => IsDefaultEnd ? null : Path.Combine(Plugin.SoundsFolder, End);
    }
}
