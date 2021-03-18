using System.Collections;
using System.Collections.Generic;

namespace Runner
{
    public class CircularList<T> : IReadOnlyList<T>, IEnumerable<T>
    {
        private readonly IReadOnlyList<T> _source;
        private readonly IEnumerator<T> _enumerator;

        public CircularList(IReadOnlyList<T> source)
        {
            _source = source;
            _enumerator = GetEnumerator();
        }
        
        public new IEnumerator<T> GetEnumerator()
        {
            return new CircularEnumerator<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new CircularEnumerator<T>(this);
        }

        public int Count => _source.Count;

        public T this[int index] => _source[index];

        public T GetNext()
        { 
            _enumerator.MoveNext();
            T data = _enumerator.Current;
            return data;
        }
    }
}