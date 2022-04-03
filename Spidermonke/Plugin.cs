using BepInEx;
using BepInEx.Configuration;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR;
using Utilla;
using System.ComponentModel;


namespace spidermonke_v2
{
    [Description("HauntedModMenu")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [ModdedGamemode]
    public class Plugin : BaseUnityPlugin
    {

        //bools
        public bool cangrapple = true;
        public bool canleftgrapple = true;
        public bool wackstart = false;
        public bool start = true;
        public bool inAllowedRoom = false;
        public bool hauntedModMenuEnabled = true;

        //floats
        public float triggerpressed;
        public float lefttriggerpressed;
        public float maxDistance = 100;
        public float Spring;
        public float Damper;
        public float MassScale;

        //vectors
        public Vector3 grapplePoint;
        public Vector3 leftgrapplePoint;

        //springjoints
        public SpringJoint joint;
        public SpringJoint leftjoint;

        //linerenderers
        public LineRenderer lr;
        public LineRenderer leftlr;

        //colors
        public Color grapplecolor;

        public static ConfigEntry<float> sp;
        public static ConfigEntry<float> dp;
        public static ConfigEntry<float> ms;
        public static ConfigEntry<Color> rc;

        #pragma warning disable IDE0051 // IDE0051: Remove unused member
        void Awake()
        {
            HarmonyPatches.ApplyHarmonyPatches();
            var customFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "web.cfg"), true);
            sp = customFile.Bind("Configuration", "Spring", 10f, "spring");
            dp = customFile.Bind("Configuration", "Damper", 30f, "damper");
            ms = customFile.Bind("Configuration", "MassScale", 12f, "massscale");
            rc = customFile.Bind("Configuration", "webColor", Color.white, "webcolor hex code");

        }

        void Update()
        {
            if (!inAllowedRoom)
            {
                if (start)
                {
                    Destroy(joint);
                    Destroy(leftjoint);

                    start = false;

                }
            }


            //this checks if the player is in a modded lobby
            if (inAllowedRoom && hauntedModMenuEnabled)
            {

                start = true;
                //right hand inputs
                List<InputDevice> list = new List<InputDevice>();
                InputDevices.GetDevices(list);

                for (int i = 0; i < list.Count; i++) //Get input
                {
                    if (list[i].characteristics.HasFlag(InputDeviceCharacteristics.Left))
                    {
                        list[i].TryGetFeatureValue(CommonUsages.trigger, out lefttriggerpressed);
                    }
                    if (list[i].characteristics.HasFlag(InputDeviceCharacteristics.Right))
                    {
                        list[i].TryGetFeatureValue(CommonUsages.trigger, out triggerpressed);
                    }
                }


                //this if statement will only be called once
                if (!wackstart)
                {
                    var child = new GameObject();


                    //cfg file
                    Spring = Plugin.sp.Value;
                    Damper = Plugin.dp.Value;
                    MassScale = Plugin.ms.Value;
                    grapplecolor = Plugin.rc.Value;

                    //linerenderer
                    lr = GorillaLocomotion.Player.Instance.gameObject.AddComponent<LineRenderer>();
                    lr.material = new Material(Shader.Find("Sprites/Default"));
                    lr.startColor = grapplecolor;
                    lr.endColor = grapplecolor;
                    lr.startWidth = 0.02f;
                    lr.endWidth = 0.02f;

                    leftlr = child.AddComponent<LineRenderer>();
                    leftlr.material = new Material(Shader.Find("Sprites/Default"));
                    leftlr.startColor = grapplecolor;
                    leftlr.endColor = grapplecolor;
                    leftlr.startWidth = 0.02f;
                    leftlr.endWidth = 0.02f;


                    //start var
                    wackstart = true;
                }

                //draws a rope
                DrawRope(GorillaLocomotion.Player.Instance);
                LeftDrawRope(GorillaLocomotion.Player.Instance);

                //this checks if the player is pressing the right trigger
                if (triggerpressed > 0.1f)
                {

                    if (cangrapple)
                    {
                        Spring = Plugin.sp.Value;
                        StartGrapple(GorillaLocomotion.Player.Instance);
                        cangrapple = false;
                    }


                }
                else //this checks if the player is not pressing the right trigger
                {

                    StopGrapple(GorillaLocomotion.Player.Instance);

                }



                if (triggerpressed > 0.1f && lefttriggerpressed > 0.1f)
                {

                    Spring = Spring / 2;
                }
                else
                {
                    Spring = Plugin.sp.Value;
                }





                //this checks if the player is pressing the left trigger
                if (lefttriggerpressed > 0.1f)
                {

                    if (canleftgrapple)
                    {
                        Spring = Plugin.sp.Value;
                        LeftStartGrapple(GorillaLocomotion.Player.Instance);
                        canleftgrapple = false;
                    }


                }
                else //this checks if the player is not pressing the left trigger
                {

                    LeftStopGrapple();
                }



            }
        }

