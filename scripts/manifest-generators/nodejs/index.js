#!/usr/bin/env node

const crypto = require('crypto');
const fs = require('fs');
const path = require('path');
const { program } = require('commander');
const { v4: uuidv4 } = require('uuid');
var mmm = require('mmmagic');

program
  .description('Generate a RO-Crate manifest of a directory')
  .option('-d, --directory <dir>', 'Directory to generate manifest of', process.cwd())
  .requiredOption('-pd, --publisherDomain <domain>', 'Domain of the organisation that publishes the data')
  .option('-pr, --publisherRor <rorId>', 'ROR ID of the organisation that publishes the data')
  .requiredOption('-c --creatorID <eppn>', 'EPPN (eduPersonPrincipalName) of the creator')
  .option('-o --creatorOrcid <orcid>', "ORCID of the creator");

program.parse();
generate();

async function generate() {
  var directory = program.opts().directory;
  var graph = [];

  graph.push({
    "@type": "CreativeWork",
    "@id": "ro-crate-metadata.json",
    "identifier": uuidv4(), //must be persistent
    "conformsTo": {
      "@id": "https://w3id.org/ro/crate/1.1"
    },
    "about": {
      "@id": "./"
    },
    "publisher": {
      "@type": "Organization",
      "@id":  program.opts().publisherRor, //optional
      "identifier": [
        {
          "@type": "PropertyValue",
          "propertyID": "domain",
          "propertyValue": program.opts().publisherDomain //required
        }
      ]
    },
    "creator": [
      {
        "@type": "Person",
        "@id": program.opts().creatorOrcid, //optional
        "email": "example@gu.se",
        "identifier": [
          {
            "@type": "PropertyValue",
            "propertyID": "eduPersonPrincipalName",
            "propertyValue": program.opts().creatorID //required
          }
        ]
      }
    ]
  });

  var files = await walkDirectory(directory);
  var fileIds = [];

  await Promise.all(files.map(async (file) => {
    const relPath = path.relative(directory, file);

    graph.push({
      "@type": "File",
      "@id": relPath,
      "sha256": await getSha256(file),
      "contentSize": fs.statSync(file).size,
      "dateCreated": fs.statSync(file).ctime,
      "dateModified": fs.statSync(file).mtime,
      "encodingFormat": await getMime(file),
    });

    fileIds.push(relPath);
  }));

  graph.push({
    "@type": "Dataset",
    "@id": "./",
    "name": "Example name",
    "hasPart": fileIds,
  });

  var roCrateJson = {
    "@context": "https://w3id.org/ro/crate/1.1/context",
    "@graph": graph
  };

  console.log(JSON.stringify(roCrateJson, null, 2));
}

function getMime(filePath) {
  const magic = new mmm.Magic(mmm.MAGIC_MIME_TYPE);

  return new Promise((resolve, reject) => {
    magic.detectFile(filePath, (err, result) => {
      if (err) {
        reject
      }
      resolve(result);
    });
  });
}

function getSha256(filePath) {
  return new Promise((resolve, reject) =>{
    const hash = crypto.createHash('sha256');
    const rs = fs.createReadStream(filePath);
    rs.on('error', reject)
    rs.on('data', chunk => hash.update(chunk))
    rs.on('end', () => resolve(hash.digest('hex')))
  });
}

async function walkDirectory(dir, fileList = []) {
  const files = await fs.promises.readdir(dir);
  for (const file of files) {
    const stat = await fs.promises.stat(path.join(dir, file));
    if (stat.isDirectory()) {
      fileList = await walkDirectory(path.join(dir, file), fileList);
    } else {
      fileList.push(path.join(dir, file));
    }
  }

  return fileList
}