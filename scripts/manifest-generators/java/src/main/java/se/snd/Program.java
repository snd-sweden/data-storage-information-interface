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

        var graph = Json.createArrayBuilder();

        graph.add(Json.createObjectBuilder()
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
                .add("@id", "https://ror.org/01tm6cn81") //reference to Organization (could also be a local id)
            ) 
            .add("creator", Json.createArrayBuilder()
                .add(Json.createObjectBuilder()
                    .add("@id", "https://orcid.org/0000-0003-4908-2169") //reference to person object, (could also be a local id)
                )
            )
        );

        graph.add(Json.createObjectBuilder()
            .add("@type", "Organization")
            .add("@id", "https://ror.org/01tm6cn81") //using ROR-id is optional. Use value from config/external source, could also be local id
            .add("identifier", Json.createArrayBuilder()
                .add(Json.createObjectBuilder()
                    .add("@id", "#domain-0") //reference to PropertyValue holding organisation domain
                )
            )
        );

        graph.add(Json.createObjectBuilder()
            .add("@type", "PropertyValue")
            .add("@id", "#domain-0")
            .add("propertyID", "eduPersonPrincipalName")
            .add("value", "xkalle@gu.se")  //required: from config/external source
        );

        graph.add(Json.createObjectBuilder()
            .add("@type", "Person")
            .add("@id", "https://orcid.org/0000-0003-4908-2169") //optional: from config/external source (could also be a local id)
            .add("email", "example@gu.se") //optional propery
            .add("identifier", Json.createArrayBuilder()
                .add(Json.createObjectBuilder()
                    .add("@id", "#eduPersonPrincipalName-0") //reference to PropertyValue holding edugain id
                )
            )
        );

        graph.add(Json.createObjectBuilder()
            .add("@type", "PropertyValue")
            .add("@id", "#eduPersonPrincipalName-0")
            .add("propertyID", "eduPersonPrincipalName")
            .add("value", "xkalle@gu.se")  //required: from config/external source
        );

        var hasPart = Json.createArrayBuilder();
        var tika = new Tika();

        try (var paths = Files.walk(path)) {
            paths.filter(p -> Files.isRegularFile(p)).forEach(p -> {
                try {
                    var attributes = Files.readAttributes(p, BasicFileAttributes.class);       
                    String id = path.relativize(p.toAbsolutePath()).toString().replace("\\", "/");

                    hasPart.add(Json.createObjectBuilder()
                        .add("@id", id)
                    );
                
                    graph.add(Json.createObjectBuilder()
                        .add("@type", "File")
                        .add("@id", id)
                        .add("sha256", DigestUtils.sha256Hex(new FileInputStream(p.toFile())))
                        .add("contentSize", attributes.size())
                        .add("encodingFormat", tika.detect(p))
                        .add("dateCreated", attributes.lastModifiedTime().toString())
                        .add("dateModified", attributes.creationTime().toString())
                    );

                } catch (IOException ex) {
                    ex.printStackTrace();
                }
            });
            
        } catch (IOException ex) {
            ex.printStackTrace();
        }

        graph.add(Json.createObjectBuilder()
            .add("@type", "Dataset")
            .add("@id", "./")
            .add("hasPart", hasPart)
        );

        var roCrate = Json.createObjectBuilder()
            .add("@context", "https://w3id.org/ro/crate/1.1/context")
            .add("@graph", graph)
            .build();
        
        var writerFactory = Json.createWriterFactory(Map.of(JsonGenerator.PRETTY_PRINTING, true));
        var writer = writerFactory.createWriter(System.out);
        writer.writeObject(roCrate);
    }
}
