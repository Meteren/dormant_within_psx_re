using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class CustomFogController : MonoBehaviour
{

    public enum Method
    {
        linearFog,
        exponentialFog
    }

    [SerializeField] private VolumeProfile volumeProfile;
    private FogVolumeProfile fogVolumeProfile;

    [SerializeField] private Color fogColor;
    [Range(0,100)]
    [SerializeField] private float fogStart;
    [Range(0, 100)]
    [SerializeField] private float fogEnd;
    [Range(0, 100)]
    [SerializeField] private float fogDensity;
    [Range(0, 100)]
    [SerializeField] private float fogDistance;
    [SerializeField] private bool isActive;

    [SerializeField] private Method method;


    private void Start()
    {
        volumeProfile.TryGet<FogVolumeProfile>(out fogVolumeProfile);
    }
    private void Update()
    {
        if (fogVolumeProfile == null)
            return;

        fogVolumeProfile.method.value = (float)method;

        fogVolumeProfile.fogColor.value = this.fogColor;
        fogVolumeProfile.fogStart.value = this.fogStart;
        fogVolumeProfile.fogEnd.value = this.fogEnd;
        fogVolumeProfile.fogDistance.value = this.fogDistance;
        fogVolumeProfile.fogDensity.value = this.fogDensity;
        fogVolumeProfile.isActive.value = this.isActive;


    }

}
