using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace Deduplicator
{
    internal class App
    {
        public void Start()
        {
            // Vstupní a výstupní adresáře
            string inputDirectory = "C:\\Users\\jjiri\\Downloads\\yaml-deduplikace.tar\\multiselect";
            string outputDirectory = "C:\\Users\\jjiri\\Downloads\\yaml-deduplikace.tar\\multiselect\\output";

            // Projít všechny soubory vstupního adresáře
            foreach (var filePath in Directory.GetFiles(inputDirectory))
            {
                string extension = Path.GetExtension(filePath).ToLower();

                // Pokud je to JSON nebo YAML soubor, provedeme deduplikaci
                if (extension == ".json")
                {
                    ProcessJsonFile(filePath, outputDirectory);
                }
                else if (extension == ".yaml" || extension == ".yml")
                {
                    ProcessYamlFile(filePath, outputDirectory);
                }
            }

            Console.WriteLine("Deduplikace dokončena.");
        }

        static void ProcessJsonFile(string filePath, string outputDirectory)
        {
            string jsonString = File.ReadAllText(filePath);
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            DeduplicateAndWriteJson(jsonObject, filePath, outputDirectory);
        }

        static void ProcessYamlFile(string filePath, string outputDirectory)
        {
            string yamlString = File.ReadAllText(filePath);
            var yamlObject = ConvertYamlToObject(yamlString);
            DeduplicateAndWriteYaml(yamlObject, filePath, outputDirectory);
        }

        static void DeduplicateAndWriteJson(Dictionary<string, object> jsonObject, string originalFilePath, string outputDirectory)
        {
            // Deduplication logic for JSON
            var deduplicatedObject = DeduplicateJsonObject(jsonObject);

            // Generujeme unikátní název souboru pro výstup
            string outputFilePath = Path.Combine(outputDirectory, GetUniqueFileName(originalFilePath, ".json"));

            // Zapišeme deduplikovaný JSON do výstupního souboru
            File.WriteAllText(outputFilePath, JsonConvert.SerializeObject(deduplicatedObject, Newtonsoft.Json.Formatting.Indented));

            Console.WriteLine($"Soubor {originalFilePath} byl zpracován a uložen jako {outputFilePath}.");
        }

        static void DeduplicateAndWriteYaml(object yamlObject, string originalFilePath, string outputDirectory)
        {
            
            var deduplicatedObject = DeduplicateYamlObject(yamlObject);

            // make unique name for output
            string outputFilePath = Path.Combine(outputDirectory, GetUniqueFileName(originalFilePath, ".yaml"));

            // write deduplicated yaml to file
            File.WriteAllText(outputFilePath, ConvertObjectToYaml(deduplicatedObject));

            Console.WriteLine($"Soubor {originalFilePath} byl zpracován a uložen jako {outputFilePath}.");
        }

        static Dictionary<string, object> DeduplicateJsonObject(Dictionary<string, object> jsonObject)
        {
            var deduplicatedObjects = new Dictionary<string, object>();

            foreach (var kvp in jsonObject)
            {
                if (kvp.Value is Dictionary<string, object> nestedObject)
                {
                  
                    var deduplicatedNestedObject = DeduplicateJsonObject(nestedObject);

                    // Check if the deduplicated object already exists in the dictionary
                    var key = GetObjectKey(deduplicatedNestedObject);
                    if (!deduplicatedObjects.ContainsKey(key))
                    {
                        // If not, add it to the dictionary with a unique identifier
                        var uniqueKey = GetUniqueKey();
                        deduplicatedObjects.Add(uniqueKey, deduplicatedNestedObject);
                    }

                    // Use the unique identifier as a reference to the deduplicated object
                    deduplicatedObjects.Add(kvp.Key, GetObjectReference(deduplicatedNestedObject));
                }
                else
                {
                    
                    deduplicatedObjects.Add(kvp.Key, kvp.Value);
                }
            }

            return deduplicatedObjects;
        }

        static object DeduplicateYamlObject(object yamlObject)
        {
            if (yamlObject is Dictionary<object, object> dict)
            {
                var deduplicatedObjects = new Dictionary<object, object>();

                foreach (var kvp in dict)
                {
                    if (kvp.Value is Dictionary<object, object> nestedObject)
                    {
                       
                        var deduplicatedNestedObject = DeduplicateYamlObject(nestedObject);

                        // Check if the deduplicated object already exists in the dictionary
                        var key = GetObjectKey(deduplicatedNestedObject);
                        if (!deduplicatedObjects.ContainsKey(key))
                        {
                            // If not, add it to the dictionary with a unique identifier
                            var uniqueKey = GetUniqueKey();
                            deduplicatedObjects.Add(uniqueKey, deduplicatedNestedObject);
                        }

                        
                        deduplicatedObjects.Add(kvp.Key, GetObjectReference(deduplicatedNestedObject));
                    }
                    else
                    {
                       
                        deduplicatedObjects.Add(kvp.Key, kvp.Value);
                    }
                }

                return deduplicatedObjects;
            }
            else
            {
                return yamlObject;
            }
        }

        static string GetObjectKey(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        static string GetObjectReference(object obj)
        {
            var key = GetObjectKey(obj);
            return $"!ref {key}";
        }

        static string GetUniqueKey()
        {
            return $"_ref{31}";
        }

        static string GetUniqueFileName(string originalFilePath, string extension)
        {
            // Vygenerujeme unikátní název souboru podle UUID
            string uniqueName = Guid.NewGuid().ToString().Replace("-", "");
            return $"{Path.GetFileNameWithoutExtension(originalFilePath)}_{uniqueName}{extension}";
        }

        static object ConvertYamlToObject(string yamlString)
        {
            var deserializer = new DeserializerBuilder().Build();
            return deserializer.Deserialize(new StringReader(yamlString));
        }

        static string ConvertObjectToYaml(object yamlObject)
        {
            var serializer = new SerializerBuilder().Build();
            return serializer.Serialize(yamlObject);
        }
    }
}