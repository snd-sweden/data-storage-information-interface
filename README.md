## About the storage manifest
The purpose of this manifest is to integrate a local research storage with SND DORIS so that the researchers can easily describe, refer to and publish data without data having to leave the organisation's own storage.

The goal is to have as few required fields as possible and focus on using a ro-crate manifest to describe a set of files connected to a organization (e.g. a University) and a list of researchers (identified by Swamid/eduGAIN identifiers).

The manifest for describing the storage manifest and files is based on [ro-crate 1.1](https://www.researchobject.org/ro-crate/1.1/).

### Json schema for the manifest

[json schema](schema.json) - [documentation](https://snd-sweden.github.io/data-storage-information-interface/docs/schema/)

### Example implementations

[Node.js command line tool](https://github.com/snd-sweden/data-storage-information-interface/tree/master/scripts/manifest-generators/nodejs)

[C# command line tool](https://github.com/snd-sweden/data-storage-information-interface/tree/master/scripts/manifest-generators/csharp)

### Example manifest

[Simple manifest with two files](ro-crate-metadata.example.json)

## Exmaple for posting a manifest

To get access to the staging environment for handling the server contact SND.

1. Generate ro-crate-metadata.json or use [ro-crate-metadata.example.json](ro-crate-metadata.example.json)
2. curl -X POST https://example-index-server.se/ro-crate -H 'Content-Type: application/json' -d @'ro-crate-metadata.json'
3. For updates post the updated manifest (the identifier in the manifest needs to be identical)
4. Your files should be listed in DORIS (staging)
