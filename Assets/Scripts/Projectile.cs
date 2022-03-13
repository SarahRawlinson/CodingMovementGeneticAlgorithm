using System;
using UnityEngine;

    public class Projectile : MonoBehaviour
    {
        [SerializeField] bool isHoming = false;
        [SerializeField] float liftime = 50f;
        // [SerializeField] FXHandler fXHandler;
        // [SerializeField] SoundFXHandler soundFXHandler;
        public float travelspeed;
        private float hitDammage;
        private GameObject instigator;
        // private WeaponHandler weaponHandler;


        private void Start()
        {
            Invoke("DestroyThis", liftime);
            
        }
        
        void Update()
        {
            if (isHoming)
            {
                try
                {
                    // transform.LookAt(weaponHandler.GetAimLocation());
                }
                catch (Exception e) { Debug.Log($"Proplem with Homing Projectile {e.Message} : {e.StackTrace}"); }
            }
            transform.Translate(Vector3.forward * travelspeed * Time.deltaTime);
        }
        
        private void DestroyThis()
        {
            Destroy(this);
        }
    }
