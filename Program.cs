using System;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        string folderPath = @"FILEPATH"; // Change to your folder path
        string duplicatesFolderPath = Path.Combine(folderPath, "duplicates");

        // Ensure duplicates folder exists
        Directory.CreateDirectory(duplicatesFolderPath);

        // Define the file types to include
        var fileTypes = new List<string> { "*.jpg", "*.png" }; // Add other file types as needed

        var files = fileTypes.SelectMany(ft => Directory.GetFiles(folderPath, ft)).ToList();
        var fileHashes = new Dictionary<string, string>();

        foreach (var file in files)
        {
            var hash = ComputeFileHash(file);
            if (fileHashes.ContainsValue(hash))
            {
                // Move duplicate file
                string destFileName = Path.Combine(duplicatesFolderPath, Path.GetFileName(file));
                if (!File.Exists(destFileName))
                {
                    File.Move(file, destFileName);
                    Console.WriteLine($"Moved duplicate: {file}");
                }
                else
                {
                    // Handle case where a file with the same name already exists in the duplicates folder
                    string uniqueDestFileName = GetUniqueFilename(duplicatesFolderPath, Path.GetFileName(file));
                    File.Move(file, uniqueDestFileName);
                    Console.WriteLine($"Moved duplicate with unique name: {file}");
                }
            }
            else
            {
                fileHashes[file] = hash;
            }
        }

        Console.WriteLine("Process completed.");
    }

    static string ComputeFileHash(string filePath)
    {
        using (var sha256 = SHA256.Create())
        {
            using (var stream = File.OpenRead(filePath))
            {
                var hash = sha256.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    static string GetUniqueFilename(string directory, string fileName)
    {
        string fileWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        string extension = Path.GetExtension(fileName);
        int number = 1;

        // Construct a new filename with a numeric suffix if the file already exists
        string newFileName = fileName;
        while (File.Exists(Path.Combine(directory, newFileName)))
        {
            newFileName = $"{fileWithoutExt} ({number++}){extension}";
        }

        return Path.Combine(directory, newFileName);
    }
}
