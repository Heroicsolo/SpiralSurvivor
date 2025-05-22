using System.Collections.Generic;
using UnityEngine;

namespace HeroicEngine.Utils.Data
{
    public class ConstructableSO : ScriptableObject
    {
        protected List<object> _parameters = new();

        public void Construct(params object[] args)
        {
            _parameters = new List<object>(args);
        }

        public virtual void Initialize()
        {
        }
    }
}
