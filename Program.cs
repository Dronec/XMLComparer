using System.Xml.Linq;

namespace XmlComparer
{
    public class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: XmlComparer.exe <file1> <file2> [Options]");
                Console.WriteLine(
                    "Options: -at (to compare attributes) -av (to compare attributes and their values) -val (to compare node values)");
                return;
            }

            var file1Path = args[0];
            var file2Path = args[1];

            var includeAttributes = args.Contains("-at");
            var includeAttributeValues = args.Contains("-av");
            var includeValues = args.Contains("-val");

            if (!File.Exists(file1Path) || !File.Exists(file2Path))
            {
                Console.WriteLine("One or both of the input files do not exist.");
                return;
            }

            var doc1 = XDocument.Load(file1Path);
            if (doc1.Root is null)
                throw new Exception($"{file1Path} is not an XML");
            var doc2 = XDocument.Load(file2Path);
            if (doc2.Root is null)
                throw new Exception($"{file2Path} is not an XML");


            var nodeCompare1 = GetAllElementPaths(doc1.Root, includeAttributes, includeAttributeValues, includeValues);
            var nodeCompare2 = GetAllElementPaths(doc2.Root, includeAttributes, includeAttributeValues, includeValues);
            Console.ForegroundColor = ConsoleColor.Yellow;
            var diff1 = nodeCompare1.Except(nodeCompare2).ToArray();
            if (diff1.Any())
            {
                Console.WriteLine($"Data only in {file1Path}:");
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var node in diff1)
                {
                    Console.WriteLine(node);
                }
            }
            var diff2 = nodeCompare2.Except(nodeCompare1).ToArray();
            if (diff2.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n\nData only in {file2Path}:");
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var node in nodeCompare2.Except(nodeCompare1))
                {
                    Console.WriteLine(node);
                }
            }
            Console.ResetColor();
        }

        public static List<string> GetAllElementPaths(XElement element, bool includeAttributes = false,
            bool includeAttributeValues = false, bool includeValues = false)
        {
            var paths = new List<string>
            {
                GetElementPath(element, includeAttributes, includeAttributeValues,includeValues)
            };

            foreach (var child in element.Elements())
            {
                paths.AddRange(GetAllElementPaths(child, includeAttributes, includeAttributeValues, includeValues));
            }

            return paths;
        }

        public static string GetElementPath(XElement element, bool includeAttributes, bool includeAttributeValues, bool includeValue)
        {
            var pathSegments = new List<string>();

            var currentElement = element;
            while (currentElement != null)
            {
                pathSegments.Add(string.Concat(currentElement.Name.LocalName,
                    includeAttributes ? ":" + GetElementAttributes(currentElement, includeAttributeValues) : "", includeValue && !currentElement.HasElements ? "/" + currentElement.Value : ""));
                currentElement = currentElement.Parent;
            }

            pathSegments.Reverse();

            return string.Join("/", pathSegments);
        }

        public static string GetElementAttributes(XElement element, bool includeValue = false) =>
            string.Join("*",
                element.Attributes().Select(a => string.Concat(a.Name, includeValue ? $":{a.Value}" : string.Empty)));

    }
}
