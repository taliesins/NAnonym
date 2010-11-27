﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Reflection.Emit;
using Urasandesu.NAnonym.Linq;

namespace Urasandesu.NAnonym.ILTools.Impl.System.Reflection
{
    class SRMethodBaseGeneratorImpl : SRMethodBaseDeclarationImpl, IMethodBaseGenerator
    {
        ISRMethodBaseGenerator methodBaseGen;
        readonly MethodBase methodBase;

        IMethodBodyGenerator methodBodyGen;
        ITypeGenerator declaringTypeGen;

        List<ParameterBuilder> parameterBuilders;
        List<SRParameterGeneratorImpl> listParameters;
        ReadOnlyCollection<IParameterGenerator> parameters;

        public SRMethodBaseGeneratorImpl(ISRMethodBaseGenerator methodBaseGen, MethodBase methodBase)
            : this(DeclaringTypeGeneratorOrDefault(methodBase), methodBaseGen, methodBase)
        {
        }

        public SRMethodBaseGeneratorImpl(ISRMethodBaseGenerator methodBaseGen, MethodBase methodBase, ParameterBuilder[] parameterBuilders, FieldBuilder[] fieldBuilders)
            : this(DeclaringTypeGeneratorOrDefault(methodBase, fieldBuilders), methodBaseGen, methodBase, parameterBuilders)
        {
        }

        public SRMethodBaseGeneratorImpl(ITypeGenerator declaringTypeGen, ISRMethodBaseGenerator methodBaseGen, MethodBase methodBase)
            : this(declaringTypeGen, methodBaseGen, methodBase, new ParameterBuilder[] { })
        {
        }

        public SRMethodBaseGeneratorImpl(ITypeGenerator declaringTypeGen, ISRMethodBaseGenerator methodBaseGen, MethodBase methodBase, ParameterBuilder[] parameterBuilders)
            : base(methodBase)
        {
            this.methodBase = methodBase;
            this.methodBaseGen = methodBaseGen;
            this.declaringTypeGen = declaringTypeGen;
            this.parameterBuilders = new List<ParameterBuilder>(parameterBuilders);
            listParameters = new List<SRParameterGeneratorImpl>(parameterBuilders.Select(parameterBuilder => new SRParameterGeneratorImpl(parameterBuilder)));
            parameters = new ReadOnlyCollection<IParameterGenerator>(
                listParameters.TransformEnumerateOnly(parameterGen => (IParameterGenerator)parameterGen));
            methodBodyGen = new SRMethodBodyGeneratorImpl(methodBaseGen);
        }

        static ITypeGenerator DeclaringTypeGeneratorOrDefault(MethodBase methodBase)
        {
            return DeclaringTypeGeneratorOrDefault(methodBase, new FieldBuilder[] { });
        }

        static ITypeGenerator DeclaringTypeGeneratorOrDefault(MethodBase methodBase, FieldBuilder[] fieldBuilders)
        {
            var declaringTypeBuilder = methodBase.DeclaringType as TypeBuilder;
            return declaringTypeBuilder == null ? null : new SRTypeGeneratorImpl(declaringTypeBuilder, fieldBuilders);
        }

        public new IMethodBodyGenerator Body
        {
            get { return methodBodyGen; }
        }

        public new ITypeGenerator DeclaringType
        {
            get { return declaringTypeGen; }
        }

        public new ReadOnlyCollection<IParameterGenerator> Parameters
        {
            get { return parameters; }
        }

        public IPortableScopeItem AddPortableScopeItem(FieldInfo fieldInfo)
        {
            throw new NotImplementedException();
        }

        public IMethodBaseGenerator CreateInstance(string name, MethodAttributes attributes, Type returnType, Type[] parameterTypes)
        {
            throw new NotImplementedException();
        }

        public IMethodBaseGenerator ExpressBody(Action<ExpressiveMethodBodyGenerator> bodyExpression)
        {
            var gen = new ExpressiveMethodBodyGenerator(this);
            bodyExpression(gen);
            if (gen.Directives.Last().OpCode != OpCodes.Ret)
            {
                gen.Eval(_ => _.End());
            }
            return this;
        }

        public IParameterGenerator AddParameter(int position, ParameterAttributes attributes, string parameterName)
        {
            var parameterBuilder = methodBaseGen.DefineParameter(position, attributes, parameterName);
            var parameterGen = new SRParameterGeneratorImpl(parameterBuilder);
            listParameters.Add(parameterGen);
            parameterBuilders.Add(parameterBuilder);
            return parameterGen;
        }

        public PortableScope CarryPortableScope()
        {
            throw new NotImplementedException();
        }
    }
}
