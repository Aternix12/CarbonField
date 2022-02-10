using System;
using System.Collections.Generic;
using System.Text;

namespace CarbonField
{
    class Clock
    {
        Counter[] _myCounters = new Counter[3];

        public Clock()
        {
            _myCounters[0] = new Counter("Hours");
            _myCounters[1] = new Counter("Minutes");
            _myCounters[2] = new Counter("Seconds");
        }

        public void Increment()
        {
            if (_myCounters[2].Increment())
            {
                if (_myCounters[1].Increment())
                    _myCounters[0].Increment();
            }
        }

        public string PrintTime()
        {
            String str = "";
            foreach(Counter c in _myCounters)
            {
                str = str + String.Format("{0:00}", c.Count);
                if(c != _myCounters[2])
                {
                    str = str + ":";
                }
            }
            return str;
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
