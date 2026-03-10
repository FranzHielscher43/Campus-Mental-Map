using UnityEngine;
using GaussianSplatting.Runtime;

[ExecuteAlways]
public class ForceGaussianSplatInit : MonoBehaviour
{
    GaussianSplatRenderer r;

    void OnEnable()
    {
        r = GetComponent<GaussianSplatRenderer>();
        if (r && r.asset != null)
        {
            r.enabled = false;
            r.enabled = true;
        }
    }

    void OnValidate()
    {
        if (r == null)
            r = GetComponent<GaussianSplatRenderer>();

        if (r && r.asset != null)
        {
            r.enabled = false;
            r.enabled = true;
        }
    }
}