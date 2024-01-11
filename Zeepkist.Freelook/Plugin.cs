using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using ZeepkistClient;
using ZeepkistNetworking;
using ZeepSDK.Cosmetics;
using ZeepSDK.Racing;
using ZeepSDK.Workshop;

namespace Zeepkist.Freelook
{

    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("ZeepSDK")]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony harmony;

        public static ConfigEntry<bool> FreelookInThird { get; private set; }
        public static ConfigEntry<bool> FreelookInFirst { get; private set; }
        public static ConfigEntry<bool> Invert { get; private set; }
        public static ConfigEntry<int> MaxRotation { get; private set; }

        public static Quaternion BaseRotation { get; private set; }
        public static CameraGuideFollower CameraFollower { get; set; }

        public const float SLERPED_AMOUNT = 10;
        public const float RANDOM_AMOUNT = 1;
        public bool MOD_ENABLED = false;

        private void Awake()
        {
            harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

            // Plugin startup logic
            Debug.Log($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            FreelookInThird = Config.Bind<bool>("Mod", "Freelook in 3rd person", true);
            FreelookInFirst = Config.Bind<bool>("Mod", "Freelook in 1st person", true);
            Invert = Config.Bind<bool>("Mod", "Invert Y", true);
            MaxRotation = Config.Bind<int>("Mod", "Max Rotation", 90);

            RacingApi.EnteredThirdPerson += RacingApi_EnteredThirdPerson;
            RacingApi.EnteredFirstPerson += RacingApi_EnteredFirstPerson;
            RacingApi.PlayerSpawned += RacingApi_EnteredThirdPerson;
        }

        private void Update()
        {
            if (!MOD_ENABLED) return;

            foreach (var joystick in Rewired.ReInput.players.AllPlayers.First().controllers.Joysticks)
            {
                if (!joystick.enabled || !joystick.supportsVibration) continue;

                if ((joystick.Axes[2].value != 0 || joystick.Axes[3].value != 0) && CameraFollower != null)
                {
                    Quaternion freelookX = Quaternion.AngleAxis(MaxRotation.Value * joystick.Axes[2].value, transform.up);
                    Quaternion freelookY = Quaternion.AngleAxis((Invert.Value ? 1 : -1) * MaxRotation.Value * joystick.Axes[3].value, transform.right);

                    Quaternion slerped = Quaternion.Slerp(Camera.main.transform.rotation, CameraFollower.mainCameraPosition.rotation * freelookX * freelookY, SLERPED_AMOUNT * Time.deltaTime);
                    Camera.main.transform.rotation = slerped;
                } else if (CameraFollower != null)
                {
                    Quaternion slerped = Quaternion.Slerp(Camera.main.transform.rotation, CameraFollower.mainCameraPosition.rotation, SLERPED_AMOUNT * Time.deltaTime);
                    Camera.main.transform.rotation = slerped; //CameraFollower.mainCameraPosition.rotation;
                }
            }
        }


        private void RacingApi_EnteredFirstPerson()
        {
            if (FreelookInFirst.Value == true)
            {
                MOD_ENABLED = true;
            } else
            {
                MOD_ENABLED = false;
            }
        }

        private void RacingApi_EnteredThirdPerson()
        {
            if (FreelookInThird.Value == true)
            {
                MOD_ENABLED = true;
            }
            else
            {
                MOD_ENABLED = false;
            }
        }

        public void OnDestroy()
        {
            harmony?.UnpatchSelf();
            harmony = null;
        }
    }
}