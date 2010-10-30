﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using SR = System.Reflection;
using System.Runtime.Serialization;
using System.Reflection;
using Urasandesu.NAnonym.Linq;
using Urasandesu.NAnonym.ILTools;
using Urasandesu.NAnonym.Cecil.ILTools.Mixins.Mono.Cecil;
using System.Collections.ObjectModel;

namespace Urasandesu.NAnonym.Cecil.ILTools.Impl.Mono.Cecil
{
    [Serializable]
    class MCTypeDeclarationImpl : MCMemberDeclarationImpl, ITypeDeclaration
    {
        [NonSerialized]
        TypeReference typeRef;

        [NonSerialized]
        TypeDefinition typeDef;

        [NonSerialized]
        ITypeDeclaration baseTypeDecl;

        string typeFullName;

        IModuleDeclaration moduleDecl;

        [NonSerialized]
        ReadOnlyCollection<IFieldDeclaration> fields;
        
        public MCTypeDeclarationImpl(TypeReference typeRef)
            : base(typeRef)
        {
            Initialize(typeRef);
        }

        void Initialize(TypeReference typeRef)
        {
            this.typeRef = typeRef;
            typeDef = typeRef.Resolve();
            typeFullName = typeDef.FullName;
            moduleDecl = new MCModuleGeneratorImpl(typeRef.Module);
            baseTypeDecl = typeRef.Equivalent(typeof(object)) ? null : new MCTypeDeclarationImpl(typeDef.BaseType);
            fields = new ReadOnlyCollection<IFieldDeclaration>(typeDef.Fields.TransformEnumerateOnly(fieldDef => (IFieldDeclaration)new MCFieldGeneratorImpl(fieldDef)));
        }

        public string FullName
        {
            get { return typeRef.FullName; }
        }

        public string AssemblyQualifiedName
        {
            get { return typeRef.FullName + ", " + typeRef.Module.Assembly.FullName; }
        }

        public ITypeDeclaration BaseType
        {
            get { return baseTypeDecl; }
        }

        public IModuleDeclaration Module { get { return moduleDecl; } }

        public IConstructorDeclaration GetConstructor(Type[] types)
        {
            // TODO: 本当は SR::BindingFlags.Default が正しい。修正。
            // MEMO: System.Object..ctor をそのまま参照させると、自身のコンストラクタ呼び出しに変換されてしまう？？
            if (typeRef.Equivalent(typeof(object)))
            {
                return new MCConstructorDeclarationImpl(typeRef.Module.Import(
                    typeof(object).GetConstructor(SR::BindingFlags.Public | SR::BindingFlags.NonPublic | SR::BindingFlags.Instance, null, types, null)));
            }
            else
            {
                return new MCConstructorDeclarationImpl(typeDef.GetConstructor(
                    SR::BindingFlags.Public | SR::BindingFlags.NonPublic | SR::BindingFlags.Instance, types));
            }
        }

        protected TypeDefinition TypeDef { get { return typeDef; } }

        protected override void OnDeserializedManually(StreamingContext context)
        {
            var moduleDecl = (MCModuleDeclarationImpl)this.moduleDecl;
            moduleDecl.OnDeserialized(context);
            var moduleDef = (ModuleDefinition)moduleDecl.ModuleRef;
            var typeDef = moduleDef.Types.First(type => type.FullName == typeFullName);
            Initialize(typeDef);
            base.OnDeserializedManually(context);
        }

        #region ITypeDeclaration メンバ


        public IFieldDeclaration[] GetFields(BindingFlags attr)
        {
            return typeDef.GetFields(attr).Select(fieldDef => (IFieldDeclaration)(MCFieldGeneratorImpl)fieldDef).ToArray();
        }

        #endregion

        #region ITypeDeclaration メンバ


        public IFieldDeclaration GetField(string name, BindingFlags bindingAttr)
        {
            var fieldDef = typeDef.GetFieldOrDefault(name, bindingAttr);
            return fieldDef == null ? null : new MCFieldGeneratorImpl(fieldDef);
        }

        #endregion

        #region ITypeDeclaration メンバ


        public ReadOnlyCollection<IFieldDeclaration> Fields
        {
            get { return fields; }
        }

        #endregion
    }

}
