using BepInEx;
using HarmonyLib;

namespace FastPhone
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string pluginGuid      = "ninjacookie.brc.fastphone";
        public const string pluginName      = "Fast Phone";
        public const string pluginVersion   = "1.0.1";

        private void Awake()
        {
            var harmony = new Harmony(pluginGuid);
            harmony.PatchAll();
        }
    }
}