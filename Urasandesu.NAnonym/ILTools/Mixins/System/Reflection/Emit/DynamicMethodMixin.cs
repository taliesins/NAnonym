﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using Urasandesu.NAnonym.ILTools.Impl.System.Reflection;
using Urasandesu.NAnonym.ILTools.Mixins.Urasandesu.NAnonym.ILTools;

namespace Urasandesu.NAnonym.ILTools.Mixins.System.Reflection.Emit
{
    public static class DynamicMethodMixin
    {
        public static void ExpressBody(this DynamicMethod dynamicMethod, Action<ExpressiveMethodBodyGenerator> expression)
        {
            var gen = new ExpressiveMethodBodyGenerator(new SRMethodGeneratorImpl(dynamicMethod));
            gen.ExpressBodyEnd(expression);
        }

        public static void ExpressBody(this DynamicMethod dynamicMethod, Action<ExpressiveMethodBodyGenerator> expression, ParameterBuilder[] parameterBuilders)
        {
            var gen = new ExpressiveMethodBodyGenerator(new SRMethodGeneratorImpl(dynamicMethod, parameterBuilders));
            gen.ExpressBodyEnd(expression);
        }
    }
}