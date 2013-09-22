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

        [Parse("assembly")]
        [Parse("module")]
        CsGlobalAttributeTarget GlobalAttributeTarget();

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

        [Parse("field")]
        [Parse("event")]
        [Parse("method")]
        [Parse("param")]
        [Parse("property")]
        [Parse("return")]
        [Parse("type")]
        CsAttributeTarget AttributeTarget();

        [Parse]
        CsAttribute Attribute(
                CsAttributeName           name,
                Opt<CsAttributeArguments> args);

        [Parse]
        CsAttributeName AttributeName(CsTypeName typeName); // also trim "Attribute" suffix

        [Parse("(", null, ")")]
        CsAttributeArguments AttributeArguments(
                CsOptCommaList<CsPositionalArgument> args);

#if false
        [Parse("(", null, ",", null, ")")]
        CsAttributeArguments AttributeArguments(
                CsCommaList<CsPositionalArgument> args,
                CsCommaList<CsNamedArgument>      namedArgs);

        [Parse("(", null, ")")]
        CsAttributeArguments AttributeArguments(
                CsCommaList<CsNamedArgument>      namedArgs);
#endif

        [Parse]
        CsPositionalArgument PositionalArgument(
                CsLiteral literal
                //Opt<CsArgumentName>             name,
                //CsAttributeArgumentExpression   expression
                );

        [Parse(null, "=", null)]
        CsNamedArgument NamedArgument(
                CsIdentifier                  identifier,
                CsAttributeArgumentExpression expression);

        [Parse]
        CsAttributeArgumentExpression AttributeArgumentExpression(
                CsExpression expression);
    }
}
