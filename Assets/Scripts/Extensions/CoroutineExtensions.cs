using UnityEngine;
using System.Collections;

public static class CoroutineExtensions
{
    public static void StartCoroutineSafely(this MonoBehaviour owner, IEnumerator routine)
    {
        if (owner != null && owner.gameObject != null && owner.gameObject.activeInHierarchy)
        {
            owner.StartCoroutine(routine);
        }
    }

    public static Coroutine StartCoroutineSafely(this MonoBehaviour owner, IEnumerator routine, out bool started)
    {
        started = false;
        if (owner != null && owner.gameObject != null && owner.gameObject.activeInHierarchy)
        {
            started = true;
            return owner.StartCoroutine(routine);
        }
        return null;
    }
} 