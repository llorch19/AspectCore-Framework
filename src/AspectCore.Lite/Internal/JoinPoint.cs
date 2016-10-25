﻿using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Internal
{
    internal class JoinPoint : IJoinPoint
    {
        private readonly IList<Func<InterceptorDelegate, InterceptorDelegate>> delegates;

        public IMethodInvoker MethodInvoker { get; set; }

        public JoinPoint()
        {
            delegates = new List<Func<InterceptorDelegate, InterceptorDelegate>>();
        }

        public void AddInterceptor(Func<InterceptorDelegate, InterceptorDelegate> interceptorDelegate)
        {
            ExceptionUtilities.ThrowArgumentNull(interceptorDelegate , nameof(interceptorDelegate));
            delegates.Add(interceptorDelegate);
        }

        public InterceptorDelegate Build()
        {
            ExceptionUtilities.Throw<InvalidOperationException>(() => MethodInvoker == null , "Calling proxy method failed.Because instance of ProxyMethodInvoker is null.");
            InterceptorDelegate next = context =>
            {
                var result = MethodInvoker.Invoke();
                context.ReturnParameter.Value = result;
                return Task.FromResult(0);
            };

            foreach (var @delegate in delegates.Reverse())
            {
                next = @delegate(next);
                ExceptionUtilities.Throw<InvalidOperationException>(() => next == null , "Invalid interceptorDelegate.");
            }

            return next;
        }
    }
}