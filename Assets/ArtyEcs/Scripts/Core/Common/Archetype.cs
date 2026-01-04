using System;

namespace ArtyECS.Core
{
    public class Archetype : IEquatable<Archetype>
    {
        private ulong _bits0;
        private ulong[] _extendedBits;

        public Archetype()
        {
            _bits0 = 0;
            _extendedBits = null;
        }

        public Archetype(ulong bits0)
        {
            _bits0 = bits0;
            _extendedBits = null;
        }

        private Archetype(ulong bits0, ulong[] extendedBits)
        {
            _bits0 = bits0;
            _extendedBits = extendedBits;
        }

        public bool HasBit(int bitIndex)
        {
            if (bitIndex < 64)
            {
                return (_bits0 & (1UL << bitIndex)) != 0;
            }

            var arrayIndex = (bitIndex - 64) / 64;
            var localBitIndex = (bitIndex - 64) % 64;

            if (_extendedBits == null || arrayIndex >= _extendedBits.Length)
            {
                return false;
            }

            return (_extendedBits[arrayIndex] & (1UL << localBitIndex)) != 0;
        }

        public void SetBit(int bitIndex)
        {
            if (bitIndex < 64)
            {
                _bits0 |= 1UL << bitIndex;
                return;
            }

            var arrayIndex = (bitIndex - 64) / 64;
            var localBitIndex = (bitIndex - 64) % 64;

            if (_extendedBits == null || arrayIndex >= _extendedBits.Length)
            {
                EnsureExtendedCapacity(arrayIndex + 1);
            }

            _extendedBits[arrayIndex] |= 1UL << localBitIndex;
        }

        public void ClearBit(int bitIndex)
        {
            if (bitIndex < 64)
            {
                _bits0 &= ~(1UL << bitIndex);
                return;
            }

            var arrayIndex = (bitIndex - 64) / 64;
            var localBitIndex = (bitIndex - 64) % 64;

            if (_extendedBits == null || arrayIndex >= _extendedBits.Length)
            {
                return;
            }

            _extendedBits[arrayIndex] &= ~(1UL << localBitIndex);
        }

        public bool IsEmpty()
        {
            if (_bits0 != 0)
            {
                return false;
            }

            if (_extendedBits == null)
            {
                return true;
            }

            for (int i = 0; i < _extendedBits.Length; i++)
            {
                if (_extendedBits[i] != 0)
                {
                    return false;
                }
            }

            return true;
        }

