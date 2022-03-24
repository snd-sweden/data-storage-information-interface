using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using System.Security.Cryptography;
using HeyRed.Mime;

if(Environment.GetCommandLineArgs().Length == 1){
    Console.WriteLine("Provide argument for directory");
    Environment.Exit(0);
}

string directory = Environment.GetCommandLineArgs()[1];

directory = Path.GetFullPath(directory);

if (!Directory.Exists(directory))
{
    Console.WriteLine("The directory specified could not be found.");
    Environment.Exit(0);
}

var graph = new List<JsonNode>();

graph.Add(new JsonObject {
    ["@type"] = "CreativeWork",
    ["@id"] = "ro-crate-metadata.json",
    ["identifier"] = "38d0324a-9fa6-11ec-b909-0242ac120002", //must be persistent
    ["conformsTo"] = new JsonObject {
        ["@id"] = "https://w3id.org/ro/crate/1.1"
    },
    ["about"] = new JsonObject {
        ["@id"] = "./"
    },
    ["publisher"] = new JsonObject {
        ["@type"] = "Organization",
        ["@id"] = "https://ror.org/01tm6cn81", //optional: from config/external source
        ["identifier"] = new JsonArray(
            new JsonObject {
                ["@type"] = "PropertyValue",
                ["propertyID"] = "domain",
                ["propertyValue"] = "gu.se" //required: from config/external source
            }
        )
    },
    ["creator"] = new JsonArray(
        new JsonObject {
            ["@type"] = "Person",
            ["@id"] = "https://orcid.org/0000-0003-4908-2169", //optional: from config/external source
            ["email"] = "example@gu.se",
            ["identifier"] = new JsonArray(
                new JsonObject {
                    ["@type"] = "PropertyValue",
                    ["propertyID"] = "eduPersonPrincipalName",
                    ["propertyValue"] = "xkalle@gu.se" //required: from config/external source
                }
            )
        }
    )
});

var dir = new DirectoryInfo(directory);
FileInfo[] files = dir.GetFiles("*", SearchOption.AllDirectories);

var fileIds = new List<JsonNode>();
foreach (FileInfo fInfo in files){
    fileIds.Add(new JsonObject{
        ["@id"] = fInfo.FullName.Replace(directory, "")
    });
}

graph.Add(new JsonObject {
    ["@type"] = "Dataset",
    ["@id"] = "./",
    ["hasPart"] = new JsonArray(fileIds.ToArray())
});

using (SHA256 sha256 = SHA256.Create())
{
    foreach (FileInfo fInfo in files)
    {
        using (FileStream fileStream = fInfo.Open(FileMode.Open))
        {
            try
            {
                fileStream.Position = 0;
                byte[] hashValue = sha256.ComputeHash(fileStream);

                graph.Add(new JsonObject {
                    ["@type"] = "File",
                    ["@id"] = fInfo.FullName.Replace(directory, ""),
                    ["sha256"] = byteToString(hashValue),
                    ["contentSize"] = fInfo.Length,
                    ["encodingFormat"] = MimeTypesMap.GetMimeType(fInfo.Name),
                    ["dateCreated"] = File.GetCreationTime(fInfo.FullName).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.sssZ"),
                    ["dateModified"] = File.GetLastWriteTime(fInfo.FullName).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.sssZ"),
                });
            }
            catch (IOException e)
            {
                Console.WriteLine($"I/O Exception: {e.Message}");
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine($"Access Exception: {e.Message}");
            }
        }
    }
}

var roCrateJson = new JsonObject {
    ["@context"] = "https://w3id.org/ro/crate/1.1/context",
    ["@graph"] = new JsonArray(graph.ToArray())
};

Console.Write(roCrateJson.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

string byteToString(byte[] array)
{
    string result = "";
    for (int i = 0; i < array.Length; i++)
    {
        result += $"{array[i]:X2}";
    }
    return result.ToLower();
}