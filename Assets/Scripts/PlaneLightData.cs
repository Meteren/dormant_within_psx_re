using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneLightData : MonoBehaviour
{
    [SerializeField] Light spotLight;
    Material mat;
    void Start()
    {
        mat = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLightVals();
    }

    private void UpdateLightVals()
    {
        mat.SetVector("_SpotLightPos", spotLight.transform.position);
        mat.SetFloat("_SpotLightIntensity", spotLight.intensity);
        mat.SetVector("_SpotLightDir", spotLight.transform.forward);
        mat.SetColor("_SpotLightColor", spotLight.color);
        mat.SetFloat("_SpotLightInnerCos",Mathf.Cos((spotLight.innerSpotAngle * 0.5f) * Mathf.Deg2Rad));
        mat.SetFloat("_SpotLightOuterCos", Mathf.Cos((spotLight.spotAngle * 0.5f) * Mathf.Deg2Rad));
        mat.SetFloat("_SpotLightRange", spotLight.range);
    }
}
