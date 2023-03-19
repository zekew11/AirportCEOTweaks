using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.IO;

namespace AirportCEOTweaks
{
    public class Extend_CommercialFlightModelSerializable
    {
        public CommercialFlightSaveData flightData;

        public Extend_CommercialFlightModelSerializable(CommercialFlightSaveData data)
        {
            flightData = data;
        }
    }

    public class Extend_CommericialFlightModelSerializableWrapper
    {
        public List<Extend_CommercialFlightModelSerializable> customSerializables;

        // Instantly set
        public Extend_CommericialFlightModelSerializableWrapper(List<Extend_CommercialFlightModelSerializable> customSerializables)
        {
            this.customSerializables = new List<Extend_CommercialFlightModelSerializable>();
            this.customSerializables = customSerializables;
        }

        // Set manually
        public Extend_CommericialFlightModelSerializableWrapper()
        {
            this.customSerializables = new List<Extend_CommercialFlightModelSerializable>();
        }
    }

}