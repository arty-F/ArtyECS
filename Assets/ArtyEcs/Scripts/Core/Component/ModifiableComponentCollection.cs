using System;

namespace ArtyECS.Core
{
    public readonly ref struct ModifiableComponentCollection<T> where T : struct, IComponent
    {
        private readonly Span<T> _span;
        private readonly ComponentTable<T> _table;
        
        internal ModifiableComponentCollection(ComponentTable<T> table)
        {
            _table = table ?? throw new ArgumentNullException(nameof(table));
            var (components, _, _) = table.GetInternalTable();
            _span = new Span<T>(components, 0, table.Count);
        }
        
        public int Count => _span.Length;
        
        public ref T this[int index]
        {
            get
            {
                if (index < 0 || index >= _span.Length)
                    throw new IndexOutOfRangeException($"Index {index} is out of range [0, {_span.Length})");
                
                return ref _span[index];
            }
        }
        
        public Entity GetEntity(int index)
        {
            if (index < 0 || index >= _span.Length)
                throw new IndexOutOfRangeException($"Index {index} is out of range [0, {_span.Length})");
            
            return _table.GetEntityAt(index);
        }
    }
}
