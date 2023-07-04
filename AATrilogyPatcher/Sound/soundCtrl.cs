#if RELEASE_WINDOWS
using System;
using System.IO;
using System.Windows;
using NAudio.Extras;
using NAudio.Wave;

namespace AATrilogyPatcher.Sound
{
    // modificacion de https://github.com/naudio/NAudio/blob/master/NAudioWpfDemo/AudioPlaybackDemo/AudioPlayback.cs#L8
    class AudioPlayback : IDisposable
    {
        private IWavePlayer playbackDevice;
        private WaveStream fileStream;

        public event EventHandler<FftEventArgs> FftCalculated;

        public event EventHandler<MaxSampleEventArgs> MaximumCalculated;

        public void LoadStream(Stream stream)
        {
            Stop();
            CloseFile();
            EnsureDeviceCreated();
            OpenStream(stream);
        }

        private void CloseFile()
        {
            fileStream?.Dispose();
            fileStream = null;
        }

        private void OpenStream(Stream stream)
        {
            try
            {
                var inputStream = new Mp3FileReader(stream);
                fileStream = inputStream;
                var aggregator = new SampleAggregator(inputStream.ToSampleProvider());
                aggregator.NotificationCount = inputStream.WaveFormat.SampleRate / 100;
                aggregator.PerformFFT = true;
                aggregator.FftCalculated += (s, a) => FftCalculated?.Invoke(this, a);
                aggregator.MaximumCalculated += (s, a) => MaximumCalculated?.Invoke(this, a);
                playbackDevice.Init(aggregator);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Problem opening file");
                CloseFile();
            }
        }

        private void EnsureDeviceCreated()
        {
            if (playbackDevice == null)
            {
                CreateDevice();
            }
        }

        private void CreateDevice()
        {
            playbackDevice = new WaveOut { DesiredLatency = 200 };
        }

        public void Play()
        {
            if (playbackDevice != null && fileStream != null && playbackDevice.PlaybackState != PlaybackState.Playing)
            {
                playbackDevice.Play();
            }
        }

        public void Pause()
        {
            playbackDevice?.Pause();
        }

        public void Stop()
        {
            playbackDevice?.Stop();
            if (fileStream != null)
            {
                fileStream.Position = 0;
            }
        }

        public void Dispose()
        {
            Stop();
            CloseFile();
            playbackDevice?.Dispose();
            playbackDevice = null;
        }
    }

    class soundCtrl
    {
        public static AudioPlayback audioPlayback;

        public static void PlaySound(byte[] resource)
        {
            if (WaveOut.DeviceCount > -1)
            {
                audioPlayback = new AudioPlayback();

                var resourceStream = new MemoryStream(resource);

                audioPlayback.LoadStream(resourceStream);
                audioPlayback.Play();
            }
        }
    }
}
#endif
