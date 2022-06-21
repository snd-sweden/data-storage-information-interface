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

var graph = new JsonArray();

graph.Add(new JsonObject
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
        ["@id"] = "https://ror.org/01tm6cn81", //Reference to Organization (could also be a local id)
    },
    ["creator"] = new JsonArray
    {
        new JsonObject
        {
            ["@id"] = "https://orcid.org/0000-0003-4908-2169", //reference to person object, (could also be a local id)
        }
    }
});

graph.Add(new JsonObject
{
    ["@type"] = "Organization",
    ["@id"] = "https://ror.org/01tm6cn81", //using ror-id is optional. Use value from config/external source, could also be local id
    ["identifier"] = new JsonArray
    {
        new JsonObject
        {
            ["@id"] = "#domain-0" //reference to PropertyValue holding organisation domain
        }
    },
});

graph.Add(new JsonObject
{
    ["@type"] = "PropertyValue",
    ["@id"] = "#domain-0",
    ["propertyID"] = "domain",
    ["value"] = "gu.se" //required: from config/external source
});


graph.Add(new JsonObject
{
    ["@type"] = "Person",
    ["@id"] = "https://orcid.org/0000-0003-4908-2169", //optional: from config/external source (could also be a local id)
    ["email"] = "example@gu.se", //optional propery
    ["identifier"] = new JsonArray
    {
        new JsonObject
        {
            ["@id"] = "#eduPersonPrincipalName-0" //reference to PropertyValue holding edugain id
        }
    }
});

graph.Add(new JsonObject
{
    ["@type"] = "PropertyValue",
    ["@id"] = "#eduPersonPrincipalName-0",
    ["propertyID"] = "eduPersonPrincipalName",
    ["value"] = "xkalle@gu.se" //required: from config/external source
});


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

        graph.Add(new JsonObject
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

graph.Add(new JsonObject
{
    ["@type"] = "Dataset",
    ["@id"] = "./",
    ["hasPart"] = hasPart
});

var roCrate = new JsonObject
{
    ["@context"] = "https://w3id.org/ro/crate/1.1/context",
    ["@graph"] = graph
};

Console.Write(roCrate.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));