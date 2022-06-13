package se.snd;

import java.io.FileInputStream;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.nio.file.attribute.BasicFileAttributes;
import java.util.Map;

import javax.json.Json;
import javax.json.stream.JsonGenerator;

import org.apache.commons.codec.digest.DigestUtils;
import org.apache.tika.Tika;

public class Program
{
    public static void main(String[] args)
    {
        if (args.length == 0) {
            System.out.println("Provide argument for directory");
            System.exit(0);
        }

        var path = Paths.get(args[0]).toAbsolutePath();
        if (!Files.exists(path))
        {
            System.out.println("The directory specified could not be found");
            System.exit(0);
        }

        var metadataFileDescriptor = Json.createObjectBuilder()
            .add("@type", "CreativeWork")
            .add("@id", "ro-crate-metadata.json")
            .add("identifier", "38d0324a-9fa6-11ec-b909-0242ac120002") //must be persistent
            .add("conformsTo", Json.createObjectBuilder()
                .add("@id", "https://w3id.org/ro/crate/1.1")
            )
            .add("about", Json.createObjectBuilder()
                .add("@id", "./")
            )
            .add("publisher", Json.createObjectBuilder()
                .add("@type", "Organization")
                .add("@id", "https://ror.org/01tm6cn81") //optional: from config/external source
                .add("identifier", Json.createArrayBuilder()
                    .add(Json.createObjectBuilder()
                        .add("@type", "PropertyValue")
                        .add("propertyID", "domain")
                        .add("propertyValue", "gu.se") //required: from config/external source
                    )
                )
            ) 
            .add("creator", Json.createArrayBuilder()
                .add(Json.createObjectBuilder()
                    .add("@type", "Person")
                    .add("@id", "https://orcid.org/0000-0003-4908-2169") //optional: from config/external source
                    .add("email", "example@gu.se")
                    .add("identifier", Json.createArrayBuilder()
                        .add(Json.createObjectBuilder()
                            .add("@type", "PropertyValue")
                            .add("propertyID", "eduPersonPrincipalName")
                            .add("propertyValue", "xkalle@gu.se")  //required: from config/external source
                        )
                    )
                )
            )
            .build();

        var hasPart = Json.createArrayBuilder();
        var files = Json.createArrayBuilder();
        var tika = new Tika();

        try (var paths = Files.walk(path)) {
            paths.filter(p -> Files.isRegularFile(p)).forEach(p -> {
                try {
                    var attributes = Files.readAttributes(p, BasicFileAttributes.class);       
                    String id = path.relativize(p.toAbsolutePath()).toString().replace("\\", "/");

                    hasPart.add(Json.createObjectBuilder()
                        .add("@id", id)
                        .build()
                    );
                
                    files.add(Json.createObjectBuilder()
                        .add("@type", "File")
                        .add("@id", id)
                        .add("sha256", DigestUtils.sha256Hex(new FileInputStream(p.toFile())))
                        .add("contentSize", attributes.size())
                        .add("encodingFormat", tika.detect(p))
                        .add("dateCreated", attributes.lastModifiedTime().toString())
                        .add("dateModified", attributes.creationTime().toString())
                        .build());

                } catch (IOException ex) {
                    ex.printStackTrace();
                }
            });
            
        } catch (IOException ex) {
            ex.printStackTrace();
        }

        var rootDataEntity = Json.createObjectBuilder()
            .add("@type", "Dataset")
            .add("@id", "./")
            .add("hasPart", hasPart)
            .build();

        var roCrate = Json.createObjectBuilder()
            .add("@context", "https://w3id.org/ro/crate/1.1/context")
            .add("@graph", Json.createArrayBuilder()
                .add(metadataFileDescriptor)
                .add(rootDataEntity)
                .add(files)
            )
            .build();
        
        var writerFactory = Json.createWriterFactory(Map.of(JsonGenerator.PRETTY_PRINTING, true));
        var writer = writerFactory.createWriter(System.out);
        writer.writeObject(roCrate);
    }
}
