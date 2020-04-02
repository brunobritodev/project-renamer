using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Renamer
{
    class Program
    {
        static void Main(string[] args)
        {

            //if (args == null || args[0] == "-?")
            //    PrintHelp();

            var directory = @"D:\workspace\JPProject.IdentityServer4.SSO\tests";
            var exceptions = ".git;.vs;.gitignore;.gitattributes;node_module";
            var replace = "JPProject.Admin.Infra.Data";
            var target = "JPProject.Admin.EntityFramework.Repository";

            //if (args[0].ToLower() == "--rootpath")
            //{
            //    directory = args[1];
            //    replace = args[2];
            //    target = args[3];
            //}

            //Rename(directory, replace, target, ".git", ".vs", ".gitignore", ".gitattributes", "node_module");

            var fileWithCommand = Path.Combine(Environment.CurrentDirectory, "gitmv.bat");
            File.Delete(fileWithCommand);
            var parameters = new RenameParameters()
            {
                FileWithCommand = fileWithCommand,
                Replace = replace,
                Target = target,
                FolderExceptions = new List<string>() { ".git", ".github", ".vs", ".gitignore", ".gitattributes", "node_module", "bin", "obj", "Debug" },
                FileExceptions = new List<string>() { ".nuspec", ".dll", ".vs", ".exe", ".dll", ".suo", ".gitignore", ".gitattributes", "node_module", "bin", "obj", "Debug", ".pdb", ".cache", ".json" }
            };

            GenerateGitMv(parameters, directory);
        }

        private static void GenerateGitMv(RenameParameters command, string directory)
        {
            var files = Directory.GetFiles(directory);
            Console.WriteLine($"Found {files.Length} files in {directory}");
            foreach (var file in files)
            {
                if (!command.FileExceptions.Any(a => a.ToUpper().Contains(Path.GetExtension(file).ToUpper())))
                {
                    try
                    {
                        var content = File.ReadAllText(file);
                        if (content.Contains(command.Replace))
                        {
                            var newContent = content.Replace(command.Replace, command.Target);
                            File.WriteAllText(file, newContent, Encoding.Unicode);
                        }

                        if (Path.GetFileName(file).Contains(command.Replace))
                        {
                            File.AppendAllText(command.FileWithCommand, $"git mv \"{file}\" \"{Path.Combine(Path.GetDirectoryName(file), Path.GetFileName(file).Replace(command.Replace, command.Target))}\" {Environment.NewLine}");
                            File.AppendAllText(command.FileWithCommand, $"rm {file}{Environment.NewLine}");
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Trouble with {file}, in {directory}, issue:{e.Message}");
                    }
                }
            }

            var dirs = Directory.GetDirectories(directory);
            foreach (var dir in dirs)
            {
                if (command.FolderExceptions.Any(a => dir.ToUpper().Contains(a.ToUpper())))
                    continue;

                GenerateGitMv(command, dir);
                try
                {
                    var topFolder = dir.Substring(dir.LastIndexOf(Path.DirectorySeparatorChar)).Replace(Path.DirectorySeparatorChar.ToString(), "");
                    if (topFolder.Contains(command.Replace))
                    {
                        var targetDir = dir.Substring(0, dir.LastIndexOf(Path.DirectorySeparatorChar));
                        targetDir = Path.Combine(targetDir, topFolder.Replace(command.Replace, command.Target));
                        File.AppendAllText(command.FileWithCommand, $"git mv \"{dir}\" \"{targetDir}\" {Environment.NewLine}");

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Trouble with {dir}, issue:{e.Message}");
                }
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Usage: [--rootpath pathToSolution] stringToReplace targetString");
            Console.WriteLine(" -? for help");
        }

        public static void Rename(RenameParameters parameters, string directory)
        {
            var files = Directory.GetFiles(directory);
            Console.WriteLine($"Found {files.Length} files in {directory}");
            foreach (var file in files)
            {
                if (Path.GetExtension(file) != ".exe" && Path.GetExtension(file) != ".dll" && Path.GetExtension(file) != ".suo")
                {
                    try
                    {
                        var content = File.ReadAllText(file);
                        if (content.Contains(parameters.Replace))
                        {
                            var newContent = content.Replace(parameters.Replace, parameters.Target);
                            File.WriteAllText(file, newContent, Encoding.Unicode);
                        }
                        if (Path.GetFileName(file).Contains(parameters.Replace))
                            File.Move(file, Path.Combine(Path.GetDirectoryName(file), Path.GetFileName(file).Replace(parameters.Replace, parameters.Target)));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Trouble with {file}, in {directory}, issue:{e.Message}");

                    }
                }

            }

            var dirs = Directory.GetDirectories(directory);
            foreach (var dir in dirs)
            {
                if (parameters.FolderExceptions.Any(a => Path.GetFileName(dir) == a))
                    continue;
                Rename(parameters, dir);
                try
                {
                    var topFolder = dir.Substring(dir.LastIndexOf(Path.DirectorySeparatorChar)).Replace(Path.DirectorySeparatorChar.ToString(), "");
                    if (topFolder.Contains(parameters.Replace))
                    {
                        var targetDir = dir.Substring(0, dir.LastIndexOf(Path.DirectorySeparatorChar));
                        targetDir = Path.Combine(targetDir, topFolder.Replace(parameters.Replace, parameters.Target));
                        if (!Directory.Exists(targetDir))
                            Directory.Move(dir, targetDir);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Trouble with {dir}, issue:{e.Message}");

                }
            }
        }
    }

    internal class RenameParameters
    {
        public string FileWithCommand { get; set; }
        public string Replace { get; set; }
        public string Target { get; set; }
        public IEnumerable<string> FolderExceptions { get; set; }
        public List<string> FileExceptions { get; set; }
    }
}
