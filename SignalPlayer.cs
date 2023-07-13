using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventTimer
{
    public class SignalPlayer
    {
        private readonly NetCoreAudio.Player _audioPlayer = new NetCoreAudio.Player();

        public async Task Play()
        {
            await _audioPlayer.Play("./sounds/signal.wav");
        }
    }
}