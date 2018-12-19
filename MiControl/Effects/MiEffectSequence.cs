using System.Collections;
using System.Collections.Generic;

using MiControl.Effects;

namespace MiControl
{
    public class MiEffectSequence : IEnumerable<IMiEffect>
    {
        private IList<IMiEffect> _effects = new List<IMiEffect>();

        public IMiEffect this[int index]  => _effects[index];
        
        public void Add(IMiEffect effect)
        {
            _effects.Add(effect);
        }

        public IEnumerator<IMiEffect> GetEnumerator() => _effects.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _effects.GetEnumerator();
    }
}
