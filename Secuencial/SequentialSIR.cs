using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Secuencial
{
    public class SequentialSIR
    {
        public readonly int Width;
        public readonly int Height;
        public readonly int TotalPopulation;

        public State[] CurrentGrid;
        private State[] _nextGrid;

        private readonly double _infectionProb;
        private readonly double _recoveryProb;
        private readonly double _deathProb;
        private readonly Random _random;

        public SequentialSIR(int size, double infectionProb, double recoveryProb, double deathProb, double initialInfectedRatio)
        {
            Width = size;
            Height = size;
            TotalPopulation = size * size;

            _infectionProb = infectionProb;
            _recoveryProb = recoveryProb;
            _deathProb = deathProb;

            _random = new Random();

            CurrentGrid = new State[TotalPopulation];
            _nextGrid = new State[TotalPopulation];

            InitializePopulation(initialInfectedRatio);
        }

        private void InitializePopulation(double initialInfectedRatio)
        {
            int initialInfectedCount = (int)(TotalPopulation * initialInfectedRatio);

            for (int i = 0; i < TotalPopulation; i++)
            {
                CurrentGrid[i] = State.Susceptible;
            }

            int infectedPlaced = 0;
            while (infectedPlaced < initialInfectedCount)
            {
                int randomPos = _random.Next(TotalPopulation);
                if (CurrentGrid[randomPos] == State.Susceptible)
                {
                    CurrentGrid[randomPos] = State.Infected;
                    infectedPlaced++;
                }
            }

            Array.Copy(CurrentGrid, _nextGrid, TotalPopulation);
        }

        public void SimulateDay()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int index = y * Width + x;
                    State currentState = CurrentGrid[index];

                    // Si ya se recuperó o murió, su estado no cambia y ya no contagia
                    if (currentState == State.Recovered || currentState == State.Dead)
                    {
                        _nextGrid[index] = currentState;
                        continue;
                    }

                    if (currentState == State.Infected)
                    {
                        // Tiramos un solo dado para decidir el destino del infectado hoy
                        double chance = _random.NextDouble();

                        if (chance < _recoveryProb)
                        {
                            _nextGrid[index] = State.Recovered;
                        }
                        else if (chance < _recoveryProb + _deathProb)
                        {
                            _nextGrid[index] = State.Dead;
                        }
                        else
                        {
                            _nextGrid[index] = State.Infected; // Sigue infectado
                        }
                    }
                    else if (currentState == State.Susceptible)
                    {
                        // Verificar vecinos para contagio
                        int infectedNeighbors = CountInfectedNeighbors(x, y);

                        double probOfStayingHealthy = Math.Pow(1.0 - _infectionProb, infectedNeighbors);
                        double probOfInfection = 1.0 - probOfStayingHealthy;

                        if (infectedNeighbors > 0 && _random.NextDouble() < probOfInfection)
                        {
                            _nextGrid[index] = State.Infected;
                        }
                        else
                        {
                            _nextGrid[index] = State.Susceptible; // Sigue sano
                        }
                    }
                }
            }

            var temp = CurrentGrid;
            CurrentGrid = _nextGrid;
            _nextGrid = temp;
        }

        private int CountInfectedNeighbors(int x, int y)
        {
            int infected = 0;
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue; // No contar a sí mismo

                    int neighborX = x + dx;
                    int neighborY = y + dy;

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
    }
}
