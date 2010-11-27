﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using System.Collections.ObjectModel;
using Urasandesu.NAnonym.Linq;
using System.Runtime.Serialization;
using MC = Mono.Cecil;
using System.Reflection;
using Mono.Cecil.Cil;
using UNI = Urasandesu.NAnonym.ILTools;
using SR = System.Reflection;

namespace Urasandesu.NAnonym.Cecil.ILTools.Impl.Mono.Cecil
{
    [Serializable]
    class MCMethodBaseDeclarationImpl : MCMemberDeclarationImpl, UNI::IMethodBaseDeclaration
    {
        [NonSerialized]
        MethodReference methodRef;

        [NonSerialized]
        MethodDefinition methodDef;
        string methodName;
        MC::MethodAttributes methodAttr;
        string[] parameterTypeFullNames;

        UNI::ITypeDeclaration declaringTypeDecl;

        [NonSerialized]
        UNI::IMethodBodyDeclaration bodyDecl;
        [NonSerialized]
        ReadOnlyCollection<UNI::IParameterDeclaration> parameters;

        public MCMethodBaseDeclarationImpl(MethodReference methodRef)
            : base(methodRef)
        {
            Initialize(methodRef, ILEmitMode.Normal, null);
        }

        public MCMethodBaseDeclarationImpl(MethodReference methodRef, ILEmitMode mode, Instruction target)
            : base(methodRef)
        {
            Initialize(methodRef, mode, target);
        }

        void Initialize(MethodReference methodRef, ILEmitMode mode, Instruction target)
        {
            this.methodRef = methodRef;
            this.methodDef = methodRef.Resolve();
            methodName = methodDef.Name;
            methodAttr = methodDef.Attributes;
            parameterTypeFullNames = methodDef.Parameters.Select(parameter => parameter.ParameterType.FullName).ToArray();
            bodyDecl = new MCMethodBodyGeneratorImpl(methodDef.Body, mode, target);
            declaringTypeDecl = new MCTypeGeneratorImpl(methodRef.DeclaringType.Resolve());
            parameters = new ReadOnlyCollection<UNI::IParameterDeclaration>(
                methodRef.Parameters.TransformEnumerateOnly(parameter => (UNI::IParameterDeclaration)new MCParameterGeneratorImpl(parameter)));
        }


        public UNI::IMethodBodyDeclaration Body
        {
            get { return bodyDecl; }
        }

        public UNI::ITypeDeclaration DeclaringType
        {
            get { return declaringTypeDecl; }
        }

        public ReadOnlyCollection<UNI::IParameterDeclaration> Parameters
        {
            get { return parameters; }
        }

        public UNI::IPortableScopeItem NewPortableScopeItem(UNI::PortableScopeItemRawData itemRawData, object value)
        {
            var fieldDef = MethodDef.DeclaringType.Fields.First(field => field.Name == itemRawData.FieldName);
            var variableDef = MethodDef.Body.Variables.First(variable => variable.Index == itemRawData.LocalIndex);
            return new MCPortableScopeItemImpl(itemRawData, value, fieldDef, variableDef);
        }


        internal MethodDefinition MethodDef { get { return methodDef; } }
        protected UNI::IMethodBodyDeclaration BodyDecl { get { return bodyDecl; } }
        protected UNI::ITypeDeclaration DeclaringTypeDecl { get { return declaringTypeDecl; } }

        protected override void OnDeserializedManually(StreamingContext context)
        {
            var declaringTypeGen = (MCTypeGeneratorImpl)this.declaringTypeDecl;
            declaringTypeGen.OnDeserialized(context);
            var typeDef = declaringTypeGen.TypeDef;
            var methodDef = typeDef.Methods.First(
                method =>
                    method.Name == methodName &&
                    method.Attributes == methodAttr &&
                    method.Parameters.Select(parameter => parameter.ParameterType.FullName).Equivalent(parameterTypeFullNames));
            // TODO: PortableScope 系のテストを通るようにする。
            Initialize(methodDef, ILEmitMode.Normal, null);
            base.OnDeserializedManually(new StreamingContext(context.State, methodDef));
        }
    }
}
