using System;
using System.Reflection;
using ThinkingHome.Migrator.Framework.Interfaces;

namespace ThinkingHome.Migrator.Providers
{
    public class ConditionByProvider : IConditionByProvider
    {
        private readonly ITransformationProvider currentProvider;

        public ITransformationProvider CurrentProvider
        {
            get { return currentProvider; }
        }

        public bool isExecuted;

        public ConditionByProvider(ITransformationProvider current)
        {
            if (current == null) throw new ArgumentNullException(nameof(current));

            currentProvider = current;
            isExecuted = false;
        }

        private static void ValidateProviderType(Type providerType)
        {
            if (providerType == null) throw new ArgumentNullException(nameof(providerType));

            if (!typeof(ITransformationProvider).GetTypeInfo().IsAssignableFrom(providerType))
            {
                throw new InvalidCastException("Provider class must implement the ITransformationProvider interface");
            }
        }

        public IConditionByProvider For<TProvider>(Action<ITransformationProvider> action)
        {
            return For(typeof(TProvider), action);
        }

        public IConditionByProvider For(Type providerType, Action<ITransformationProvider> action)
        {
            ValidateProviderType(providerType);

            bool needExecute = providerType.GetTypeInfo().IsInstanceOfType(currentProvider);

            if (needExecute)
            {
                action?.Invoke(currentProvider);
            }

            isExecuted |= needExecute;

            return this;
        }

        public void Else(Action<ITransformationProvider> action)
        {
            if (!isExecuted && action != null)
            {
                action(currentProvider);
                isExecuted = true;
            }
        }
    }
}