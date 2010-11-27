using System.Collections.ObjectModel;
using System.Reflection;
using System;

namespace Urasandesu.NAnonym.ILTools
{
    public interface IMethodBaseGenerator : IMethodBaseDeclaration, IMemberGenerator
    {
        new IMethodBodyGenerator Body { get; }
        new ITypeGenerator DeclaringType { get; }
        new ReadOnlyCollection<IParameterGenerator> Parameters { get; }
        IPortableScopeItem AddPortableScopeItem(FieldInfo fieldInfo);
        IMethodBaseGenerator ExpressBody(Action<ExpressiveMethodBodyGenerator> bodyExpression);
        IParameterGenerator AddParameter(int position, ParameterAttributes attributes, string parameterName);
        PortableScope CarryPortableScope();
    }

}