using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarcassEnemy.Patches
{
    //[HarmonyPatch(typeof(NewMovement))]
    public static class ReplaceShaders
    {
        private static bool loaded;

        //[HarmonyPatch("Start"), HarmonyPostfix]
        private static void OnStart()
        {
            if (loaded)
                return;

            loaded = true;
            //Fix shader placeholders.
            foreach (Material mat in Plugin.AssetLoader.LoadAllAssets<Material>())
            {
                if (mat == null)
                    continue;

                //Each material has a placeholder shader with the same name as one in the game.
                mat.shader = Shader.Find(mat.shader.name);
            }
        }

    }
}
