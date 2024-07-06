using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity;

namespace AirportCEOTweaksCore
{
    public static class StaticExtensions
    {
        public static IMonoClassExtension ExtendMono<OriginalT,ExtenededT>(this OriginalT original, ref OriginalT originalRef)
            where ExtenededT:OriginalT , IMonoClassExtension
            where OriginalT : MonoBehaviour
        {
            if (original is ExtenededT)
            {
                return originalRef as ExtenededT;
            }

            
            GameObject gameObject = original.gameObject;
            ExtenededT extended = gameObject.AddComponent<ExtenededT>();
            extended.SetupExtend(original);
            GameObject.Destroy(original);

            originalRef = extended;
            return extended;
        }
    }

    public interface IMonoClassExtension
    {
        void SetupExtend(MonoBehaviour original);
    }
}
