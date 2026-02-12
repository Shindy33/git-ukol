using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Had
{
    internal class Program
    {
        struct Bod
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        static void Main(string[] args)
        {
            // Nastavení konzole
            Console.CursorVisible = false;
            int width = Console.WindowWidth - 2; // Ohraničení rámečkem
            int height = Console.WindowHeight - 2; // Ohraničení rámečkem

            // Vykreslení rámu
            DrawFrame(width, height);

            // Pozice hada (tělo je reprezentováno jako seznam bodů)
            List<Bod> had = new List<Bod>
            {
                new Bod { X = width / 2, Y = height / 2 }
            };
            int hadDelka = 5; // Počáteční délka hada
            string smer = "RIGHT"; // Výchozí směr

            // Generování jídla
            Random random = new Random();
            Bod jidlo = GenerateFood(width, height, had, random);

            // Herní cyklus
            while (true)
            {
                // 1. Ovládání
                if (Console.KeyAvailable)
                {
                    ConsoleKey key = Console.ReadKey(true).Key;
                    smer = key switch
                    {
                        ConsoleKey.UpArrow when smer != "DOWN" => "UP",
                        ConsoleKey.DownArrow when smer != "UP" => "DOWN",
                        ConsoleKey.LeftArrow when smer != "RIGHT" => "LEFT",
                        ConsoleKey.RightArrow when smer != "LEFT" => "RIGHT",
                        _ => smer
                    };
                }

                // 2. Pohyb hada
                Bod hlava = had.First();
                Bod novaHlava = smer switch
                {
                    "UP" => new Bod { X = hlava.X, Y = hlava.Y - 1 },
                    "DOWN" => new Bod { X = hlava.X, Y = hlava.Y + 1 },
                    "LEFT" => new Bod { X = hlava.X - 1, Y = hlava.Y },
                    "RIGHT" => new Bod { X = hlava.X + 1, Y = hlava.Y },
                    _ => hlava
                };

                // Přidej novou hlavu
                had.Insert(0, novaHlava);

                // Kontrola jestli had snědl jídlo
                if (novaHlava.X == jidlo.X && novaHlava.Y == jidlo.Y)
                {
                    // Had snědl jídlo, prodluž délku
                    hadDelka++;

                    // Pokus o generování nového jídla
                    jidlo = GenerateFood(width, height, had, random);
                }
                else
                {
                    // Zkrať hada (když nesnědl jídlo)
                    while (had.Count > hadDelka)
                    {
                        had.RemoveAt(had.Count - 1);
                    }
                }

                // 3. Kontrola kolize
                if (novaHlava.X <= 0 || novaHlava.Y <= 0 || novaHlava.X >= width || novaHlava.Y >= height || had.Skip(1).Any(b => b.X == novaHlava.X && b.Y == novaHlava.Y))
                {
                    // Had narazil
                    Console.Clear();
                    Console.SetCursorPosition(width / 2 - 5, height / 2);
                    Console.WriteLine("GAME OVER!");
                    Console.WriteLine($"Skóre: {hadDelka - 5}");
                    break;
                }

                // 4. Vykreslení
                Console.Clear();

                // Vykreslení hadího těla a hlavy
                for (int i = 0; i < had.Count; i++)
                {
                    Console.SetCursorPosition(had[i].X, had[i].Y);
                    if (i == 0)
                    {
                        Console.Write("☺"); // Hlavu hada vykreslíme jiným znakem
                    }
                    else
                    {
                        Console.Write("●"); // Tělo hada
                    }
                }

                // Vykreslení jídla
                Console.SetCursorPosition(jidlo.X, jidlo.Y);
                Console.Write("*");

                // Vykreslení rámu
                DrawFrame(width, height);

                // 5. Zpomalení hry (vyrovnání rychlosti pohybu)
                int sleepTime = smer == "UP" || smer == "DOWN" ? 200 : 150; // Zpomalení při pohybu nahoru/dolu
                Thread.Sleep(sleepTime);
            }
        }

        static void DrawFrame(int width, int height)
        {
            // Vykreslení horní a dolní hrany
            for (int x = 0; x <= width; x++)
            {
                Console.SetCursorPosition(x, 0); 
                Console.Write("◼");
                Console.SetCursorPosition(x, height + 1);
                Console.Write("◼");
            }

            // Vykreslení levé a pravé hrany
            for (int y = 0; y <= height + 1; y++)
            {
                Console.SetCursorPosition(0, y);
                Console.Write("◼");
                Console.SetCursorPosition(width + 1, y);
                Console.Write("◼");
            }
        }

        static Bod GenerateFood(int width, int height, List<Bod> had, Random random)
        {
            HashSet<(int, int)> bodyHada = new HashSet<(int, int)>(had.Select(b => (b.X, b.Y))); // Sada obsazených pozic (tělo hada)
            Bod jidlo;

            // Maximální počet pokusů pro náhodné generování
            const int maxPokusy = 100;
            for (int i = 0; i < maxPokusy; i++)
            {
                jidlo = new Bod
                {
                    X = random.Next(1, width), // Generuj uvnitř herní plochy mimo rám
                    Y = random.Next(1, height)
                };

                if (!bodyHada.Contains((jidlo.X, jidlo.Y)))
                {
                    return jidlo; // Náhodná pozice je volná
                }
            }

            // Pokud se nepodaří najít jídlo náhodně, najdi první volnou pozici systematicky
            for (int x = 1; x < width; x++)
            {
                for (int y = 1; y < height; y++)
                {
                    if (!bodyHada.Contains((x, y)))
                    {
                        return new Bod { X = x, Y = y }; // První dostupná pozice nalezena
                    }
                }
            }

            throw new Exception("Nelze vygenerovat nové jídlo - celá plocha je obsazena!");
        }
    }
}