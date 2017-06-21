using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokenEmailPush
{
    public class QueueSlno
    {
        private Dictionary<Environment, long> _slnos = new Dictionary<Environment, long>();
        public void SetSlno(Environment environment, long slno)
        {
            lock (_slnos)
            {
                if (!_slnos.ContainsKey(environment))
                    _slnos.Add(environment, slno);
                else
                    _slnos[environment] = slno;
            }
        }
        public long GetSlno(Environment environment)
        {
            lock (_slnos)
            {
                if (!_slnos.ContainsKey(environment))
                    SetSlno(environment, 0);
                return _slnos[environment];
            }
        }
    }
}
