using System;
using System.Collections.ObjectModel;
using CSCore.CoreAudioAPI;
using CSCore.SoundIn;

namespace Syne.IO.Audio
{
    public class DeviceEnumerator
    {
//        private MMDeviceCollection _mmDeviceCollection;
        public DeviceEnumerator()
        {
            var devices = MMDeviceEnumerator.EnumerateDevices(DataFlow.Capture, DeviceState.All);
            Console.WriteLine("There are {0} MM devices", devices.Count);
            Console.WriteLine(devices[0].FriendlyName);
        }
    }
}