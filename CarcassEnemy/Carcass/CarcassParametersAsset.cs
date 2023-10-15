using UnityEngine;

namespace CarcassEnemy
{
    //Unity Editor QoL thing
    public class CarcassParametersAsset : ScriptableObject
    {
        //Match fields from CarcassParameters
        [SerializeField] private CarcassParameters parameters;

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
            if (parameters != null)
                return parameters;

            return new CarcassParameters();
        }

    }
}