        public bool Contains(Archetype other)
        {
            if ((_bits0 & other._bits0) != other._bits0)
            {
                return false;
            }

            if (other._extendedBits == null)
            {
                return true;
            }

            if (_extendedBits == null)
            {
                return false;
            }

            var minLength = Math.Min(_extendedBits.Length, other._extendedBits.Length);
            for (int i = 0; i < minLength; i++)
            {
                if ((_extendedBits[i] & other._extendedBits[i]) != other._extendedBits[i])
                {
                    return false;
                }
            }

            for (int i = minLength; i < other._extendedBits.Length; i++)
            {
                if (other._extendedBits[i] != 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static Archetype operator |(Archetype left, Archetype right)
        {
            var result = new Archetype(left._bits0 | right._bits0, null);

            var maxLength = 0;
            if (left._extendedBits != null)
            {
                maxLength = left._extendedBits.Length;
            }
            if (right._extendedBits != null && right._extendedBits.Length > maxLength)
            {
                maxLength = right._extendedBits.Length;
            }

            if (maxLength > 0)
            {
                result._extendedBits = new ulong[maxLength];
                
                if (left._extendedBits != null)
                {
                    Array.Copy(left._extendedBits, result._extendedBits, left._extendedBits.Length);
                }
                
                if (right._extendedBits != null)
                {
                    for (int i = 0; i < right._extendedBits.Length; i++)
                    {
                        result._extendedBits[i] |= right._extendedBits[i];
                    }
                }
            }

            return result;
        }

        public static Archetype operator &(Archetype left, Archetype right)
        {
            var result = new Archetype(left._bits0 & right._bits0, null);

            if (left._extendedBits == null || right._extendedBits == null)
            {
                return result;
            }

            var minLength = Math.Min(left._extendedBits.Length, right._extendedBits.Length);
            result._extendedBits = new ulong[minLength];

            for (int i = 0; i < minLength; i++)
            {
                result._extendedBits[i] = left._extendedBits[i] & right._extendedBits[i];
            }

            return result;
        }

        public static Archetype operator ~(Archetype archetype)
        {
            var result = new Archetype(~archetype._bits0, null);

            if (archetype._extendedBits != null)
            {
                result._extendedBits = new ulong[archetype._extendedBits.Length];
                for (int i = 0; i < archetype._extendedBits.Length; i++)
                {
                    result._extendedBits[i] = ~archetype._extendedBits[i];
                }
            }

            return result;
        }

        public static Archetype operator ^(Archetype left, Archetype right)
        {
            var result = new Archetype(left._bits0 ^ right._bits0, null);

            var maxLength = 0;
            if (left._extendedBits != null)
            {
                maxLength = left._extendedBits.Length;
            }
            if (right._extendedBits != null && right._extendedBits.Length > maxLength)
            {
                maxLength = right._extendedBits.Length;
            }

            if (maxLength > 0)
            {
                result._extendedBits = new ulong[maxLength];
                
                if (left._extendedBits != null)
                {
                    Array.Copy(left._extendedBits, result._extendedBits, left._extendedBits.Length);
                }
                
                if (right._extendedBits != null)
                {
                    for (int i = 0; i < right._extendedBits.Length; i++)
                    {
                        result._extendedBits[i] ^= right._extendedBits[i];
                    }
                }
            }

            return result;
        }

        public static bool operator ==(Archetype left, Archetype right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            if (left._bits0 != right._bits0)
            {
                return false;
            }

            if (left._extendedBits == null && right._extendedBits == null)
            {
                return true;
            }

            if (left._extendedBits == null || right._extendedBits == null)
            {
                if (left._extendedBits == null)
                {
                    return IsAllZeros(right._extendedBits);
                }
                return IsAllZeros(left._extendedBits);
            }

            if (left._extendedBits.Length != right._extendedBits.Length)
            {
                var shorter = left._extendedBits.Length < right._extendedBits.Length ? left._extendedBits : right._extendedBits;
                var longer = left._extendedBits.Length < right._extendedBits.Length ? right._extendedBits : left._extendedBits;

                for (int i = shorter.Length; i < longer.Length; i++)
                {
                    if (longer[i] != 0)
                    {
                        return false;
                    }
                }
            }

            var minLength = Math.Min(left._extendedBits.Length, right._extendedBits.Length);
            for (int i = 0; i < minLength; i++)
            {
                if (left._extendedBits[i] != right._extendedBits[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool operator !=(Archetype left, Archetype right)
        {
            return !(left == right);
        }

        private void EnsureExtendedCapacity(int minCapacity)
        {
            if (_extendedBits == null)
            {
                _extendedBits = new ulong[minCapacity];
                return;
            }

            if (_extendedBits.Length < minCapacity)
            {
                var newArray = new ulong[minCapacity];
                Array.Copy(_extendedBits, newArray, _extendedBits.Length);
                _extendedBits = newArray;
            }
        }

        private static bool IsAllZeros(ulong[] array)
        {
            if (array == null)
            {
                return true;
            }

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] != 0)
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Archetype);
        }

        public bool Equals(Archetype other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            var hash = _bits0.GetHashCode();
            if (_extendedBits != null)
            {
                for (int i = 0; i < _extendedBits.Length; i++)
                {
                    hash ^= _extendedBits[i].GetHashCode();
                }
            }
            return hash;
        }
    }
}

