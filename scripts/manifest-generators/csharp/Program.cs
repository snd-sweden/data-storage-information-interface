using HeyRed.Mime;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

if (Environment.GetCommandLineArgs().Length == 1)
{
    Console.WriteLine("Provide argument for directory");
    Environment.Exit(0);
}

var directory = new DirectoryInfo(Environment.GetCommandLineArgs()[1]);

if (!directory.Exists)
{
    Console.WriteLine("The directory specified could not be found");
    Environment.Exit(0);
}

var metadataFileDescriptor = new JsonObject
{
    ["@type"] = "CreativeWork",
    ["@id"] = "ro-crate-metadata.json",
    ["identifier"] = "38d0324a-9fa6-11ec-b909-0242ac120002", //must be persistent
    ["conformsTo"] = new JsonObject
    {
        ["@id"] = "https://w3id.org/ro/crate/1.1"
    },
    ["about"] = new JsonObject
    {
        ["@id"] = "./"
    },
    ["publisher"] = new JsonObject
    {
        ["@type"] = "Organization",
        ["@id"] = "https://ror.org/01tm6cn81", //optional: from config/external source
        ["identifier"] = new JsonArray
        {
            new JsonObject
            {
                ["@type"] = "PropertyValue",
                ["propertyID"] = "domain",
                ["propertyValue"] = "gu.se" //required: from config/external source
            }
        },
    },
    ["creator"] = new JsonArray
    {
        new JsonObject
        {
            ["@type"] = "Person",
            ["@id"] = "https://orcid.org/0000-0003-4908-2169", //optional: from config/external source
            ["email"] = "example@gu.se",
            ["identifier"] = new JsonArray
            {
                new JsonObject
                {
                    ["@type"] = "PropertyValue",
                    ["propertyID"] = "eduPersonPrincipalName",
                    ["propertyValue"] = "xkalle@gu.se" //required: from config/external source
                }
            }
        }
    }
};

var hasPart = new JsonArray();
var files = new JsonArray();

using var sha256 = SHA256.Create();
foreach (var fInfo in directory.EnumerateFiles("*", SearchOption.AllDirectories))
{
    try
    {
        using var fileStream = fInfo.OpenRead();
        byte[] hashValue = sha256.ComputeHash(fileStream);

        string id = Path.GetRelativePath(directory.FullName, fInfo.FullName).Replace(Path.DirectorySeparatorChar, '/');

        hasPart.Add(new JsonObject
        {
            ["@id"] = id
        });

        files.Add(new JsonObject
        {
            ["@type"] = "File",
            ["@id"] = id,
            ["sha256"] = Convert.ToHexString(hashValue),
            ["contentSize"] = fInfo.Length,
            ["encodingFormat"] = MimeTypesMap.GetMimeType(fInfo.Name),
            ["dateCreated"] = fInfo.CreationTimeUtc.ToString("o"),
            ["dateModified"] = fInfo.LastWriteTimeUtc.ToString("o"),
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

var rootDataEntity = new JsonObject
{
    ["@type"] = "Dataset",
    ["@id"] = "./",
    ["hasPart"] = hasPart
};

var roCrate = new JsonObject
{
    ["@context"] = "https://w3id.org/ro/crate/1.1/context",
    ["@graph"] = new JsonArray
    {
        metadataFileDescriptor,
        rootDataEntity,
        files
    }
};

Console.Write(roCrate.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));