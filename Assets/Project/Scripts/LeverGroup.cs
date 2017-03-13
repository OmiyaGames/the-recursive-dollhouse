using UnityEngine;
using System.Collections.Generic;

namespace Toggler
{
    public class LeverGroup : MonoBehaviour
    {
        readonly List<Lever> allLevers = new List<Lever>();

        bool state = false;

        public bool IsOn
        {
            get
            {
                return state;
            }
            internal set
            {
                if (state != value)
                {
                    state = value;
                    foreach (Lever lever in allLevers)
                    {
                        lever.IsOnDirect = state;
                    }
                }
            }
        }

        internal void AddToGroup(Lever lever)
        {
            allLevers.Add(lever);
        }
    }
}
