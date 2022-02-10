using System;
using System.Collections.Generic;
using System.Text;

namespace CarbonField
{
    class Counter
    {
        private String _name;
        private int _count;

        public Counter(String s)
        {
            _name = s;
            _count = 0;
        }

        public bool Increment()
        {
            _count++;
            if (_name.Equals("Milliseconds"))
            {
                if (_count == 100)
                {
                    _count = 0;
                    return true;
                }
            } else

            if(_count == 60)
            {
                _count = 0;
                return true;
            }
            return false;
        }

        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }
    }
}
