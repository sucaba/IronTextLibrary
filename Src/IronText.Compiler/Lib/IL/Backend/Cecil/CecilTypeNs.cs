using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Lib.Shared;
using Mono.Cecil;

namespace IronText.Lib.IL.Backend.Cecil
{
    class CecilTypeNs : ITypeNs
    {
        private readonly ModuleDefinition container;
        private readonly Dictionary<TypeReference, Def<Types>> TypeReference2Def = new Dictionary<TypeReference,Def<Types>>();

        public CecilTypeNs(ModuleDefinition container)
        {
            this.container = container;
        }

        public ClassName ClassNameInScope(Ref<ResolutionScopes> scope, SlashedName slashedName)
        {
            return new ClassName(scope, slashedName);
        }

        public Ref<Types> Class_(ClassName className)
        {
            TypeReference resultTypeRef = ClassNameToTypeReference(className);
            return ValueToRef(resultTypeRef);
        }

        public Ref<Types> Import(Type type)
        {
            TypeReference resultTypeRef = container.Import(type);
            return ValueToRef(resultTypeRef);
        }

        public TypeSpec TypeSpec(ClassName className)
        {
            return new TypeSpec { Type = Class_(className) };
        }

        public TypeSpec TypeSpec(Ref<Types> type)
        {
            return new TypeSpec { Type = type };
        }

        public Ref<Types> Object
        {
            get { return ValueToRef(container.TypeSystem.Object); }
        }

        public Ref<Types> String
        {
            get { return ValueToRef(container.TypeSystem.String); }
        }

        public Ref<Types> Value(ClassName className)
        {
            var typeRef = ClassNameToTypeReference(className);
            typeRef.IsValueType = true;
            return ValueToRef(typeRef);
        }

        public Ref<Types> Array(Ref<Types> elementType)
        {
            var elementTypeRef = RefToValue(elementType);
            return ValueToRef(new ArrayType(elementTypeRef));
        }

        public Ref<Types> Array(Ref<Types> elementType, Bounds1 bounds)
        {
            throw new NotImplementedException();
        }

        public Ref<Types> Reference(Ref<Types> elementType)
        {
            var elementTypeRef = RefToValue(elementType);
            return ValueToRef(new ByReferenceType(elementTypeRef));
        }

        public Ref<Types> Pointer(Ref<Types> elementType)
        {
            var elementTypeRef = RefToValue(elementType);
            return ValueToRef(new PointerType(elementTypeRef));
        }

        public Ref<Types> Pinned(Ref<Types> elementType)
        {
            var elementTypeRef = RefToValue(elementType);
            return ValueToRef(new PinnedType(elementTypeRef));
        }

        public Ref<Types> RequiredModifier(Ref<Types> elementType, ClassName className)
        {
            var elementRefType = RefToValue(elementType);
            var modifierType = ClassNameToTypeReference(className);
            var type = new RequiredModifierType(modifierType, elementRefType);
            return ValueToRef(type);
        }

        public Ref<Types> OptionalModifier(Ref<Types> elementType, ClassName className)
        {
            var elementRefType = RefToValue(elementType);
            var modifierType = ClassNameToTypeReference(className);
            var type = new OptionalModifierType(modifierType, elementRefType);
            return ValueToRef(type);
        }

        public Ref<Types> GenericArg(int genericArgIndex)
        {
            // GenericParameterType ctor requires IGenericParameterProvider.
            // Separate scope for IGenericParameterProvider is needed.
            throw new NotImplementedException();
        }

        public Ref<Types> Typedref
        {
            get { return ValueToRef(container.TypeSystem.TypedReference); }
        }

        public Ref<Types> Char
        {
            get { return ValueToRef(container.TypeSystem.Char); }
        }

        public Ref<Types> Void
        {
            get { return ValueToRef(container.TypeSystem.Void); }
        }

        public Ref<Types> Bool
        {
            get { return ValueToRef(container.TypeSystem.Boolean); }
        }

        public Ref<Types> Int8
        {
            get { return ValueToRef(container.TypeSystem.SByte); }
        }

        public Ref<Types> Int16
        {
            get { return ValueToRef(container.TypeSystem.Int16); }
        }

        public Ref<Types> Int32
        {
            get { return ValueToRef(container.TypeSystem.Int32); }
        }

        public Ref<Types> Int64
        {
            get { return ValueToRef(container.TypeSystem.Int64); }
        }

        public Ref<Types> Float32
        {
            get { return ValueToRef(container.TypeSystem.Single); }
        }

        public Ref<Types> Float64
        {
            get { return ValueToRef(container.TypeSystem.Double); }
        }

        public Ref<Types> UnsignedInt8
        {
            get { return ValueToRef(container.TypeSystem.Byte); }
        }

        public Ref<Types> UnsignedInt16
        {
            get { return ValueToRef(container.TypeSystem.UInt16); }
        }

        public Ref<Types> UnsignedInt32
        {
            get { return ValueToRef(container.TypeSystem.UInt32); }
        }

        public Ref<Types> UnsignedInt64
        {
            get { return ValueToRef(container.TypeSystem.UInt64); }
        }

        public Ref<Types> NativeInt
        {
            get { throw new NotImplementedException(); }
        }

        public Ref<Types> NativeUnsignedInt
        {
            get { throw new NotImplementedException(); }
        }

        public Ref<Types> NativeFloat
        {
            get { throw new NotImplementedException(); }
        }

        Ref<Types> ValueToRef(TypeReference typeRef)
        {
            var def = new ValueSlot<Types>();
            def.Value = typeRef;
            return def.GetRef();
        }

        TypeReference RefToValue(Ref<Types> reference)
        {
            return (TypeReference)reference.Value;
        }

        private TypeReference ClassNameToTypeReference(ClassName className)
        {
            TypeReference resultTypeRef = null;
            TypeReference innerType = null;
            
            /*
            if (className.Scope == null)
            {
                string fullName = className.Name;
                var definition = container.GetType(fullName);
                if (definition != null)
                {
                    return definition;
                }
            }
            */

            foreach (var name1 in className.SlashedName.Parts.Reverse())
            {
                var typeRef = new TypeReference(name1.Namespace, name1.Name, container, null);

                if (innerType == null)
                {
                    resultTypeRef = typeRef;
                }
                else
                {
                    innerType.DeclaringType = typeRef;
                }

                innerType = typeRef;
            }

            if (className.Scope != null)
            {
                resultTypeRef.Scope = (IMetadataScope)className.Scope.Value;
            }
            else
            {
                resultTypeRef.Scope = container;
            }

            return container.Import(resultTypeRef);
        }
    }
}
