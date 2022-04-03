using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace spidermonke_v2.Patches
{
    [HarmonyPatch(typeof(GorillaLocomotion.Player))]
    [HarmonyPatch("Update", MethodType.Normal)]
    
    public class ExamplePatch : MonoBehaviour
    {



    }
}
