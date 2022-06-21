## About the storage manifest
The purpose of this manifest is to integrate a local research storage with SND DORIS so that the researchers can easily describe, refer to and publish data without data having to leave the organisation's own storage.

The goal is to have as few required fields as possible and focus on using a RO-Crate manifest to describe a set of files connected to an organisation (e.g. a university) and a list of researchers (identified by Swamid/eduGAIN identifiers).

The manifest for describing the storage manifest and files is based on [RO-Crate 1.1](https://www.researchobject.org/ro-crate/1.1/).

### JSON schema for the manifest

[JSON schema](schema.json) - [documentation](https://snd-sweden.github.io/data-storage-information-interface/docs/schema/)

### Example implementations

[Node.js command line tool](https://github.com/snd-sweden/data-storage-information-interface/tree/master/scripts/manifest-generators/nodejs)

[C# command line tool](https://github.com/snd-sweden/data-storage-information-interface/tree/master/scripts/manifest-generators/csharp)

[Java command line tool](https://github.com/snd-sweden/data-storage-information-interface/tree/master/scripts/manifest-generators/java)

[PHP Nextcloud extension example](https://github.com/snd-sweden/data-storage-information-interface/tree/master/scripts/manifest-generators/nextcloud)

## Properties overview

This is an overview of the properties supported by SND's RO-Crate profile.

* @type [CreativeWork](https://schema.org/CreativeWork)
* @id `ro-crate-metadata.json` *reference to the manifest, [must be ro-crate-metadata.json](https://www.researchobject.org/ro-crate/1.1/root-data-entity.html#ro-crate-metadata-file-descriptor)*
* [conformsTo](https://www.dublincore.org/specifications/dublin-core/dcmi-terms/#conformsTo)
   * @id `https://w3id.org/ro/crate/1.1`
* [identifier](https://schema.org/identifier) `04679b46-964c-11ec-b909-0242ac120002` *UUID for the manifest*
* [publisher](https://schema.org/publisher) *organisation responsible for the dataset*
   * @type [Organization](https://schema.org/Organization)
   * [identifier](https://schema.org/identifier)
      * @type [PropertyValue](https://schema.org/PropertyValue)
      * [propertyID](https://schema.org/propertyID) `domain`
      * [value](https://schema.org/value) `example.com` *organisation's top domain*
* [creator](https://schema.org/creator) *one or more "owners" of the files described in the manifest*
   * @type [Person](https://schema.org/Person)
   * [identifier](https://schema.org/identifier)
      * @type [PropertyValue](https://schema.org/PropertyValue)
      * [propertyID](https://schema.org/propertyID) `eduPersonPrincipalName`
      * [value](https://schema.org/value) `localid@example.com` *person Swamid/eduGAIN id*
* [about](https://schema.org/about)
   * @type [Dataset](https://schema.org/Dataset)
   * [name](https://schema.org/name) *optional: provide a name for the dataset*
   * [hasPart](https://schema.org/hasPart)
      * @type [File](https://schema.org/File)
      * @id `example-file.csv` *relative path from dataset root*
      * [contentSize](https://schema.org/contentSize) *optional: size in bytes*
      * [dateCreated](https://schema.org/dateCreated) *optional: created date (ISO 8601)*
      * [dateModified](https://schema.org/dateModified) *optional: modified date (ISO 8601)*
      * [encodingFormat](https://schema.org/encodingFormat) *optional: MIME format*
      * [sha256](https://schema.org/sha256) *optional: file checksum (SHA256)*
      * [url](https://schema.org/url) `https://example.com/04679b46-964c-11ec-b909-0242ac120002/example-file.csv` *optional: public URL to file*


### Example manifest

[Simple manifest with two files](ro-crate-metadata.example.json)

__Note:__ RO-Crate manifests must be expressed as a JSON-LD flattened graph, nested properties are not allowed.
This means references must be expressed with local id:s, for example `Person.identifier` to `PropertyValue` to reference the value for `eduPersonPrincipalName` (see example manifest and example scripts).

## Example for posting a manifest

To get access to the staging environment for handling the server contact SND.

1. Generate ro-crate-metadata.json or use [ro-crate-metadata.example.json](ro-crate-metadata.example.json)
2. curl -X POST https://example-index-server.se/ro-crate -H 'Content-Type: application/json' -d @'ro-crate-metadata.json'
3. For updates post the updated manifest (the identifier in the manifest needs to be identical)
4. Your files should be listed in DORIS (staging)
