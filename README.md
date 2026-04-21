# Simulación Monte-Carlo de Epidemias (SIRD) Paralela

**Autor:** Elvyn Alexander Bernal

**Matrícula:** 2024-0058

**Materia:** Programación Paralela

## Descripción del Proyecto
Este proyecto implementa una simulación estocástica (Monte-Carlo) de la propagación de un virus en una población de 1,000,000 de personas (Grilla 2D de 1k x 1k). El modelo matemático utilizado es el **SIRD** (Susceptible, Infectado, Recuperado, Fallecido).

El proyecto consta de tres partes principales:
1. **Secuencial:** Implementación base de la lógica de actualización celular.
2. **Paralelo:** Optimización utilizando C# `Parallel.For`, descomposición de dominio y reducción paralela (`Interlocked`).
3. **Visualización:** Script en Python para generar una animación del brote epidémico.

## Requisitos Previos
* .NET SDK 8.0 (o superior)
* Python 3.x
* Librerías de Python: `pip install numpy matplotlib`

## Instrucciones de Ejecución

### 1. Ejecutar el Benchmark (Strong Scaling)
Para obtener los tiempos de ejecución y la gráfica de Speed-up, compila el código paralelo en modo Release:
```bash
cd Paralelo
dotnet run -c Release
```

Esto generará el archivo Resultados_Scaling.csv en el directorio de ejecución.

### 2. Generar la Animación

Para renderizar el video (GIF) del brote, primero ejecuta el generador de datos en C# (asegúrate de que el método Main esté configurado para exportar a la carpeta Frames). Luego, ejecuta el script de Python:

```bash
python generar_video.py
```

El resultado será el archivo brote_epidemia.gif.
