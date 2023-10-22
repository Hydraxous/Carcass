using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarcassEnemy.Components.FX
{
    public class MaterialChanger : MonoBehaviour
    {
        [SerializeField] private Renderer renderer;
        private Material[] baseSet;

        public void SetMaterialSet(Material[] set)
        {
            if (baseSet == null)
                baseSet = renderer.materials;

            renderer.materials = set;
        }

        public void ResetMaterials()
        {
            if (baseSet == null)
                return;

            renderer.materials = baseSet;
        }
    }

}
