using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

class Program
{
    static void Main()
    {
        int ancho = 30;
        int alto = 15;

        // Cuerpo de la serpiente: la cabeza es el último elemento
        List<int> cuerpoX = new List<int>();
        List<int> cuerpoY = new List<int>();
        cuerpoX.Add(10);
        cuerpoY.Add(7);

        int dirX = 1;
        int dirY = 0;

        Random azar = new Random();
        int comidaX = azar.Next(1, ancho - 1);
        int comidaY = azar.Next(1, alto - 1);

        int puntaje = 0;
        bool jugando = true;

        Console.CursorVisible = false;
        Console.Clear();

        while (jugando)
        {
            // 1. Leer teclas sin bloquear
            if (Console.KeyAvailable)
            {
                ConsoleKey tecla = Console.ReadKey(true).Key;

                if (tecla == ConsoleKey.UpArrow && dirY == 0) { dirX = 0; dirY = -1; }
                else if (tecla == ConsoleKey.DownArrow && dirY == 0) { dirX = 0; dirY = 1; }
                else if (tecla == ConsoleKey.LeftArrow && dirX == 0) { dirX = -1; dirY = 0; }
                else if (tecla == ConsoleKey.RightArrow && dirX == 0) { dirX = 1; dirY = 0; }
            }

            // 2. Calcular nueva cabeza
            int cabezaX = cuerpoX[cuerpoX.Count - 1] + dirX;
            int cabezaY = cuerpoY[cuerpoY.Count - 1] + dirY;

            // 3. Colisión con paredes
            if (cabezaX <= 0 || cabezaX >= ancho - 1 || cabezaY <= 0 || cabezaY >= alto - 1)
            {
                jugando = false;
            }

            // 4. Colisión con el propio cuerpo
            for (int i = 0; i < cuerpoX.Count; i++)
            {
                if (cuerpoX[i] == cabezaX && cuerpoY[i] == cabezaY)
                {
                    jugando = false;
                }
            }

            if (jugando)
            {
                cuerpoX.Add(cabezaX);
                cuerpoY.Add(cabezaY);

                // 5. ¿Comió?
                if (cabezaX == comidaX && cabezaY == comidaY)
                {
                    puntaje++;
                    comidaX = azar.Next(1, ancho - 1);
                    comidaY = azar.Next(1, alto - 1);
                }
                else
                {
                    // Si no comió, se quita la cola para que no crezca
                    cuerpoX.RemoveAt(0);
                    cuerpoY.RemoveAt(0);
                }
            }

            // 6. Dibujar
            // Se arma todo el cuadro en un solo texto y se imprime de una vez
            StringBuilder cuadro = new StringBuilder();
            for (int y = 0; y < alto; y++)
            {
                for (int x = 0; x < ancho; x++)
                {
                    if (x == 0 || x == ancho - 1 || y == 0 || y == alto - 1)
                    {
                        cuadro.Append("#");
                    }
                    else if (x == comidaX && y == comidaY)
                    {
                        cuadro.Append("$");
                    }
                    else
                    {
                        bool esSerpiente = false;
                        for (int i = 0; i < cuerpoX.Count; i++)
                        {
                            if (cuerpoX[i] == x && cuerpoY[i] == y)
                            {
                                esSerpiente = true;
                            }
                        }
                        if (esSerpiente)
                        {
                            cuadro.Append("0");
                        }
                        else
                        {
                            cuadro.Append(" ");
                        }
                    }
                }
                cuadro.AppendLine();
            }
            cuadro.AppendLine("Puntaje: " + puntaje + "   ");

            // En vez de borrar la pantalla, se vuelve al inicio y se sobreescribe
            Console.SetCursorPosition(0, 0);
            Console.Write(cuadro.ToString());

            Thread.Sleep(150);
        }

        Console.WriteLine("Game over. Puntaje final: " + puntaje);
        Console.CursorVisible = true;
    }
}