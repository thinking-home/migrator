using System;

namespace ThinkingHome.Migrator.Framework
{
    public interface IConditionByProvider
    {
        ITransformationProvider CurrentProvider { get; }

        IConditionByProvider For<TProvider>(Action<ITransformationProvider> action);

        IConditionByProvider For(string providerTypeName, Action<ITransformationProvider> action);

        IConditionByProvider For(Type providerType, Action<ITransformationProvider> action);

        void Else(Action<ITransformationProvider> action);
    }
}