using System;
using UnityEngine;

namespace DefaultNamespace
{
    
    public class FloorSquareData : MonoBehaviour
    {
        public int zoneId;
        public int id;
        public int KillCount;
        private GameObject  weapon;
        public int xPos, yPos;
        public Renderer renderer;
        private GameObject checkPoint;


        public void SetCheckPoint(GameObject check)
        {
            Debug.Log($"Check Point Set To {check.name}");
            checkPoint = check;
        }

        public bool CheckForWeapon()
        {
            return weapon != null;
        }
        
        public bool CheckForCheckPoint()
        {
            return checkPoint != null;
        }
        
        public void SetWeapon(GameObject weaponObject)
        {
            weapon = weaponObject;
            Debug.Log($"Weapon Set To {weaponObject.name}");
            renderer.material.color = Color.grey;
        }
        
        public void setXPos(int xPos)
        {
            this.xPos = xPos;
        }

        public void setYPos(int yPos)
        {
            this.yPos = yPos;
        }
        
        public void setZoneId(int id)
        {
            this.zoneId = id;
        }

        public void Awake()
        {
            renderer = GetComponent<MeshRenderer>();
        }

        public void DeleteObjectData()
        {
            if (CheckForWeapon())
            {
                Debug.Log($"Destroying Weapon");
                Destroy(weapon);
                weapon = null;
            }

            if (CheckForCheckPoint())
            {
                Debug.Log($"Destroying Check Point");
                Destroy(checkPoint);
                checkPoint = null;
            }
            renderer.material.color = Color.magenta;
        }
    }
}