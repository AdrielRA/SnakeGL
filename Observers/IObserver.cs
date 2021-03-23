using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameGL.Observers
{
    interface IObserver
    {
        void Update(ISubject subject);
    }
}
