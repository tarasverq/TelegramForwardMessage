using System.Collections;
using System.Collections.Generic;

namespace Runner
{
    class CircularEnumerator<T> : IEnumerator<T>
    {
        private readonly IReadOnlyList<T> _list;
        int _i = -1;

        public CircularEnumerator(IReadOnlyList<T> list){
            this._list = list;
        }

        public T Current => _list[_i];

        object IEnumerator.Current => this;

        public void Dispose()
        {
        
        }

        public bool MoveNext()
        {
            _i = (_i + 1) % _list.Count;
            return true;
        }

        public void Reset()
        {
            _i = -1;
        }
    }
}