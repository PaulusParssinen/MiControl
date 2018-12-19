using System;
using System.Collections.Generic;
using System.Text;

namespace MiControl.Effects
{
    class MiLastCommandEffect : IMiEffectBase
    {
        public int Duration { get; set; }

        public MiLastCommandEffect(int duration)
        {
            Duration = duration;
        }
    }
}
