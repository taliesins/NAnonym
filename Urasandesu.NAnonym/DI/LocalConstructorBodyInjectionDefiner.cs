﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Urasandesu.NAnonym.DI
{
    class LocalConstructorBodyInjectionDefiner : ConstructorBodyInjectionDefiner
    {
        public new LocalConstructorBodyInjection ParentBody { get { return (LocalConstructorBodyInjection)base.ParentBody; } }
        public LocalConstructorBodyInjectionDefiner(LocalConstructorBodyInjection parentBody)
            : base(parentBody)
        {
        }
    }
}