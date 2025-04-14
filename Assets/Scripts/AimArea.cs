using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimArea : MonoBehaviour
{
    private PlayerController p_controller;

    private void Start()
    {
        p_controller = GetComponentInParent<PlayerController>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Enemy>(out Enemy enemy))
            p_controller.enemiesInRange.Add(enemy); 
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Enemy>(out Enemy enemy))
            p_controller.enemiesInRange.Remove(enemy);
    }
}
