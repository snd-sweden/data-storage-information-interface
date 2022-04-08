## About the storage manifest
The purpose of this manifest is to integrate the university's research storage with Doris so that the researchers can easily describe, refer to and publish data without data having to leave the university's own storage.

The goal is to have as few required fields as posible and focus on using a ro-crate manifest to describe a set of files connected to a ornigization (e.g. a University) and a list of researchers (identified by Swamid/eduGAIN identifiers).

The manifest for describing the storage manifest and files is based on [ro-cate 1.1](https://www.researchobject.org/ro-crate/1.1/).

### Json schema for the manifest

[json schema](schema.json) - [documentation](docs/schema/)

### Example implementations

[Node.js command line tool](https://github.com/snd-sweden/data-storage-information-interface/tree/master/scripts/manifest-generators/nodejs)

[C# command line tool](https://github.com/snd-sweden/data-storage-information-interface/tree/master/scripts/manifest-generators/csharp)

### Example manifest

[Simple manifest with two files](example.json)
