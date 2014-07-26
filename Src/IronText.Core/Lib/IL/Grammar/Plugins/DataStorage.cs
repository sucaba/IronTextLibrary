using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace IronText.Lib.IL
{
    internal class DataStorage : IEmitSyntaxPlugin
    {
        private readonly Dictionary<byte[],FieldSpec> plannedFields = new Dictionary<byte[],FieldSpec>();

        private int dataFieldCount = 0;
        private EmitSyntax currentEmit;
        private ICilDocumentInfo documentInfo;
        private string implDetailsTypeName;

        public void Init(ICilDocumentInfo documentInfo)
        {
            this.documentInfo = documentInfo;
            this.implDetailsTypeName = string.Format("<PrivateImplementationDetails>{{{0}}}", documentInfo.Mvid.ToString().ToUpper());
        }

        void IEmitSyntaxPlugin.SetCurrent(EmitSyntax emit)
        {
            this.currentEmit = emit;
        }

        public EmitSyntax Load(byte[] data)
        {
            currentEmit = currentEmit
                .Ldc_I4(data.Length)
                .Newarr(documentInfo.Types.UnsignedInt8)
                .Dup()
                .With<DataStorage>().LoadFieldHandle(data)
                .Call((Array arr, RuntimeFieldHandle handle) =>
                    RuntimeHelpers.InitializeArray(arr, handle)
                );

            return currentEmit;
        }

        public EmitSyntax LoadFieldHandle(byte[] data)
        {
            string dataTypeName  = GetDataFieldTypeName(data);
            string dataFieldName = string.Format("dataField{0}", dataFieldCount++);

            var fieldSpec = new FieldSpec { 
                    FieldType = documentInfo.Types.Value(ClassName.Parse(dataTypeName)),
                    DeclType  = documentInfo.Types.Class_(ClassName.Parse(implDetailsTypeName)),
                    FieldName = dataFieldName
                };

            plannedFields.Add(data, fieldSpec);
            
            return currentEmit.Ldtoken(fieldSpec);
        }

        private static string GetDataFieldTypeName(byte[] data)
        {
            return string.Format("Arraytype{0}", data.Length);
        }

        CilDocumentSyntax IEmitSyntaxPlugin.BeforeEndDocument(CilDocumentSyntax syntax)
        {
            foreach (var dataFieldPair in plannedFields)
            {
                syntax = syntax
                .Class_()
                        .Explicit.Ansi.Sealed
                        .Named(GetDataFieldTypeName(dataFieldPair.Key))
                        .Extends(syntax.Types.ValueType)
                    .Pack(1)
                    .Size(10)
                .EndClass();
            }
            
            ClassSyntax implDetails = syntax
                                    .Class_()
                                            .Public
                                            .Named(implDetailsTypeName);

            foreach (var dataFieldPair in plannedFields)
            {
                implDetails = implDetails
                    .Field()
                        .Private()
                        .Static()
                        .Assembly()
                        .OfType(dataFieldPair.Value.FieldType)
                        .Named(dataFieldPair.Value.FieldName)
                        .HasRVA
                        .Init(new Bytes(dataFieldPair.Key));
            }

            return implDetails.EndClass();
        }
    }
}