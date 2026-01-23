namespace ArtyECS.Core
{
    internal class Archetype
    {
        private sbyte[] _flags;

        internal Archetype()
        {
            _flags = new sbyte[ComponentsManager.ComponentTypesCount < Constants.DEFAULT_ARCHETYPE_CAPACITY 
                ? Constants.DEFAULT_ARCHETYPE_CAPACITY 
                : ComponentsManager.ComponentTypesCount];
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

        internal void SetFlag(int index)
        {
            SetFlag(index, 1);
        }

        internal bool HasFlag(int index)
        {
            if (index >= _flags.Length)
            {
                return false;
            }

            return _flags[index] == 1;
        }

        internal void RemoveFlag(int index)
        {
            SetFlag(index, 0);
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

        private void SetFlag(int index, sbyte value)
        {
            if (_flags.Length < index + 1)
            {
                var newFlags = new sbyte[ComponentsManager.ComponentTypesCount];
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
    }
}