        public void StartGrapple(GorillaLocomotion.Player __instance)
        {
            //raycast settings
            RaycastHit hit;
            if (Physics.Raycast(__instance.rightHandTransform.position, __instance.rightHandTransform.forward, out hit, maxDistance))
            {

                grapplePoint = hit.point;

                //joint settings
                joint = __instance.gameObject.AddComponent<SpringJoint>();
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = grapplePoint;

                float distanceFromPoint = Vector3.Distance(__instance.bodyCollider.attachedRigidbody.position, grapplePoint);

                joint.maxDistance = distanceFromPoint * 0.8f;
                joint.minDistance = distanceFromPoint * 0.25f;

                joint.spring = Spring;
                joint.damper = Damper;
                joint.massScale = MassScale;

                lr.positionCount = 2;
            }
        }

        public void DrawRope(GorillaLocomotion.Player __instance)
        {
            //rope settings
            if (!joint) return;

            lr.SetPosition(0, __instance.rightHandTransform.position);
            lr.SetPosition(1, grapplePoint);
        }

        public void StopGrapple(GorillaLocomotion.Player __instance)
        {



            //stops the grapple
            lr.positionCount = 0;
            Destroy(joint);
            cangrapple = true;
        }








        public void LeftStartGrapple(GorillaLocomotion.Player __instance)
        {
            //raycast settings
            RaycastHit lefthit;
            if (Physics.Raycast(__instance.leftHandTransform.position, __instance.leftHandTransform.forward, out lefthit, maxDistance))
            {
                leftgrapplePoint = lefthit.point;

                //joint settings
                leftjoint = __instance.gameObject.AddComponent<SpringJoint>();
                leftjoint.autoConfigureConnectedAnchor = false;
                leftjoint.connectedAnchor = leftgrapplePoint;

                float leftdistanceFromPoint = Vector3.Distance(__instance.bodyCollider.attachedRigidbody.position, leftgrapplePoint);

                leftjoint.maxDistance = leftdistanceFromPoint * 0.8f;
                leftjoint.minDistance = leftdistanceFromPoint * 0.25f;

                leftjoint.spring = Spring;
                leftjoint.damper = Damper;
                leftjoint.massScale = MassScale;

                leftlr.positionCount = 2;
            }
        }

        public void LeftDrawRope(GorillaLocomotion.Player __instance)
        {
            //rope settings
            if (!leftjoint) return;

            leftlr.SetPosition(0, __instance.leftHandTransform.position);
            leftlr.SetPosition(1, leftgrapplePoint);
        }

        public void LeftStopGrapple()
        {
            //stops the grapple
            leftlr.positionCount = 0;
            Destroy(leftjoint);
            canleftgrapple = true;
        }

        [ModdedGamemodeJoin]
        private void RoomJoined(string gamemode)
        {
            // The room is modded. Enable mod stuff.
            
            
            inAllowedRoom = true;
                    
        }

        [ModdedGamemodeLeave]
        private void RoomLeft(string gamemode)
        {
            // The room was left. Disable mod stuff.
            inAllowedRoom = false;
        }

        void OnEnable()
        {
            hauntedModMenuEnabled = true;
        }

        void OnDisable()
        {
            hauntedModMenuEnabled = false;
        }

    }
}
