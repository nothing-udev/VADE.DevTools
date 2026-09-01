using System.Collections;
using UnityEngine;

namespace VADE.DevTools.Utilities
{

    public class CoroutineRunner : MonoBehaviour
    {
        public Coroutine Run(IEnumerator routine) => StartCoroutine(routine);
    }
}
