
using System;
using SR = System.Reflection;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Urasandesu.NAnonym.ILTools
{
    public interface ITypeGenerator : ITypeDeclaration, IMemberGenerator
    {
        IFieldGenerator AddField(string fieldName, Type type, SR::FieldAttributes attributes);
        IMethodGenerator AddMethod(string name, SR::MethodAttributes attributes, Type returnType, Type[] parameterTypes);
        IMethodGenerator AddMethod(string name, SR::MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes);
        new ReadOnlyCollection<IFieldGenerator> Fields { get; }
        new IModuleGenerator Module { get; }
        ITypeGenerator AddInterfaceImplementation(Type interfaceType);
        ITypeGenerator SetParent(Type parentType);
        IConstructorGenerator AddConstructor(MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes);
        Type CreateType();
    }

}