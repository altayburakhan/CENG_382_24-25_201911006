using LabProject.Models;
using System.Text.Json;

namespace LabProject.Services
{
    public class DataService
    {
        private readonly string _jsonFilePath;
        private List<ClassInformationTable>? _classes;

        public DataService(IWebHostEnvironment webHostEnvironment)
        {
            _jsonFilePath = Path.Combine(webHostEnvironment.ContentRootPath, "Data", "classes.json");
            
            // Data klasörünü oluştur
            var dataDirectory = Path.GetDirectoryName(_jsonFilePath);
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory!);
            }

            // Eğer JSON dosyası yoksa veya boşsa örnek veri oluştur
            if (!File.Exists(_jsonFilePath) || File.ReadAllText(_jsonFilePath).Trim() == "[]")
            {
                GenerateSampleData();
            }
        }

        private void GenerateSampleData()
        {
            var random = new Random();
            _classes = new List<ClassInformationTable>();

            for (int i = 1; i <= 100; i++)
            {
                _classes.Add(new ClassInformationTable
                {
                    Id = i,
                    ClassName = $"Class {i}",
                    StudentCount = random.Next(1, 100),
                    Description = $"Description for Class {i}"
                });
            }

            SaveClasses();
        }

        public List<ClassInformationTable> GetClasses()
        {
            if (_classes == null)
            {
                try
                {
                    if (!File.Exists(_jsonFilePath))
                    {
                        Console.WriteLine($"Classes data file not found at {_jsonFilePath}. Creating new file.");
                        _classes = new List<ClassInformationTable>();
                        SaveClasses(); // Boş listeyi kaydet
                        return _classes;
                    }

                    var json = File.ReadAllText(_jsonFilePath);
                    _classes = JsonSerializer.Deserialize<List<ClassInformationTable>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<ClassInformationTable>();
                }
                catch (JsonException jsonEx)
                {
                    Console.WriteLine($"Error deserializing classes data: {jsonEx.Message}");
                    _classes = new List<ClassInformationTable>();
                }
                catch (IOException ioEx)
                {
                    Console.WriteLine($"Error reading classes data file: {ioEx.Message}");
                    _classes = new List<ClassInformationTable>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                    _classes = new List<ClassInformationTable>();
                }
            }
            return _classes;
        }

        public void SaveClasses()
        {
            try
            {
                var json = JsonSerializer.Serialize(_classes, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(_jsonFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving classes data: {ex.Message}");
            }
        }

        public void AddClass(ClassInformationTable newClass)
        {
            var classes = GetClasses();
            newClass.Id = classes.Count > 0 ? classes.Max(c => c.Id) + 1 : 1;
            classes.Add(newClass);
            SaveClasses();
        }

        public void UpdateClass(ClassInformationTable updatedClass)
        {
            var classes = GetClasses();
            var existingClass = classes.FirstOrDefault(c => c.Id == updatedClass.Id);
            if (existingClass != null)
            {
                existingClass.ClassName = updatedClass.ClassName;
                existingClass.StudentCount = updatedClass.StudentCount;
                existingClass.Description = updatedClass.Description;
                SaveClasses();
            }
        }

        public void DeleteClass(int id)
        {
            var classes = GetClasses();
            var classToRemove = classes.FirstOrDefault(c => c.Id == id);
            if (classToRemove != null)
            {
                classes.Remove(classToRemove);
                SaveClasses();
            }
        }
    }
} 