using System.Text.Json;
using System.Text.Json.Serialization;

namespace LabProject.Helpers
{
    public class Utils
    {
        private static Utils? _instance;
        private static readonly object _lock = new object();

        private Utils() { }

        public static Utils Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new Utils();
                    }
                }
                return _instance;
            }
        }

        public string ExportToJson<T>(IEnumerable<T> data, IEnumerable<string>? selectedProperties = null)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            if (selectedProperties != null && selectedProperties.Any())
            {
                var filteredData = data.Select(item =>
                {
                    var dict = new Dictionary<string, object?>();
                    var properties = typeof(T).GetProperties();
                    foreach (var prop in properties)
                    {
                        if (selectedProperties.Contains(prop.Name))
                        {
                            dict[prop.Name] = prop.GetValue(item);
                        }
                    }
                    return dict;
                });

                return JsonSerializer.Serialize(filteredData, options);
            }

            return JsonSerializer.Serialize(data, options);
        }
    }
} 