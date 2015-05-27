using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace Dashx.CEP.Datastructures
{
    public interface ICEPObject 
    {
        CEPObjectType GetCEPObjectType();
    }
    public enum CEPObjectType : int
    {
        Order = 1,
        ParentOrder,
        MarketData,
        ChildOrderWrapper,
        ParentOrderState,
        ParentOrderStateUpdate,
    }
    public class CEPObject : ICEPObject
    {
        public static CEPObjectType Type;
        public CEPObject(CEPObjectType type)
        {
            Type = type;
        }

        public static CEPObjectType StaticCEPObjectType(ICEPObject obj)
        {
            return obj.GetCEPObjectType();
        }

        public CEPObjectType GetCEPObjectType()
        {
            return Type;
        }

        public CEPObject()
        {
            
        }
    }
}
