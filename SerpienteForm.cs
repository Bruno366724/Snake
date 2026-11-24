using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Serpiente
{
    public class SerpienteForm : Form
    {
        const int Celda = 24;
        const int Columnas = 28;
        const int Filas = 18;
        const int AlturaPanel = 56;


        // Partes de dificultad
        string[] nombresDificultad = {"Fácil", "Medio", "Difícil"};
        int[] intervaloInicial = {170, 130, 90};
        int[] intervaloMinimo = {110, 60, 40};
        int[] records = new int[3];

        //Menú
        bool enMenu;
        int opcionMenu;
        int dificultad;


        List<int> cuerpoX = new List<int>();
        List<int> cuerpoY = new List<int>();
        int dirX;
        int dirY;
        int ultDirX;
        int ultDirY;
        Queue<Point> colaDirecciones = new Queue<Point>();
        int comidaX;
        int comidaY;
        int puntaje;
        // int record;
        int nivel;
        bool jugando;
        bool pausa;
        Random azar = new Random();
        System.Windows.Forms.Timer reloj = new System.Windows.Forms.Timer();

        SolidBrush pincelA = new SolidBrush(Color.FromArgb(26, 32, 48));
        SolidBrush pincelB = new SolidBrush(Color.FromArgb(31, 38, 57));

        public SerpienteForm()
        {
            Text = "Serpiente";
            ClientSize = new Size(Columnas * Celda, Filas * Celda + AlturaPanel);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;
            BackColor = Color.FromArgb(15, 18, 28);

            Paint += Dibujar;
            KeyDown += TeclaPresionada;

            reloj.Tick += AvanzarJuego;

            // IniciarJuego();
            enMenu = true;
            opcionMenu = 0;
            dificultad = 1;
        }

        //Calcular los intervalos y volver al menú
        int CalcularIntervalo()
        {
            int intervalo = intervaloInicial[dificultad] - (nivel - 1) * 10;
            return Math.Max(intervaloMinimo[dificultad], intervalo);
        }
        void VolverAlMenu()
        {
            reloj.Stop();
            enMenu = true;
            pausa = false;
            opcionMenu = 0;
            Invalidate();
        }

        void IniciarJuego()
        {
            cuerpoX.Clear();
            cuerpoY.Clear();
            cuerpoX.Add(10);
            cuerpoY.Add(9);
            dirX = 1;
            dirY = 0;
            ultDirX = 1;
            ultDirY = 0;
            colaDirecciones.Clear();
            puntaje = 0;
            nivel = 1;
            jugando = true;
            pausa = false;
            // reloj.Interval = 130;
            reloj.Interval = CalcularIntervalo();
            NuevaComida();
            reloj.Start();
            Invalidate();
        }

        void NuevaComida()
        {
            bool ocupada = true;
            while (ocupada)
            {
                comidaX = azar.Next(0, Columnas);
                comidaY = azar.Next(0, Filas);
                ocupada = false;
                for (int i = 0; i < cuerpoX.Count; i++)
                {
                    if (cuerpoX[i] == comidaX && cuerpoY[i] == comidaY)
                    {
                        ocupada = true;
                    }
                }
            }
        }

        void TeclaPresionada(object? sender, KeyEventArgs e)
{
    if (enMenu)
    {
        TeclasMenu(e);
        return;
    }

    if (e.KeyCode == Keys.Up) { EncolarDireccion(0, -1); }
    else if (e.KeyCode == Keys.Down) { EncolarDireccion(0, 1); }
    else if (e.KeyCode == Keys.Left) { EncolarDireccion(-1, 0); }
    else if (e.KeyCode == Keys.Right) { EncolarDireccion(1, 0); }
    else if (e.KeyCode == Keys.Escape) { VolverAlMenu(); }
    else if (e.KeyCode == Keys.Enter && !jugando) { IniciarJuego(); }
    else if (e.KeyCode == Keys.Space && jugando)
    {
        pausa = !pausa;
        if (pausa) { reloj.Stop(); }
        else { reloj.Start(); }
        Invalidate();
    }
}

void TeclasMenu(KeyEventArgs e)
{
    if (e.KeyCode == Keys.Up)
    {
        opcionMenu = (opcionMenu + 2) % 3;
    }
    else if (e.KeyCode == Keys.Down)
    {
        opcionMenu = (opcionMenu + 1) % 3;
    }
    else if (e.KeyCode == Keys.Left && opcionMenu == 1)
    {
        dificultad = (dificultad + 2) % 3;
    }
    else if (e.KeyCode == Keys.Right && opcionMenu == 1)
    {
        dificultad = (dificultad + 1) % 3;
    }
    else if (e.KeyCode == Keys.Enter)
    {
        if (opcionMenu == 0)
        {
            enMenu = false;
            IniciarJuego();
        }
        else if (opcionMenu == 1)
        {
            dificultad = (dificultad + 1) % 3;
        }
        else
        {
            Close();
        }
    }
    else if (e.KeyCode == Keys.Escape)
    {
        Close();
    }

    Invalidate();
}

        void EncolarDireccion(int nx, int ny)
        {
            if (!jugando || pausa)
            {
                return;
            }

            // Se valida contra la última dirección pedida (no contra la actual),
            // así dos flechas seguidas se guardan y se aplican una por turno
            bool valida = false;
            if (nx == 0 && ultDirY == 0) { valida = true; }
            if (ny == 0 && ultDirX == 0) { valida = true; }

            if (valida && colaDirecciones.Count < 3)
            {
                colaDirecciones.Enqueue(new Point(nx, ny));
                ultDirX = nx;
                ultDirY = ny;
            }
        }

        void AvanzarJuego(object? sender, EventArgs e)
        {
            // Cada turno se usa, como máximo, una dirección de la cola
            if (colaDirecciones.Count > 0)
            {
                Point nueva = colaDirecciones.Dequeue();
                dirX = nueva.X;
                dirY = nueva.Y;
            }

            int cabezaX = cuerpoX[cuerpoX.Count - 1] + dirX;
            int cabezaY = cuerpoY[cuerpoY.Count - 1] + dirY;

            // Colisión con paredes
            if (cabezaX < 0 || cabezaX >= Columnas || cabezaY < 0 || cabezaY >= Filas)
            {
                jugando = false;
            }

            // Colisión con el propio cuerpo
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

                if (cabezaX == comidaX && cabezaY == comidaY)
                {
                    puntaje++;
                    nivel = puntaje / 5 + 1;
                    // reloj.Interval = Math.Max(60, 130 - (nivel - 1) * 10);
                    reloj.Interval = CalcularIntervalo();
                    NuevaComida();
                }
                else
                {
                    cuerpoX.RemoveAt(0);
                    cuerpoY.RemoveAt(0);
                }
            }
            else
            {
                reloj.Stop();
                if (puntaje > records[dificultad])
                {
                    records[dificultad] = puntaje;
                }
            }

            Invalidate();
        }

        void Dibujar(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (enMenu)
            {
            DibujarMenu(g);
            return;
            }

            DibujarPanel(g);

            // Todo lo que sigue se dibuja debajo del panel superior
            g.TranslateTransform(0, AlturaPanel);
            DibujarTablero(g);
            // DibujarComida(g);
            DibujarManzana(g, comidaX * Celda, comidaY * Celda);
            DibujarSerpiente(g);
            if (pausa || !jugando)
            {
                DibujarMensaje(g);
            }
            g.ResetTransform();
        }

        //Dibujar el menú
        void DibujarMenu(Graphics g)
{
    int ancho = ClientSize.Width;
    int alto = ClientSize.Height;

    // Fondo en cuadrícula
    for (int y = 0; y <= alto / Celda; y++)
    {
        for (int x = 0; x < Columnas; x++)
        {
            if ((x + y) % 2 == 0)
            {
                g.FillRectangle(pincelA, x * Celda, y * Celda, Celda, Celda);
            }
            else
            {
                g.FillRectangle(pincelB, x * Celda, y * Celda, Celda, Celda);
            }
        }
    }

    // Serpiente decorativa que mira a una manzana
    int total = 7;
    int inicioX = (ancho - 8 * Celda) / 2;
    int inicioY = 130;
    for (int i = 0; i < total - 1; i++)
    {
        int verde = 110 + (110 * i) / (total - 2);
        DibujarCuerpo(g, inicioX + i * Celda, inicioY, verde);
    }
    DibujarCabeza(g, inicioX + (total - 1) * Celda, inicioY, 1, 0);
    DibujarManzana(g, inicioX + total * Celda, inicioY);

    using (Font titulo = new Font("Segoe UI", 40, FontStyle.Bold))
    using (Font opcion = new Font("Segoe UI", 16, FontStyle.Bold))
    using (Font ayuda = new Font("Segoe UI", 10))
    using (SolidBrush verdeTitulo = new SolidBrush(Color.FromArgb(80, 220, 120)))
    using (SolidBrush resalte = new SolidBrush(Color.FromArgb(90, 80, 220, 120)))
    using (StringFormat centro = new StringFormat())
    {
        centro.Alignment = StringAlignment.Center;
        centro.LineAlignment = StringAlignment.Center;

        g.DrawString("SERPIENTE", titulo, verdeTitulo, new RectangleF(0, 30, ancho, 80), centro);

        string[] textos = new string[3];
        textos[0] = "Jugar";
        textos[1] = "Dificultad: " + nombresDificultad[dificultad];
        textos[2] = "Salir";
        if (opcionMenu == 1)
        {
            textos[1] = "‹  " + textos[1] + "  ›";
        }

        for (int i = 0; i < 3; i++)
        {
            int y = 210 + i * 62;
            Rectangle caja = new Rectangle((ancho - 360) / 2, y, 360, 46);
            Brush colorTexto = Brushes.Gray;
            if (i == opcionMenu)
            {
                RellenarRedondeado(g, resalte, caja.X, caja.Y, caja.Width, caja.Height, 14);
                colorTexto = Brushes.White;
            }
            g.DrawString(textos[i], opcion, colorTexto, caja, centro);
        }

        g.DrawString("↑ ↓ moverte   ·   Enter elegir   ·   ← → cambiar dificultad", ayuda, Brushes.Gray, new RectangleF(0, alto - 45, ancho, 30), centro);
            }
        }

        void DibujarPanel(Graphics g)
        {
            using (Font titulo = new Font("Segoe UI", 18, FontStyle.Bold))
            using (Font dato = new Font("Segoe UI", 11, FontStyle.Bold))
            using (SolidBrush verde = new SolidBrush(Color.FromArgb(80, 220, 120)))
            {
                g.DrawString("SERPIENTE", titulo, verde, 12, 10);

                string texto = nombresDificultad[dificultad] + "    Puntaje " + puntaje + "    Récord " + records[dificultad] + "    Nivel " + nivel;
                SizeF medida = g.MeasureString(texto, dato);
                g.DrawString(texto, dato, Brushes.White, ClientSize.Width - medida.Width - 12, 18);
            }
        }

        void DibujarTablero(Graphics g)
        {
            // Tablero en cuadrícula tipo ajedrez, con dos tonos parecidos
            for (int y = 0; y < Filas; y++)
            {
                for (int x = 0; x < Columnas; x++)
                {
                    if ((x + y) % 2 == 0)
                    {
                        g.FillRectangle(pincelA, x * Celda, y * Celda, Celda, Celda);
                    }
                    else
                    {
                        g.FillRectangle(pincelB, x * Celda, y * Celda, Celda, Celda);
                    }
                }
            }

            using (Pen borde = new Pen(Color.FromArgb(80, 220, 120), 2))
            {
                g.DrawRectangle(borde, 1, 1, Columnas * Celda - 2, Filas * Celda - 2);
            }
        }

        // void DibujarComida(Graphics g)
        // {
        //     int px = comidaX * Celda;
        //     int py = comidaY * Celda;

        //     // Manzana con brillo y una hojita
        //     g.FillEllipse(Brushes.Crimson, px + 3, py + 5, Celda - 6, Celda - 7);
        //     using (SolidBrush brillo = new SolidBrush(Color.FromArgb(160, 255, 255, 255)))
        //     {
        //         g.FillEllipse(brillo, px + 7, py + 8, 5, 5);
        //     }
        //     g.FillEllipse(Brushes.LimeGreen, px + Celda / 2, py + 1, 8, 5);
        // }

        // void DibujarSerpiente(Graphics g)
        // {
        //     int total = cuerpoX.Count;

        //     // Cuerpo: degradé que va de verde oscuro (cola) a verde claro (cerca de la cabeza)
        //     for (int i = 0; i < total - 1; i++)
        //     {
        //         int verde = 110;
        //         if (total > 2)
        //         {
        //             verde = 110 + (110 * i) / (total - 2);
        //         }
        //         using (SolidBrush pincel = new SolidBrush(Color.FromArgb(20, verde, 70)))
        //         {
        //             RellenarRedondeado(g, pincel, cuerpoX[i] * Celda + 2, cuerpoY[i] * Celda + 2, Celda - 4, Celda - 4, 8);
        //         }
        //     }

        //     // Cabeza
        //     int cabezaX = cuerpoX[total - 1];
        //     int cabezaY = cuerpoY[total - 1];
        //     using (SolidBrush pincelCabeza = new SolidBrush(Color.FromArgb(120, 255, 140)))
        //     {
        //         RellenarRedondeado(g, pincelCabeza, cabezaX * Celda + 1, cabezaY * Celda + 1, Celda - 2, Celda - 2, 9);
        //     }

        //     // Ojos: se ubican según hacia dónde mira la serpiente
        //     int cx = cabezaX * Celda + Celda / 2;
        //     int cy = cabezaY * Celda + Celda / 2;
        //     int ojo1X;
        //     int ojo1Y;
        //     int ojo2X;
        //     int ojo2Y;
        //     if (dirX != 0)
        //     {
        //         ojo1X = cx + dirX * 4;
        //         ojo2X = ojo1X;
        //         ojo1Y = cy - 5;
        //         ojo2Y = cy + 5;
        //     }
        //     else
        //     {
        //         ojo1Y = cy + dirY * 4;
        //         ojo2Y = ojo1Y;
        //         ojo1X = cx - 5;
        //         ojo2X = cx + 5;
        //     }
        //     g.FillEllipse(Brushes.White, ojo1X - 4, ojo1Y - 4, 8, 8);
        //     g.FillEllipse(Brushes.White, ojo2X - 4, ojo2Y - 4, 8, 8);
        //     g.FillEllipse(Brushes.Black, ojo1X - 2 + dirX, ojo1Y - 2 + dirY, 4, 4);
        //     g.FillEllipse(Brushes.Black, ojo2X - 2 + dirX, ojo2Y - 2 + dirY, 4, 4);
        // }
        // px y py son la posición en píxeles de la esquina de la celda
void DibujarManzana(Graphics g, int px, int py)
{
    g.FillEllipse(Brushes.Crimson, px + 3, py + 5, Celda - 6, Celda - 7);
    using (SolidBrush brillo = new SolidBrush(Color.FromArgb(160, 255, 255, 255)))
    {
        g.FillEllipse(brillo, px + 7, py + 8, 5, 5);
    }
    g.FillEllipse(Brushes.LimeGreen, px + Celda / 2, py + 1, 8, 5);
}

void DibujarSerpiente(Graphics g)
{
    int total = cuerpoX.Count;

    // Cuerpo: degradé de verde oscuro (cola) a verde claro (cerca de la cabeza)
    for (int i = 0; i < total - 1; i++)
    {
        int verde = 110;
        if (total > 2)
        {
            verde = 110 + (110 * i) / (total - 2);
        }
        DibujarCuerpo(g, cuerpoX[i] * Celda, cuerpoY[i] * Celda, verde);
    }

    DibujarCabeza(g, cuerpoX[total - 1] * Celda, cuerpoY[total - 1] * Celda, dirX, dirY);
}

void DibujarCuerpo(Graphics g, int px, int py, int verde)
{
    using (SolidBrush pincel = new SolidBrush(Color.FromArgb(20, verde, 70)))
    {
        RellenarRedondeado(g, pincel, px + 2, py + 2, Celda - 4, Celda - 4, 8);
    }
}

// dx y dy indican hacia dónde mira la cabeza
void DibujarCabeza(Graphics g, int px, int py, int dx, int dy)
{
    using (SolidBrush pincelCabeza = new SolidBrush(Color.FromArgb(120, 255, 140)))
    {
        RellenarRedondeado(g, pincelCabeza, px + 1, py + 1, Celda - 2, Celda - 2, 9);
    }

    int cx = px + Celda / 2;
    int cy = py + Celda / 2;
    int ojo1X;
    int ojo1Y;
    int ojo2X;
    int ojo2Y;
    if (dx != 0)
    {
        ojo1X = cx + dx * 4;
        ojo2X = ojo1X;
        ojo1Y = cy - 5;
        ojo2Y = cy + 5;
    }
    else
    {
        ojo1Y = cy + dy * 4;
        ojo2Y = ojo1Y;
        ojo1X = cx - 5;
        ojo2X = cx + 5;
    }
    g.FillEllipse(Brushes.White, ojo1X - 4, ojo1Y - 4, 8, 8);
    g.FillEllipse(Brushes.White, ojo2X - 4, ojo2Y - 4, 8, 8);
    g.FillEllipse(Brushes.Black, ojo1X - 2 + dx, ojo1Y - 2 + dy, 4, 4);
    g.FillEllipse(Brushes.Black, ojo2X - 2 + dx, ojo2Y - 2 + dy, 4, 4);
}

        void RellenarRedondeado(Graphics g, Brush pincel, int x, int y, int ancho, int alto, int radio)
        {
            int d = radio * 2;
            using (GraphicsPath camino = new GraphicsPath())
            {
                camino.AddArc(x, y, d, d, 180, 90);
                camino.AddArc(x + ancho - d, y, d, d, 270, 90);
                camino.AddArc(x + ancho - d, y + alto - d, d, d, 0, 90);
                camino.AddArc(x, y + alto - d, d, d, 90, 90);
                camino.CloseFigure();
                g.FillPath(pincel, camino);
            }
        }

        void DibujarMensaje(Graphics g)
        {
            string titulo = "GAME OVER";
            string subtitulo = "Enter para reiniciar · Esc para el menú";
            if (pausa)
            {
                titulo = "PAUSA";
                subtitulo = "Espacio para continuar · Esc para el menú";
            }

            using (SolidBrush velo = new SolidBrush(Color.FromArgb(170, 0, 0, 0)))
            using (Font grande = new Font("Segoe UI", 28, FontStyle.Bold))
            using (Font chica = new Font("Segoe UI", 12))
            using (StringFormat centro = new StringFormat())
            {
                centro.Alignment = StringAlignment.Center;
                centro.LineAlignment = StringAlignment.Center;

                g.FillRectangle(velo, 0, 0, Columnas * Celda, Filas * Celda);

                RectangleF zona1 = new RectangleF(0, Filas * Celda / 2 - 40, Columnas * Celda, 50);
                RectangleF zona2 = new RectangleF(0, Filas * Celda / 2 + 10, Columnas * Celda, 30);
                g.DrawString(titulo, grande, Brushes.White, zona1, centro);
                g.DrawString(subtitulo, chica, Brushes.LightGray, zona2, centro);
            }
        }
    }
}