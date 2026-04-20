using System.Diagnostics;
using System.Text;

namespace Paralelo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Iniciando generación de datos para la animación...");

            // 1. Creamos la carpeta (y usamos el truco para que te diga exactamente dónde quedó)
            Directory.CreateDirectory("Frames");
            Console.WriteLine($"\n--> LA CARPETA FRAMES SE ESTÁ CREANDO AQUÍ: {Path.GetFullPath("Frames")}\n");

            // 2. Configuramos una simulación más pequeña (200x200) para el video
            int videoGridSize = 200;
            var simVideo = new ParallelSIR(videoGridSize, 0.2, 0.1, 0.02, 0.01, 4);

            // 3. Corremos el bucle de 100 días guardando cada archivo
            for (int day = 1; day <= 100; day++)
            {
                simVideo.SimulateDay(day);

                // Guarda cada día como un archivo txt (ej. dia_1.txt, dia_2.txt)
                simVideo.ExportGridToFile($"Frames/dia_{day}.txt");

                // Imprimimos un mensaje cada 10 días para ver que no se haya trabado
                if (day % 10 == 0)
                {
                    Console.WriteLine($"Exportando día {day} de 100...");
                }
            }

            Console.WriteLine("\n¡Datos para el video generados con éxito!");
            Console.WriteLine("Ahora puedes ir a Python para correr el script visual.");

            // Generrar los datos para la animacion (GIF)
            GenerarDatosAnimacion();

        }

        static void ExportarCSV(int[] cores, long[] times, double[] speedups)
        {
            StringBuilder csv = new StringBuilder();
            csv.AppendLine("Cores,Tiempo_ms,Speedup");

            for (int i = 0; i < cores.Length; i++)
            {
                // Usamos punto como separador decimal para evitar problemas en Excel
                string speedupStr = speedups[i].ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                csv.AppendLine($"{cores[i]},{times[i]},{speedupStr}");
            }

            string filePath = "Resultados_Scaling.csv";
            File.WriteAllText(filePath, csv.ToString());

            Console.WriteLine($"\n¡Archivo '{filePath}' generado con éxito!");
            Console.WriteLine($"Puedes encontrarlo en: {Path.GetFullPath(filePath)}");
        }

        static void GenerarDatosAnimacion()
        {
            Console.WriteLine("Iniciando generación de datos para la animación...");

            // 1. Creamos la carpeta (y usamos el truco para que te diga exactamente dónde quedó)
            Directory.CreateDirectory("Frames");
            Console.WriteLine($"\n--> LA CARPETA FRAMES SE ESTÁ CREANDO AQUÍ: {Path.GetFullPath("Frames")}\n");

            // 2. Configuramos una simulación más pequeña (200x200) para el video
            int videoGridSize = 200;
            var simVideo = new ParallelSIR(videoGridSize, 0.2, 0.1, 0.02, 0.01, 4);

            // 3. Corremos el bucle de 100 días guardando cada archivo
            for (int day = 1; day <= 100; day++)
            {
                simVideo.SimulateDay(day);

                // Guarda cada día como un archivo txt (ej. dia_1.txt, dia_2.txt)
                simVideo.ExportGridToFile($"Frames/dia_{day}.txt");

                // Imprimimos un mensaje cada 10 días para ver que no se haya trabado
                if (day % 10 == 0)
                {
                    Console.WriteLine($"Exportando día {day} de 100...");
                }
            }

            Console.WriteLine("\n¡Datos para el video generados con éxito!");
            Console.WriteLine("Ahora puedes ir a Python para correr el script visual.");
        }
    }
}
