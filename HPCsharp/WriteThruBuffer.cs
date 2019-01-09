using System;
using System.Collections.Generic;
using System.Text;

namespace HPCsharp
{
    public class WriteThruBuffer<T>
    {
        private int _bufferSize;
        private T[] _buffer;
        private int _index;         // current location to be written to
        private T[] _destArray;
        private int _destIndex;

        public WriteThruBuffer(int bufferSize, T[] destinationArray)
        {
            _buffer = new T[bufferSize];
            _bufferSize = bufferSize;
            _index = 0;
            _destArray = destinationArray;
            _destIndex = 0;
        }

        public void WriteThru(T element)
        {
            if (_index < _buffer.Length)
            {
                _buffer[_index++] = element;
            }
            else
            {
                int numWrites = Math.Min(_buffer.Length, _destArray.Length - _destIndex);
                for (int i = 0; i < numWrites; i++)
                {
                    _destArray[_destIndex++] = _buffer[i++];
                }
                _index = 0;
            }
        }

        public void Flush()
        {
            int numWrites = Math.Min(_index, _destArray.Length - _destIndex);
            for (int i = 0; i < numWrites; i++)
            {
                _destArray[_destIndex++] = _buffer[i++];
            }
            _index = 0;
        }
    }

    public class WriteThruExternalBuffer<T>
    {
        private int _bufferSize;
        private T[] _buffer;
        private int _startIndex;
        private int _index;         // current location to be written to
        private T[] _destArray;
        private int _destIndex;

        public WriteThruExternalBuffer(T[] buffer, int startIndex, int bufferSize, T[] destinationArray)
        {
            _buffer = buffer;
            _bufferSize = bufferSize;
            _startIndex = startIndex;
            _index      = startIndex;
            _destArray = destinationArray;
            _destIndex = 0;
        }

        public void WriteThru(T element)
        {
            if (_index < _buffer.Length)
            {
                _buffer[_index++] = element;
            }
            else
            {
                int numWrites = Math.Min(_buffer.Length, _destArray.Length - _destIndex);
                for (int i = 0; i < numWrites; i++)
                {
                    _destArray[_destIndex++] = _buffer[i++];
                }
                _index = _startIndex;
            }
        }

        public void Flush()
        {
            int numWrites = Math.Min(_index, _destArray.Length - _destIndex);
            for (int i = 0; i < numWrites; i++)
            {
                _destArray[_destIndex++] = _buffer[i++];
            }
            _index = _startIndex;
        }
    }
}
