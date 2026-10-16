using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utils
{
    public class SceneChange : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log(other.gameObject.name);
            if (other.gameObject.name.Equals("XR Origin (XR Rig)"))
            {
                SceneManager.LoadScene("GSTestScene_室内");
            }
        }

    }
}