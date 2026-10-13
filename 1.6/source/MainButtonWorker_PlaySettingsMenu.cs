using RimWorld;
using Verse;
using RimWorld.Planet;

namespace Declutter_Main_Buttons_Bar
{
    public class MainButtonWorker_PlaySettingsMenu : MainButtonWorker
    {
        public override bool Visible => ModSettings.useSearchablePlaySettingsMenu && !ModSettings.revealPlaySettingsOnHover;

        public override bool Disabled => false;

        public override void Activate()
        {
            Find.WindowStack.Add(new MapControlsTableWindow(WorldRendererUtility.WorldRendered));
        }
    }
}
