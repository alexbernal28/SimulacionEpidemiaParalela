using System.Diagnostics;

namespace Secuencial
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Iniciando Simulación SIRD Secuencia...");

            // Parámetros de la simulación
            int gridSize = 100; // Tamaño de la cuadrícula (100x100)
            double infectionProb = 0.2; // Probabilidad de infección
            double recoveryProb = 0.1; // Probabilidad de recuperación
            double deathProb = 0.02; // Probabilidad de muerte
            double initialInfectedRatio = 0.01; // 1% de la población inicial infectada
            int totalDays = 365; // Número total de días a simular

            var simulacion = new SequentialSIR(gridSize, infectionProb, recoveryProb, deathProb, initialInfectedRatio);

            // Usamos Stopwatch para empezar a medir los tiempos
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int day = 1; day <= totalDays; day++)
            {
                simulacion.SimulateDay();

                // Cada 30 días, imprimimos el estado actual
                if (day % 30 == 0 || day == totalDays)
                {
                    Console.WriteLine($"Día {day} completado.");
                }
            }

            stopwatch.Stop();

            Console.WriteLine("\nResultados Secuenciales...");
            Console.WriteLine($"Tiempo total de simulación: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine("Simulación finalizada.");
        }
    }
}
