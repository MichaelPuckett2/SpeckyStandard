using SpeckyStandard.Controllers;

namespace SpeckyStandard
{
    public static class SpeckControllersManager
    {
        public static void StartControllers()
        {
            if (!RestDalController.Instance.IsStarted) RestDalController.Instance.Start();
        }

        public static void StopControllers()
        {
            RestDalController.Instance.Stop();
        }
    }
}