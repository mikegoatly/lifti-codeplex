# LIFTI
A Lightweight Full Text Indexer for .NET.

Migrated from the original [codeplex project](https://lifti.codeplex.com/), this version targets netstandard 1.4, so is usable pretty much anywhere. The only breaking changes are around how persisted indexes are constructed and that distributed transactions are no longer supported in this core library, but everything is is backwards compatible for now.

## Intro
LIFTI provides a simple API that allows you to build a searchable full text index. This can be useful for document retrieval or simply allowing .NET objects to be searched for by associated text.