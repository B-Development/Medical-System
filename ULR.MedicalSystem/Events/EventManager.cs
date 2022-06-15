using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULR.MedicalSystem.Events
{
    public class EventManager
    {
        public Main pluginInstance;

        public EventManager(Main instance)
        {
            pluginInstance = instance;
        }
    }
}
