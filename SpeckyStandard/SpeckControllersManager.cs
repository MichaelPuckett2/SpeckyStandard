using SpeckyStandard.Controllers;
using SpeckyStandard.Enums;
using SpeckyStandard.Logging;
using System;

namespace SpeckyStandard
{
    public static class SpeckControllersManager
    {
        public static void StartControllers()
        {
            if (!SpeckAutoStrapper.IsStrappingStarted && !SpeckAutoStrapper.IsStrappingComplete)
                throw new Exception($"{nameof(SpeckControllersManager)} cannot start until {nameof(SpeckAutoStrapper)} has completed strapping the application.");

            if (!ControllerBuilder.IsControllersBuilt)
            {
                Log.Print("Building Controllers....", PrintType.DebugWindow);
                ControllerBuilder.Start();
            }

            if (RestDalController.Instance.CanStart) RestDalController.Instance.Start();
        }

        public static void StopControllers()
        {
            RestDalController.Instance.Stop();
        }
    }
}