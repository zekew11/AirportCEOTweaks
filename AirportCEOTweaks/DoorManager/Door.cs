using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace AirportCEOTweaks.DoorManager
{
    class Door : MonoBehaviour
    {
        
        public bool Available { get; set; }
        public DoorSide Side { get; set; }
        public DoorState State { get; set; }
        public float MetersFromNose 
        { 
            get 
            {
                return metersFromNose;
            } 
            set
            {
                metersFromNose = value;
            }
        }
        public float MetersFromCenter
        {
            get
            {
                return math.abs(metersFromCenter);
            }
            set
            {
                if (Side == DoorSide.Right)
                {
                    metersFromCenter = math.abs(value);
                }
                else if (Side == DoorSide.Left)
                {
                    metersFromCenter = -math.abs(value);
                }
                else
                {
                    metersFromCenter = value;
                }
            }
        }
        public float Angle // 0 == forward, 90 to stbd, 180 rwd, 270 port
        {
            get
            {
                if (angle < 0) // only true on init with no assigned value. Our setter assures positive angles only.
                {
                    if (Side == DoorSide.Right)
                    {
                        angle = 90f;
                    }
                    else if (Side == DoorSide.Left)
                    {
                        angle = 270f;
                    }
                    else // we assume rear facing here
                    {
                        angle = 180f;
                    }
                }
                return angle;
            }
            set
            {
                angle = value % 360;
                if (angle < 0)
                {
                    angle += 360;
                }
            }
        }
       
        private float metersFromNose;
        private float metersFromCenter;
        private float angle = -1;
        public enum DoorSide
        {
            Left,
            Right,
            Center
        };
        public enum DoorState
        {
            Closed,
            Opening,
            Open,
            Closing
        };

    }
}
