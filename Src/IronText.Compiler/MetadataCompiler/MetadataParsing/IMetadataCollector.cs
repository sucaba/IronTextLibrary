﻿using IronText.Extensibility;

namespace IronText.MetadataCompiler
{
    interface IMetadataCollector
    {
        void AddMeta(ICilMetadata meta);
        void AddRule(ICilMetadata meta, CilProduction parseRule);
        void AddToken(CilSymbolRef token);
    }
}
