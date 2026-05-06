namespace VehicleGadgetsPlus
{
    using System;
    using System.IO;

    // No-op stub — DirectSound (SlimDX) is not available in FiveM.
    // Sound objects are created normally so Ladder/Outriggers compile unchanged,
    // but nothing is actually played.
    internal class Sound : IDisposable
    {
        public string Id { get; } = Guid.NewGuid().ToString();
        public bool Looped { get; }
        public float Volume { get; }
        public bool IsPlaying => false;
        public bool IsDisposed { get; private set; }

        public Sound(bool looped, float volume, Func<string> waveFilePathGetter) { Looped = looped; Volume = volume; }
        public Sound(bool looped, float volume, Func<Stream> waveStreamGetter) { Looped = looped; Volume = volume; }

        public void Play() { }
        public void Stop() { }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    internal class SoundPlayer : IDisposable
    {
        private static readonly SoundPlayer instance = new SoundPlayer();
        public static SoundPlayer Instance => instance;
        public static bool IsInitialized => true;
        public bool IsDisposed => false;

        private SoundPlayer() { }

        public bool IsPlaying(string id) => false;
        public void Clean(string id) { }
        public void Play(string id, bool loop, float volume, Func<Stream> getWaveStream) { }
        public void Play(string id, bool loop, float volume, Func<string> getWaveFilePath) { }
        public void Stop(string id) { }
        public void Dispose() { }
    }
}
