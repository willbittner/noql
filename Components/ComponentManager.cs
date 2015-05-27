using System;
using System.Collections.Concurrent;

namespace NoQL.CEP.Components
{
    public interface IComponentManager
    {
        IComponent GetComponent(string name);

        void RegisterComponent(IComponent component);

        // IComponent CreateComponent<ComponentType>(string name = "");
    }

    internal class ComponentManager : IComponentManager
    {
        private static ConcurrentDictionary<string, IComponent> Components = new ConcurrentDictionary<string, IComponent>();
        private Processor processor;

        internal ComponentManager(Processor p)
        {
            processor = p;
        }

        #region IComponentManager Members

        public void RegisterComponent(IComponent component)
        {
            if (component == null) throw new Exception("You cannot register a null component, bad JuJu");
            if (component.GetComponentName() == "") throw new Exception("Component must have a name to register");
            if (component.GetComponentName() == null) throw new Exception(" Component name can't be null. Bad JuJu");
            Components[component.GetComponentName()] = component;
        }

        public IComponent GetComponent(string name)
        {
            IComponent outval;
            if (Components.TryGetValue(name, out outval))
            {
                return outval;
            }
            throw new Exception("Component Doesnt Exist");
        }

        public IComponent<ObjectType> GetComponent<ObjectType>(string name)
        {
            IComponent outval;
            if (Components.TryGetValue(name, out outval))
            {
                var com = outval as IComponent<ObjectType>;
                if (com == null)
                    throw new Exception("Component for that type does not exist.");
                return com;
            }

            throw new Exception("Component Doesnt Exist");
        }

        //public IComponent CreateComponent<ComponentType>(string name = "")
        //{
        //    return new Component<ComponentType>(processor, name);
        //}

        #endregion IComponentManager Members
    }
}