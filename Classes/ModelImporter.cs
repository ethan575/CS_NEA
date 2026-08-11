using OpenTK.Mathematics;

namespace _3D_Engine.Classes
{
    public static class ModelImporter
    {
        // not working yet
        // need to figure out to use f and read which vertices are used in the faces // fixed using stl

        public static float[] LoadModel(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file {filePath} does not exist.");
            }
            else if (Path.GetExtension(filePath).ToLower() != ".stl")
            {
                throw new NotSupportedException($"The format {Path.GetExtension(filePath)} is not supported. Only .stl files are supported.");
            }

            // execute if no errors
            List<float> vertices = new List<float>();
            // temp to track order of triangles of vertices and normals
            List<float> tempVertices = new List<float>();
            float nx = 0, ny = 0, nz = 0; // normal vector components

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    //Console.WriteLine(line);
                    if (line.TrimStart().StartsWith("vertex"))
                    {
                        // Vertex position
                        string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                        tempVertices.Add(float.Parse(parts[1]));
                        tempVertices.Add(float.Parse(parts[2]));
                        tempVertices.Add(float.Parse(parts[3]));

                    }
                    else if (line.TrimStart().StartsWith("facet normal"))
                    {
                        // Normal vector
                        // each normal must be passed 3 times for each vertex in the triangle
                        // this is only what opengl handles
                        string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                       
                        nx = float.Parse(parts[2]);
                        ny = float.Parse(parts[3]);
                        nz = float.Parse(parts[4]);

                    }

                    else if (line.TrimStart().StartsWith("endloop"))
                    {
                        // exactly 3 vertices
                        for (int i = 0; i < 9; i += 3)
                        {
                            // position
                            vertices.Add(tempVertices[i]);
                            vertices.Add(tempVertices[i + 1]);
                            vertices.Add(tempVertices[i + 2]);

                            // normal same for all 3 vertices
                            vertices.Add(nx);
                            vertices.Add(ny);
                            vertices.Add(nz);
                        }

                        //// clear temp lists for the next triangle
                        tempVertices.Clear();
                        nx = ny = nz = 0;
                    }
                    //for (int i = 0; i < vertices.Count; i++)
                    //{
                    //    Console.Write(vertices[i] + " ");
                    //    if (i % 3 == 2)
                    //    {
                    //        Console.WriteLine();
                    //    }
                    //}
                }

                return vertices.ToArray();

            }
        }

        public static Vector3[] LoadNormals(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file {filePath} does not exist.");
            }
            else if (Path.GetExtension(filePath).ToLower() != ".stl")
            {
                throw new NotSupportedException($"The format {Path.GetExtension(filePath)} is not supported. Only .stl files are supported.");
            }

            // execute if no errors
            List<Vector3> normals = new List<Vector3>();
            float nx = 0, ny = 0, nz = 0; // normal vector components

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.TrimStart().StartsWith("facet normal"))
                    {
                        // Normal vector
                        // each normal must be passed 3 times for each vertex in the triangle
                        // this is only what opengl handles
                        string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                        nx = float.Parse(parts[2]);
                        ny = float.Parse(parts[3]);
                        nz = float.Parse(parts[4]);
                        normals.Add(new Vector3(nx, ny, nz));
                    }

                }
                
                return normals.ToArray();

            }
        }

        public static Vector3[] LoadOnlyVertices(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file {filePath} does not exist.");
            }
            else if (Path.GetExtension(filePath).ToLower() != ".stl")
            {
                throw new NotSupportedException($"The format {Path.GetExtension(filePath)} is not supported. Only .stl files are supported.");
            }

            // execute if no errors
            List<Vector3> vertices = new List<Vector3>();
            // temp to track order of triangles of vertices and normals
            List<float> tempVertices = new List<float>();


            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    //Console.WriteLine(line);
                    if (line.TrimStart().StartsWith("vertex"))
                    {
                        // Vertex position
                        string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                        tempVertices.Add(float.Parse(parts[1]));
                        tempVertices.Add(float.Parse(parts[2]));
                        tempVertices.Add(float.Parse(parts[3]));

                    }
                    
                    else if (line.TrimStart().StartsWith("endloop"))
                    {
                        // exactly 3 vertices added
                        for (int i = 0; i < 9; i += 3)
                        {
                            // position
                            vertices.Add(new Vector3(tempVertices[i], tempVertices[i+1], tempVertices[i+2]));

                        }

                        //// clear temp lists for the next triangle
                        tempVertices.Clear();
                    }
                }

                return vertices.ToArray();

            }
        }
    }
}
