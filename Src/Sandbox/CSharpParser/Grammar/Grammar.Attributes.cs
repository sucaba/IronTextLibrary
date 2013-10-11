using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;

namespace CSharpParser
{
    public partial interface ICsGrammar
    {
        [Parse]
        CsGlobalAttributes GlobalAttributes(
                CsList<CsGlobalAttributeSection> sections);

        [Parse("[", null, ":", null, "]")]
        [Parse("[", null, ":", null, ",", "]")]
        CsGlobalAttributeSection GlobalAttributeSection(
                CsGlobalAttributeTarget  target,
                CsCommaList<CsAttribute> attributes);

        // TODO: Schrodinger's token
#if false
        [Parse("assembly")]
        [Parse("module")]
        CsGlobalAttributeTarget GlobalAttributeTarget();
#else
        [Parse]
        CsGlobalAttributeTarget GlobalAttributeTarget(CsIdentifier target);
#endif

        [Parse]
        CsAttributes Attributes(CsList<CsAttributeSection> sections);

        [Parse("[", null, null, "]")]
        [Parse("[", null, null, ",", "]")]
        CsAttributeSection AttributeSection(
                Opt<CsAttributeTargetSpecifier> specifier,
                CsCommaList<CsAttribute>        attributes);

        [Parse(null, ":")]
        CsAttributeTargetSpecifier AttributeTargetSpecifier(
                CsAttributeTarget target);

        // TODO: Schrodinger's token
#if false
        [Parse("field")]
        [Parse("event")]
        [Parse("method")]
        [Parse("param")]
        [Parse("property")]
        [Parse("return")]
        [Parse("type")]
        CsAttributeTarget AttributeTarget();
#else
        [Parse]
        CsAttributeTarget AttributeTarget(CsIdentifier target);
#endif

        [Parse]
        CsAttribute Attribute(
                CsAttributeName           name,
                Opt<CsAttributeArguments> args);

        [Parse]
        CsAttributeName AttributeName(CsTypeName typeName); // also trim "Attribute" suffix

        [Parse("(", null, ")")]
        CsAttributeArguments AttributeArguments(
                CsOptCommaList<CsPositionalArgument> args);

        [Parse("(", null, ",", null, ")")]
        CsAttributeArguments AttributeArguments(
                CsCommaList<CsPositionalArgument> args,
                CsCommaList<CsNamedArgument>      namedArgs);

        [Parse("(", null, ")")]
        CsAttributeArguments AttributeArguments(
                CsCommaList<CsNamedArgument>      namedArgs);

        [Parse]
        CsPositionalArgument PositionalArgument(
                Opt<CsArgumentName>             name,
                CsAttributeArgumentExpression   expression);

        [Parse(null, "=", null)]
        CsNamedArgument NamedArgument(
                CsIdentifier                  identifier,
                CsAttributeArgumentExpression expression);

        [Parse]
        CsAttributeArgumentExpression AttributeArgumentExpression(
                CsExpression expression);
    }
}
