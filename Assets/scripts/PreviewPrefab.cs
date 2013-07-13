using UnityEngine;
using System.Collections;
using Lobo;

public class PreviewPrefab : MonoBehaviour
{
    public static PreviewPrefab main = null;

    void Awake()
    {
        // We will be instantiated, so don't freak out with multiple instances
        if( main == null )
            main = this;
    }
}
