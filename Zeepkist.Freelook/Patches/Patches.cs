using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeepkist.Freelook.Patches
{
    [HarmonyPatch(typeof(CameraGuideFollower), "Start")]
    public static class CameraGuideFollower_Start
    {
        public static void Postfix(CameraGuideFollower __instance) => Plugin.CameraFollower = __instance;
    }
}
