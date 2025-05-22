using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HeroicEngine.Systems.DI
{
    [AttributeUsage(AttributeTargets.Field)]
    public class InjectAttribute : Attribute
    {
    };

    public interface IInjectable
    {
        void PostInject();
    }

    public sealed class InjectionManager : MonoBehaviour
    {
        private static List<ISystem> SystemsContainer = new();
        private static Dictionary<Type, IInjectable> ObjectsContainer = new();

        public static bool ContainsSystem(Type systemType)
        {
            return SystemsContainer.FindIndex(systemType.IsInstanceOfType) >= 0;
        }

        public static void RegisterSystem<T>(T systemInstance) where T : MonoBehaviour, ISystem
        {
            if (!SystemsContainer.Contains(systemInstance))
            {
                SystemsContainer.Add(systemInstance);

                foreach (var sys in SystemsContainer)
                {
                    InjectTo(sys);
                }
            }
        }

        public static void UnregisterSystem<T>(T systemInstance) where T : MonoBehaviour, ISystem
        {
            if (SystemsContainer.Contains(systemInstance))
            {
                SystemsContainer.Remove(systemInstance);
            }
        }

        public static T CreateGameObject<T>() where T : MonoBehaviour, IInjectable
        {
            var go = new GameObject();
            var obj = go.AddComponent<T>();

            InjectTo(obj);
            obj.PostInject();
            RegisterObject(obj);

            return obj;
        }

        public static T CreateObject<T>() where T : IInjectable
        {
            T obj = default;

            InjectTo(obj);
            obj?.PostInject();
            RegisterObject(obj);

            return obj;
        }

        public static void RegisterObject<T>(T obj) where T : IInjectable
        {
            if (obj == null)
            {
                obj = default;
                InjectTo(obj);
            }

            var type = typeof(T);

            ObjectsContainer[type] = obj;

            foreach (var sys in SystemsContainer)
            {
                InjectTo(sys);
            }

            foreach (var o in ObjectsContainer)
            {
                InjectTo(o.Value);
            }

            obj?.PostInject();
        }

        public static void InjectTo<T>(T instance)
        {
            InjectTo(instance, typeof(T));
            InjectTo(instance, instance.GetType());
        }

        private static void InjectTo<T>(T instance, Type monoType)
        {
            var objectFields = monoType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (var t in objectFields)
            {
                if (Attribute.GetCustomAttribute(t, typeof(InjectAttribute)) is InjectAttribute)
                {
                    var injectType = t.FieldType;
                    var system = SystemsContainer.Find(x => injectType.IsInstanceOfType(x));

                    if (system != null)
                    {
                        t.SetValue(instance, system);
                    }
                    else if (ObjectsContainer.TryGetValue(injectType, out var obj))
                    {
                        t.SetValue(instance, obj);
                    }
                }
            }
        }

        private void Awake()
        {
            InitializeSystems();
            DontDestroyOnLoad(gameObject);
        }

        private static void InitializeSystems()
        {
            SystemsContainer.AddRange(FindObjectsOfType<MonoBehaviour>(true).OfType<ISystem>());

            foreach (var sys in SystemsContainer)
            {
                InjectTo(sys);
                var go = (sys as MonoBehaviour)?.gameObject;
                DontDestroyOnLoad(go);
            }
        }
    }
}
