using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarcassEnemy
{
    //Unity Editor QoL thing
    public class CarcassParametersAsset : ScriptableObject
    {
        //Match fields from CarcassParameters

        private CarcassParameters _parameters;
        public CarcassParameters Parameters 
        {
            get
            {
                if(_parameters == null)
                {
                    _parameters = Convert();
                }
                return _parameters;
            }
        }

        private CarcassParameters Convert()
        {
            return new CarcassParameters()
            {

            };
        }

    }
}
