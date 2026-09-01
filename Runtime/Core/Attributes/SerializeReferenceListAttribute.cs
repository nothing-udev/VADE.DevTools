using System;
using UnityEngine;

namespace VADE.DevTools.Attributes
{
    public class SerializeReferenceListAttribute : PropertyAttribute
    {
        public readonly Type BaseType;
        public SerializeReferenceListAttribute(Type baseType) => BaseType = baseType;
    }
}
