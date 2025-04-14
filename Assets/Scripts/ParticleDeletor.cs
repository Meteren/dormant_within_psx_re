using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleDeletor : MonoBehaviour
{
    private ParticleSystem particle;
    float particleDuration;
    void Start()
    {
        particle = GetComponent<ParticleSystem>();
        particleDuration = particle.main.duration;
    }

    // Update is called once per frame
    void Update()
    {
        particleDuration -= Time.deltaTime;
        if(particleDuration <= 0)
            Destroy(gameObject);
    }
}
