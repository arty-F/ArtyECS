namespace ArtyECS.Core
{
    internal class Archetype
    {
        private sbyte[] _flags;

        internal Archetype(int maxIndex)
        {
            _flags = new sbyte[maxIndex + 1];
        }

        public override int GetHashCode()
        {
            var hash = 0;
            for (int i = 0; i < _flags.Length; i++)
            {
                hash ^= _flags[i].GetHashCode();
            }
            return hash;
        }

        internal void Set(int index, sbyte value = 1)
        {
            var minLength = index + 1;
            if (_flags.Length < minLength)
            {
                var newFlags = new sbyte[minLength];
                for (int i = 0; i < _flags.Length; i++)
                {
                    if (_flags[i] != 0)
                    {
                        newFlags[i] = _flags[i];
                    }
                }
                _flags = newFlags;
            }

            _flags[index] = value;
        }

        /*public bool Equals(Archetype other)
        {
            return this == other;
        }*/

        internal void Clear()
        {
            for (int i = 0; i < _flags.Length; i++)
            {
                _flags[i] = 0;
            }
        }
    }
}

