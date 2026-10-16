using System.Collections.Generic;
using ScriptObject;
using UnityEngine;

namespace Utils.Config
{
    public class Configuration : MonoBehaviour
    {
        public List<ObservationPointPositionInfo> configList;

        public static Configuration Instance;

        private void Awake()
        {
            Instance = this;
        }
    }
}