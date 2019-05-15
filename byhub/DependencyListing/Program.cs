using System;
using System.Collections.Generic;
using System.IO;

namespace DependencyListing
{
    class Program
    {

        static class Globals
        {
            public static List<Dependency> CompleteDependencyList{get; set;}
        }

        static void Main(string[] args)
        {           
            string[] directories = new string[1];

            directories[0] = @"C:\Users\rapha\Downloads\dependencies";

            Globals.CompleteDependencyList = new List<Dependency>();

            foreach(string path in directories) 
            {
                if(File.Exists(path)) 
                {
                    // This path is a file
                    Globals.CompleteDependencyList.AddRange(ProcessFile(path)); 
                }               
                else if(Directory.Exists(path)) 
                {
                    // This path is a directory
                    ProcessDirectory(path);
                }
                else 
                {
                    Console.WriteLine("{0} is not a valid file or directory.", path);
                }        
            } 
            PrintOnScreen();

            CalculateWeight();

            WriteToFile();
        }

        // Process all files in the directory passed in, recurse on any directories 
        // that are found, and process the files they contain.
        
        public static void ProcessDirectory(string targetDirectory) 
        {
            // Process the list of files found in the directory.
            string [] fileEntries = Directory.GetFiles(targetDirectory);
            foreach(string fileName in fileEntries)
                Globals.CompleteDependencyList.AddRange(ProcessFile(fileName)); 

            // Recurse into subdirectories of this directory.
            string [] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach(string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory);
        }
        
    // Insert logic for processing found files here.
        public static List<Dependency> ProcessFile(string path) 
        {
            Console.WriteLine("#####################################--Processed file '{0}'.", path);
            int counter = 0;
            string line;
            string _parent = "";

            List<Dependency> DependencyList = new List<Dependency>();

            // Read the file and display it line by line.  
            System.IO.StreamReader file =   
                new System.IO.StreamReader(path);  
            while((line = file.ReadLine()) != null)  
            {  
                if(!line.StartsWith(@"  Depends: "))
                {
                    _parent = line;
                    
                    // Console.WriteLine(line);
                }
                
                if(line.StartsWith(@"  Depends: "))
                {
                    string _name = line.Replace(@"  Depends: ", "");
                    string[] _dep = _name.Split('(');
                    Dependency _dependency = new Dependency();
                    
                    _name = _dep[0];
                    if(_dep.Length>1)
                    {
                        string _version = _dep[1].Replace(")", "");
                        _dependency.Version = _version;
                    }
                    else _dependency.Version = "any";
                    _dependency.File = path;
                    _dependency.Parent = _parent;
                    _dependency.Name = _name;
                    
                    DependencyList.Add(_dependency);
                }
                
                //System.Console.WriteLine(line);  
                counter++;  
            }  
            return DependencyList;
        }


        public static void CalculateWeight()
        {
            string oldParent = "";
            int count = 0;

            List<Dependency> newCompleteDependencyList = new List<Dependency>();
            newCompleteDependencyList = Globals.CompleteDependencyList;

            foreach(Dependency _dependency in Globals.CompleteDependencyList)
            {
                string newParent = _dependency.Parent;

                if(!newParent.Equals(oldParent))
                {
                    newCompleteDependencyList.FindAll(dep => dep.Parent.Equals(oldParent)).ForEach(dep => dep.Weight = count);
                    oldParent = newParent;
                    count = 0;
                }
                count++;
            }

            Globals.CompleteDependencyList = newCompleteDependencyList;


        }
        public static void PrintOnScreen()
        {
            string oldFile = "";
            string oldParent = "";

            foreach(Dependency _dependency in Globals.CompleteDependencyList)
            {
                string newParent = _dependency.Parent;
                string newFile = _dependency.File;

                if(!newFile.Equals(oldFile))
                {
                    oldFile = newFile;
                    Console.WriteLine("File: '{0}'.", _dependency.File);
                }
                if(!newParent.Equals(oldParent))
                {
                Console.WriteLine("Parent: '{0}'.", _dependency.Parent);
                    oldParent = newParent;
                }
                Console.WriteLine("-->: '{0}'.", _dependency.Name);
                Console.WriteLine("         #: '{0}'.", _dependency.Version);
            }
        }

        public static void WriteToFile()
        {
            string path = @"C:\Users\rapha\Downloads\dependencies\result.csv";
            string oldFile = "";
            string oldParent = "";

            foreach(Dependency _dependency in Globals.CompleteDependencyList)
            {
                string line = "";
                string newParent = _dependency.Parent;
                string newFile = _dependency.File;

                if(!newFile.Equals(oldFile))
                {
                    oldFile = newFile;
                }
                if(!newParent.Equals(oldParent))
                {
                    // Console.WriteLine("Parent: '{0}'.", _dependency.Parent);
                    oldParent = newParent;
                }
                line = line + oldFile + ",";
                line = line + oldParent + ",";
                line = line + _dependency.Name + ",";
                line = line + _dependency.Version + ",";
                line = line + _dependency.Weight;

                if (!File.Exists(path)) 
                {
                   // Create a file to write to.
                    using (StreamWriter sw = File.CreateText(path)) 
                    {
                        sw.WriteLine(line);
                    }	
                }

                // This text is always added, making the file longer over time
                // if it is not deleted.
                else
                {
                    using (StreamWriter sw = File.AppendText(path)) 
                    {
                        sw.WriteLine(line);
                    }	
                }
            }
        }
    }
}
