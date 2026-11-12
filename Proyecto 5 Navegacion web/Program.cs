using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NavegacionWeb
{
    // Navegador simulado: usa DOS pilas (Atrás y Adelante) + la página actual.
    class Navegador
    {
        private readonly Stack<string> pilaAtras = new Stack<string>();
        private readonly Stack<string> pilaAdelante = new Stack<string>();
        private string paginaActual = null;
        private string ultimaVisitada = null; // última página abierta con "Visitar"

        public string PaginaActual => paginaActual;

        // 1. Visitar página
        public void Visitar(string pagina)
        {
            if (paginaActual != null)
                pilaAtras.Push(paginaActual);   // la actual pasa al historial

            paginaActual = pagina;
            ultimaVisitada = pagina;
            pilaAdelante.Clear();               // al visitar algo nuevo, "adelante" se invalida
            Console.WriteLine($"Visitando: {pagina}");
        }

        // 2. Regresar (Atrás)
        public void Regresar()
        {
            if (pilaAtras.Count == 0)
            {
                Console.WriteLine("No hay páginas anteriores.");
                return;
            }
            pilaAdelante.Push(paginaActual);    // la actual se guarda para poder avanzar
            paginaActual = pilaAtras.Pop();
            Console.WriteLine($"Regresaste a: {paginaActual}");
        }

        // 3. Avanzar (Adelante)
        public void Avanzar()
        {
            if (pilaAdelante.Count == 0)
            {
                Console.WriteLine("No hay páginas siguientes.");
                return;
            }
            pilaAtras.Push(paginaActual);
            paginaActual = pilaAdelante.Pop();
            Console.WriteLine($"Avanzaste a: {paginaActual}");
        }

        // 4. Mostrar historial
        public void MostrarHistorial()
        {
            if (paginaActual == null)
            {
                Console.WriteLine("El historial está vacío.");
                return;
            }

            Console.WriteLine("\n===== HISTORIAL =====");
            // Stack enumera del tope a la base, así que invertimos para ver cronológicamente.
            foreach (string p in pilaAtras.Reverse())
                Console.WriteLine($"   {p}");

            Console.WriteLine($" > {paginaActual}   (página actual)");

            foreach (string p in pilaAdelante)  // el tope es la siguiente a la que se avanza
                Console.WriteLine($"   {p}   (adelante)");
            Console.WriteLine("=====================");
        }

        // 5. Eliminar historial
        public void EliminarHistorial()
        {
            pilaAtras.Clear();
            pilaAdelante.Clear();
            paginaActual = null;
            ultimaVisitada = null;
            Console.WriteLine("Historial eliminado.");
        }

        // 6. Buscar una página
        public void Buscar(string pagina)
        {
            bool encontrada = false;

            if (paginaActual != null &&
                paginaActual.Equals(pagina, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"'{pagina}' es la página actual.");
                encontrada = true;
            }

            int posAtras = 0;
            foreach (string p in pilaAtras) // del tope hacia la base
            {
                posAtras++;
                if (p.Equals(pagina, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"'{pagina}' está en la pila Atrás ({posAtras} paso(s) atrás).");
                    encontrada = true;
                }
            }

            int posAdelante = 0;
            foreach (string p in pilaAdelante)
            {
                posAdelante++;
                if (p.Equals(pagina, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"'{pagina}' está en la pila Adelante ({posAdelante} paso(s) adelante).");
                    encontrada = true;
                }
            }

            if (!encontrada)
                Console.WriteLine($"'{pagina}' no se encuentra en el historial.");
        }

        // 7. Mostrar última página visitada
        public void MostrarUltimaVisitada()
        {
            if (ultimaVisitada == null)
                Console.WriteLine("Aún no se ha visitado ninguna página.");
            else
                Console.WriteLine($"Última página visitada: {ultimaVisitada}");
        }

        // Estado de ambas pilas (útil para entender el funcionamiento)
        public void MostrarPilas()
        {
            Console.WriteLine("\nPila Atrás    (tope -> base): " +
                (pilaAtras.Count == 0 ? "(vacía)" : string.Join(" -> ", pilaAtras)));
            Console.WriteLine("Pila Adelante (tope -> base): " +
                (pilaAdelante.Count == 0 ? "(vacía)" : string.Join(" -> ", pilaAdelante)));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Navegador nav = new Navegador();
            int opcion;

            do
            {
                Console.WriteLine("\n===== NAVEGADOR WEB =====");
                Console.WriteLine($"Página actual: {nav.PaginaActual ?? "(ninguna)"}");
                Console.WriteLine("1. Visitar página");
                Console.WriteLine("2. Regresar");
                Console.WriteLine("3. Avanzar");
                Console.WriteLine("4. Mostrar historial");
                Console.WriteLine("5. Eliminar historial");
                Console.WriteLine("6. Buscar una página");
                Console.WriteLine("7. Mostrar última página visitada");
                Console.WriteLine("8. Ver estado de las pilas");
                Console.WriteLine("0. Salir");
                Console.Write("Opción: ");

                if (!int.TryParse(Console.ReadLine(), out opcion))
                    opcion = -1;

                switch (opcion)
                {
                    case 1:
                        string pagina = LeerTexto("Nombre o URL de la página: ");
                        nav.Visitar(pagina);
                        break;
                    case 2: nav.Regresar(); break;
                    case 3: nav.Avanzar(); break;
                    case 4: nav.MostrarHistorial(); break;
                    case 5: nav.EliminarHistorial(); break;
                    case 6:
                        nav.Buscar(LeerTexto("Página a buscar: "));
                        break;
                    case 7: nav.MostrarUltimaVisitada(); break;
                    case 8: nav.MostrarPilas(); break;
                    case 0: Console.WriteLine("Cerrando navegador..."); break;
                    default: Console.WriteLine("Opción no válida."); break;
                }
            } while (opcion != 0);
        }

        static string LeerTexto(string mensaje)
        {
            string texto;
            do
            {
                Console.Write(mensaje);
                texto = (Console.ReadLine() ?? "").Trim();
                if (texto == "")
                    Console.WriteLine("El valor no puede estar vacío.");
            } while (texto == "");
            return texto;
        }
    }
}
