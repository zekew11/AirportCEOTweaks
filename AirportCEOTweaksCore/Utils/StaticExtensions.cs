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
            if (originalRef is ExtenededT)
            {
                return originalRef as ExtenededT;
            }

            
            GameObject gameObject = original.gameObject;
            ExtenededT extended = gameObject.AddComponent<ExtenededT>();
            extended.SetupExtend(original);
            //GameObject.Destroy(original);

            originalRef = extended;
            return extended;
        }
        public static IClassExtension Extend<OriginalT,ExtendedT>(this OriginalT original, ref OriginalT originalRef)
            where ExtendedT: class, OriginalT , IClassExtension, new()
            where OriginalT: class
        {
            if (originalRef is ExtendedT)
            {
                return originalRef as ExtendedT;
            }
            ExtendedT extended = new ExtendedT();
            extended.SetupExtend(originalRef);
            originalRef = extended;
            return extended;
        }
    }


    public interface IMonoClassExtension
    {
        void SetupExtend(MonoBehaviour original);
    }
    public interface IClassExtension
    {
        void SetupExtend(object oringinal);
    }
}
