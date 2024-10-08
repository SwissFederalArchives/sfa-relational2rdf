# Relational2Rdf
Copy of new rewritten Siard2Rdf Repo

# Description
Relational2Rdf is a tool which can convert relational datasources to rdf graphs with the help of llm naming.

## Supported Date Sources
| Name  | Version   | CLI Command | Argument              |
|-------|-----------|-------------|-----------------------|
| Siard | 1, 2, 2.1 | siard       | Path to siard archive |

# CLI

## Parameters
The program comes with an CLI inteface allowing the following Parameters


| Parameter      | Shortname | Default                 | Description                                                                                                                      |
|----------------|-----------|-------------------------|----------------------------------------------------------------------------------------------------------------------------------|
| --threads      | -t        | Processor Count         | How many threads should be used, each thread can convert a table in the datasource at the same time                              |
| --base-iri     | -i        | https://ld.admin.ch/    | BaseIRI for RDF Names in the generated graph                                                                                     |
| --no-console   |           | False                   | Suppress any console output to default output                                                                                    |
| --table-config | -c        | -                       | Path to table config file, default is provided as table-config.json                                                              |
| --output       | -o        | ./                      | Output Directory for archive                                                                                                     |
| --output-file  | -f        | Name of the datasource  | Name for the resulting archive                                                                                                   |
| --trace        |           | False                   | Whether to save tracing information (Debug information, usefull to track why tool is slow)                                       |
| --converter    | -v        | Ontology                | Which conversion algorithm to use, converter specific options are prefixed with it's name. Converters are: `Ontology`. `Ai`      |
| --ai-key       | -k        | -                       | Key for the AI Endpoint                                                                                                          |
| --ai-endpoint  | -e        | https://api.openai.com/ | Base URL to AI Endpoint, must be OpenAI compatible, for self hostable alternative see [ollama](https://github.com/ollama/ollama) |
| --ai-model     | -m        | gpt-3.5-turbo           | AI Model to use                                                                                                                  |
| --ai-service   | -s        | OpenAI                  | Ai Service to use, available Choices: `OpenAI`, `Ollama`                                                                         |

## Table Config

The table config is a JSON File with the following properties, it can be supplied via the `--table-config` parameter

Default JSON:
```json
{
  "MaxBlobLength": 134217728,
  "MaxBlobLengthBeforeCompression": 8192,
  "BlobCompressionLevel": "SmallestSize",
  "BlobToLargeErrorValue": "Error Blob was to large during conversion",
  "ConvertMetadata": false,
  "BiDirectionalReferences": true
}
```

| Name                           | Value in Default                          | Description                                                                                                                               |
|--------------------------------|-------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------|
| MaxBlobLength                  | 134217728 (128mb)                         | What the maximum allowed blob size is, since blobs are included as Base64 string object, set to null to allow any blobs (not recommended) |
| MaxBlobLenghtBeforeCompression | 8192 (8kb)                                | What the maximum blob size is before it should be gzip compressed, set to null to disable                                                 |
| BlobCompressionLevel           | SmallestSize                              | Which compression level to use for blob compression, legal values: `NoCompression`, `Fastest`, `Optimal`, `SmallestSize`                  |
| BlobToLargeErrorValue          | Error Blob was to large during conversion | Placeholder Value if blob was too large, can be set to null, won't be Base64 encoded                                                      |
| ConvertMetadata                | False                                     | Currently doesn't do anything, will include source metadata, like original table names, in the RDF graph in future                        |
| BiDirectionalReferences        | True                                      | If Relationships should be bi-directional or only from foreign-key holder perspective                                                     |

## Converters
A converter is an algorithm which defines, how a relational data source get's converted to it's knowledgegraph representation.

### Ontology
A fixed Ontology describing a relational data source, depicted by the following Diagram.

![Ontology](./.docs/ontology.drawio.png)

### Ai
Uses large language models to rename fields and relationships to human friendly names following common rdf conventions.
The data itself is transformed into a knowledgegraph and meta data about it's orginal data source is lost.

## Basic Usage
`Relational2Rdf.Cli(.exe) siard [Siard File] --table-config table-config.json --threads 8 --ai-key <your open ai key> -v Ai`

## Download
https://github.com/SwissFederalArchives/sfa-relational2Rdf/releases/tag/v1.1.0
