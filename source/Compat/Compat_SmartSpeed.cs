using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Declutter_Main_Buttons_Bar.Compat
{
    public static class Compat_SmartSpeed
    {
        private const string SmartSpeedPackageId = "sarg.smartspeed";

        public static bool IsEnabled()
        {
            return ModsConfig.IsActive(SmartSpeedPackageId);
        }
    }
}
