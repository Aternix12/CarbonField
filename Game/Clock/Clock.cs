using System;
using System.Collections.Generic;
using System.Text;

namespace CarbonField
{
    class Clock
    {
        readonly Counter[] _myCounters = new Counter[4];

        public Clock()
        {
            _myCounters[0] = new Counter("Hours");
            _myCounters[1] = new Counter("Minutes");
            _myCounters[2] = new Counter("Seconds");
            _myCounters[3] = new Counter("Milliseconds");
        }

        public void Increment()
        {
            if (_myCounters[3].Increment())
            {
                _myCounters[2].Increment();
            }

            if (_myCounters[2].Increment())
            {
                _myCounters[1].Increment();
            }

            if (_myCounters[1].Increment())
            {
                _myCounters[0].Increment();
            }
        }

        public string PrintTime()
        {
            StringBuilder strBuilder = new StringBuilder();

            foreach (Counter c in _myCounters)
            {
                strBuilder.AppendFormat("{0:00}", c.Count);

                if (c != _myCounters[3])
                {
                    strBuilder.Append(":");
                }
            }

            return strBuilder.ToString();
        }

        public float Seconds()
        {
            float _sec = _myCounters[3].Count/100f + _myCounters[2].Count + 60f*_myCounters[1].Count;
            return _sec;
        }

        public void Reset()
        {
            foreach(Counter c in _myCounters)
            {
                c.Count = 0;
            }
        }
    }
}
