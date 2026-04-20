using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Paralelo
{
    public class ParallelSIR
    {
        public readonly int Width;
        public readonly int Height;
        public readonly int TotalPopulation;

        public State[] CurrentGrid;
        private State[] _nextGrid;

        private readonly double _infectionProb;
        private readonly double _recoveryProb;
        private readonly double _deathProb;

        //Nueva variable para controlar el número de hilos (Requisito para Strong Scaling)
        private readonly int _numThreads;

        public ParallelSIR(int size, double infectionProb, double recoveryProb, double deathProb, double initialInfectedRatio, int numThreads)
        {
            Width = size;
            Height = size;
            TotalPopulation = size * size;
            _infectionProb = infectionProb;
            _recoveryProb = recoveryProb;
            _deathProb = deathProb;
            _numThreads = numThreads; // Asignamos el número de hilos

            CurrentGrid = new State[TotalPopulation];
            _nextGrid = new State[TotalPopulation];

            InitializePopulation(initialInfectedRatio);
        }

        private void InitializePopulation(double initialInfectedRatio)
        {
            int initialInfectedCount = (int)(TotalPopulation * initialInfectedRatio);
            Random random = new Random();

            for (int i = 0; i < TotalPopulation; i++)
            {
                CurrentGrid[i] = State.Susceptible;
            }

            int infectedPlaced = 0;
            while (infectedPlaced < initialInfectedCount)
            {
                int randomPos = random.Next(TotalPopulation);
                if (CurrentGrid[randomPos] == State.Susceptible)
                {
                    CurrentGrid[randomPos] = State.Infected;
                    infectedPlaced++;
                }
            }
            Array.Copy(CurrentGrid, _nextGrid, TotalPopulation);
        }

        public DailyStats SimulateDay(int dayNumber)
        {
            // SPLIT EN BLOQUES: Calculamos cuántas filas le tocan a cada hilo
            int rowsPerThread = Height / _numThreads;

            //Variables globales para estadísticas del día
            int globalCurrentInfected = 0;
            int globalNewInfections = 0;
            int globalDead = 0;
            int globalRecovered = 0;

            // Lanzamos los hilos en paralelo. Forzamos a usar exactamente _numThreads
            Parallel.For(0, _numThreads, new ParallelOptions { MaxDegreeOfParallelism = _numThreads }, threadId =>
            {
                // Calculamos los limites de la franja para este hilo
                int startY = threadId * rowsPerThread;

                // Si es el último hilo, le asignamos hasta el final (por si la división no es exacta)
                int endY = (threadId == _numThreads - 1) ? Height : startY + rowsPerThread;

                // SOLUCIÓN A MONTE-CARLO: Generador Random local exclusivo para este hilo
                // Usamos Guid para asegurar una semilla única incluso si los hilos inician en el mismo milisegundo
                Random localRandom = new Random(Guid.NewGuid().GetHashCode());

                //Librerias locales del hilo
                int localCurrentInfected = 0;
                int localNewInfections = 0;
                int localDead = 0;
                int localRecovered = 0;

                // PROCESAMIENTO DEL BLOQUE (Con lectura de Ghost-Cells implícita en los bordes)
                for (int y = startY; y < endY; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int index = y * Width + x;
                        State currentState = CurrentGrid[index];

                        if (currentState == State.Recovered)
                        {
                            _nextGrid[index] = currentState;
                            localRecovered++;
                            continue;
                        }

                        if (currentState == State.Dead)
                        {
                            _nextGrid[index] = currentState;
                            localDead++;
                            continue;
                        }

                        if (currentState == State.Infected)
                        {
                            double chance = localRandom.NextDouble();
                            if (chance < _recoveryProb)
                            {
                                _nextGrid[index] = State.Recovered;
                                localRecovered++;
                            }
                            else if (chance < _recoveryProb + _deathProb)
                            {
                                _nextGrid[index] = State.Dead;
                                localDead++;
                            }
                            else
                            {
                                _nextGrid[index] = State.Infected; // Sigue infectado
                                localCurrentInfected++;
                            }
                        }
                        else if (currentState == State.Susceptible)
                        {
                            int infectedNeighbors = CountInfectedNeighbors(x, y);

                            if (infectedNeighbors > 0)
                            {
                                double probOfStayingHealthy = Math.Pow(1.0 - _infectionProb, infectedNeighbors);
                                double probOfInfection = 1.0 - probOfStayingHealthy;

                                if (localRandom.NextDouble() < probOfInfection)
                                {
                                    _nextGrid[index] = State.Infected;
                                    localNewInfections++;
                                    localCurrentInfected++;
                                }
                                else
                                {
                                    _nextGrid[index] = State.Susceptible; // Sigue susceptible
                                }
                            }
                            else
                            {
                                _nextGrid[index] = State.Susceptible; // Sigue susceptible
                            }
                        }
                    }
                }

                // --- REDUCCIÓN PARALELA (El embudo seguro) ---
                // Aquí sumamos la libreta local a la variable global usando una operación Atómica
                Interlocked.Add(ref globalCurrentInfected, localCurrentInfected);
                Interlocked.Add(ref globalNewInfections, localNewInfections);
                Interlocked.Add(ref globalDead, localDead);
                Interlocked.Add(ref globalRecovered, localRecovered);
            });

            // Barrera de sincronización: El código de abajo no se ejecuta hasta que TODOS los hilos terminen.
            var temp = CurrentGrid;
            CurrentGrid = _nextGrid;
            _nextGrid = temp;

            return new DailyStats
            {
                Day = dayNumber,
                CurrentInfected = globalCurrentInfected,
                NewInfectionsToday = globalNewInfections,
                AccumulatedDead = globalDead,
                AccumulatedRecovered = globalRecovered
            };
        }

        private int CountInfectedNeighbors(int x, int y)
        {
            int infected = 0;
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue; // Saltar la celda actual

                    int neighborX = x + dx;
                    int neighborY = y + dy;

                    // Verificar límites del grid
                    if (neighborX >= 0 && neighborX < Width && neighborY >= 0 && neighborY < Height)
                    {
                        int neighborIndex = neighborY * Width + neighborX;
                        if (CurrentGrid[neighborIndex] == State.Infected)
                        {
                            infected++;
                        }
                    }
                }
            }
            return infected;
        }

        public void ExportGridToFile(string filePath)
        {
            StringBuilder sb = new StringBuilder();
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    // Convertimos el Enum a número (0=S, 1=I, 2=R, 3=D)
                    sb.Append((byte)CurrentGrid[y * Width + x]).Append(",");
                }
                sb.Length--; // Quitar la última coma
                sb.AppendLine();
            }
            File.WriteAllText(filePath, sb.ToString());
        }
    }
}
