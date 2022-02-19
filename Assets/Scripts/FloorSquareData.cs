using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class FloorSquareData : MonoBehaviour
    {
        public int id;
        public int KillCount;
        public GameObject  weapon;
        public int xPos, yPos;
        public Renderer renderer;

        public void setXPos(int xPos)
        {
            this.xPos = xPos;
        }

        public void setYPos(int yPos)
        {
            this.yPos = yPos;
        }

        public void Awake()
        {
            renderer = GetComponent<MeshRenderer>();
        }
    }
}