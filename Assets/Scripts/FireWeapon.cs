using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FireWeapon : MonoBehaviour
{
    [SerializeField] private Projectile bullet;

    [SerializeField] private float timer = 20f;
    private float timePassed = 0f;

    private void Update()
    {
        timePassed += Time.deltaTime;
        if (timePassed >= timer)
        {
            timePassed = 0;
            Instantiate(bullet, transform.position, transform.rotation);
        }
    }
}
