﻿using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Urasandesu.NAnonym.Mixins.System.Reflection;
using SRE = System.Reflection.Emit;

namespace Urasandesu.NAnonym.DW
{
    class AnonymousInstanceMethodBodyWeaveBuilderWithBase : MethodBodyWeaveBuilder
    {
        public AnonymousInstanceMethodBodyWeaveBuilderWithBase(MethodBodyWeaveDefiner parentBodyDefiner)
            : base(parentBodyDefiner)
        {
        }

        public override void Construct()
        {
            var bodyDefiner = ParentBodyDefiner.ParentBody;
            var definer = bodyDefiner.ParentBuilder.ParentDefiner;

            var injectionMethod = definer.WeaveMethod;
            var gen = bodyDefiner.Gen;
            var ownerType = definer.Parent.ConstructorWeaver.DeclaringType;
            var cachedMethod = definer.CachedMethod;
            var cachedSetting = definer.CachedSetting;
            var returnType = injectionMethod.Source.ReturnType;
            var parameterTypes = definer.ParameterTypes;
            var baseMethod = definer.BaseMethod;

            gen.Eval(_ => _.If(_.Ld(_.X(cachedMethod.Name)) == null));
            {
                var dynamicMethod = default(DynamicMethod);
                gen.Eval(_ => _.St(dynamicMethod).As(new DynamicMethod(
                                                            "dynamicMethod",
                                                            _.X(returnType),
                                                            new Type[] { _.X(ownerType) }.Concat(_.X(parameterTypes)).ToArray(),
                                                            _.X(ownerType),
                                                            true)));


                var cacheField = default(FieldInfo);
                gen.Eval(_ => _.St(cacheField).As(_.X(ownerType).GetField(
                                                        _.X(cachedSetting.Name),
                                                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)));

                var targetMethod = default(MethodInfo);
                gen.Eval(_ => _.St(targetMethod).As(_.X(injectionMethod.Destination.DeclaringType).GetMethod(
                                                        _.X(injectionMethod.Destination.Name),
                                                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)));


                var il = default(ILGenerator);
                gen.Eval(_ => _.St(il).As(dynamicMethod.GetILGenerator()));
                gen.Eval(_ => il.Emit(SRE::OpCodes.Ldarg_0));
                gen.Eval(_ => il.Emit(SRE::OpCodes.Ldfld, cacheField));
                for (int parametersIndex = 0; parametersIndex < parameterTypes.Length; parametersIndex++)
                {
                    switch (parametersIndex)
                    {
                        case 0:
                            gen.Eval(_ => il.Emit(SRE::OpCodes.Ldarg_1));
                            break;
                        case 1:
                            gen.Eval(_ => il.Emit(SRE::OpCodes.Ldarg_2));
                            break;
                        case 2:
                            gen.Eval(_ => il.Emit(SRE::OpCodes.Ldarg_3));
                            break;
                        case 3:
                            gen.Eval(_ => il.Emit(SRE::OpCodes.Ldarg, (short)4));
                            break;
                        case 4:
                            gen.Eval(_ => il.Emit(SRE::OpCodes.Ldarg, (short)5));
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }
                gen.Eval(_ => il.Emit(SRE::OpCodes.Callvirt, targetMethod));
                gen.Eval(_ => il.Emit(SRE::OpCodes.Ret));
                gen.Eval(_ => _.St(_.X(cachedMethod.Name)).As(dynamicMethod.CreateDelegate(_.X(injectionMethod.DelegateType), _.This())));
            }
            gen.Eval(_ => _.EndIf());
            var invoke = injectionMethod.DelegateType.GetMethod(
                                                        "Invoke",
                                                        BindingFlags.Public | BindingFlags.Instance,
                                                        null,
                                                        parameterTypes,
                                                        null);
            var delegateForBaseConstructor = parameterTypes[0].GetConstructor(
                                                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                                    null,
                                                    new Type[] 
                                                    { 
                                                        typeof(Object), 
                                                        typeof(IntPtr) 
                                                    }, null);

            var delegateForBase = default(object);
            gen.Eval(_ => _.St(delegateForBase).As(_.New(_.X(delegateForBaseConstructor), _.Ftn(_.This(), _.X(baseMethod)))));
            var variableNames = new string[] { TypeSavable.GetName(() => delegateForBase) }.Concat(injectionMethod.Source.ParameterNames()).ToArray();
            gen.Eval(_ => _.Return(_.Invoke(_.Ld(_.X(cachedMethod.Name)), _.X(invoke), _.Ld(_.X(variableNames)))));
        }
    }
}