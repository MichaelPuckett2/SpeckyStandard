using SpeckyStandard.Controllers;
using System;

namespace SpeckyStandard
{
    public static class SpeckControllersManager
    {
        public static void StartControllers()
        {
            if (!SpeckAutoStrapper.IsStrappingStarted && !SpeckAutoStrapper.IsStrappingComplete)
                throw new Exception($"{nameof(SpeckControllersManager)} cannot start until {nameof(SpeckAutoStrapper)} has completed strapping the application.");

            if (RestDalController.Instance.CanStart) RestDalController.Instance.Start();
        }

        public static void StopControllers()
        {
            RestDalController.Instance.Stop();
        }
    }
}