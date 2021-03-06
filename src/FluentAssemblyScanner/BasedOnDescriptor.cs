﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentAssemblyScanner
{
    public class BasedOnDescriptor
    {
        private readonly FromDescriptor from;
        private readonly List<Type> potentialBasedOns;
        private Predicate<Type> ifFilter;

        internal BasedOnDescriptor(IEnumerable<Type> basedOn, FromDescriptor from)
        {
            potentialBasedOns = basedOn.ToList();
            this.from = from;
            If(DefaultFilter);
        }

        private static bool DefaultFilter(Type type) => type.IsClass && type.IsAbstract == false;

        public BasedOnDescriptor If(Predicate<Type> filter)
        {
            ifFilter += filter;
            return this;
        }

        public BasedOnDescriptor NonStatic()
        {
            ifFilter += type => type.IsAbstract == false && type.IsSealed == false;
            return this;
        }

        public BasedOnDescriptor HasAttribute<TAttribute>() where TAttribute : Attribute
        {
            If(Component.HasAttribute<TAttribute>);
            return this;
        }

        public BasedOnDescriptor HasAttribute<TAttribute>(Predicate<TAttribute> filter) where TAttribute : Attribute
        {
            If(Component.HasAttribute(filter));
            return this;
        }

        public BasedOnDescriptor OrBasedOn(Type basedOn)
        {
            potentialBasedOns.Add(basedOn);
            return this;
        }

        public BasedOnDescriptor OrBasedOn<T>()
        {
            potentialBasedOns.Add(typeof(T));
            return this;
        }

        public List<Type> Scan()
        {
            return from.SelectedTypes()
                       .Where(ConsiderIfCondition)
                       .Where(ConsiderBasedOns)
                       .ToList();
        }

        private bool ConsiderBasedOns(Type type)
        {
            return potentialBasedOns.Any(t => t.IsAssignableFrom(type));
        }

        private bool ConsiderIfCondition(Type type)
        {
            if (ifFilter == null)
            {
                return true;
            }

            foreach (Predicate<Type> filter in ifFilter.GetInvocationList())
            {
                if (filter(type) == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}