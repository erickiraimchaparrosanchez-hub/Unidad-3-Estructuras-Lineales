using System;
using System.Collections.Generic;
using System.Linq;

namespace SimuladorLlamadas
{
    enum EstadoLlamada
    {
        Registrada, // Sonando / recién registrada
        Activa,     // En conversación
        EnEspera    // Pausada porque entró otra llamada encima
    }

    class Llamada
    {
        public int Id { get; }
        public string Numero { get; }
        public string Contacto { get; }
        public DateTime HoraRegistro { get; }
        public EstadoLlamada Estado { get; set; }

        public Llamada(int id, string numero, string contacto)
        {
            Id = id;
            Numero = numero;
            Contacto = contacto;
            HoraRegistro = DateTime.Now;
            Estado = EstadoLlamada.Registrada;
        }

        public override string ToString()
        {
            return $"#{Id} | {Contacto} ({Numero}) | {Estado} | {HoraRegistro:HH:mm:ss}";
        }
    }

    class GestorLlamadas
    {
        // La pila: la última llamada registrada está en el tope.
        private readonly Stack<Llamada> pila = new Stack<Llamada>();
        private int contadorId = 1;

        // 1. Registrar llamada (Push)
        public void Registrar(string numero, string contacto)
        {
            // Si hay una llamada activa, pasa a espera para poder regresar a ella después.
            if (pila.Count > 0 && pila.Peek().Estado == EstadoLlamada.Activa)
                pila.Peek().Estado = EstadoLlamada.EnEspera;

            var llamada = new Llamada(contadorId++, numero, contacto);
            pila.Push(llamada);
            Console.WriteLine($"Llamada registrada: {llamada}");
        }

        // 2. Atender llamada (la del tope)
        public void Atender()
        {
            if (pila.Count == 0)
            {
                Console.WriteLine("No hay llamadas para atender.");
                return;
            }

            var tope = pila.Peek();
            if (tope.Estado == EstadoLlamada.Activa)
            {
                Console.WriteLine("La llamada ya está siendo atendida.");
                return;
            }

            tope.Estado = EstadoLlamada.Activa;
            Console.WriteLine($"Atendiendo: {tope}");
        }

        // 3. Finalizar llamada (Pop) y regresar al estado anterior
        public void Finalizar()
        {
            if (pila.Count == 0)
            {
                Console.WriteLine("No hay llamadas activas para finalizar.");
                return;
            }

            var finalizada = pila.Pop();
            Console.WriteLine($"Llamada finalizada: {finalizada}");

            // Regresa a la llamada anterior (estado anterior)
            if (pila.Count > 0)
            {
                pila.Peek().Estado = EstadoLlamada.Activa;
                Console.WriteLine($"Se retoma la llamada: {pila.Peek()}");
            }
            else
            {
                Console.WriteLine("No quedan llamadas en la pila.");
            }
        }

        // 4. Mostrar llamadas activas (de tope a base)
        public void Mostrar()
        {
            if (pila.Count == 0)
            {
                Console.WriteLine("La pila de llamadas está vacía.");
                return;
            }

            Console.WriteLine("=== PILA DE LLAMADAS (tope -> base) ===");
            foreach (var l in pila)
                Console.WriteLine(l);
        }

        // 5. Recuperar última llamada (Peek, sin sacarla)
        public void RecuperarUltima()
        {
            if (pila.Count == 0)
            {
                Console.WriteLine("No hay llamadas registradas.");
                return;
            }

            Console.WriteLine($"Última llamada: {pila.Peek()}");
        }

        // 6. Buscar llamada por número o nombre del contacto
        public void Buscar(string texto)
        {
            var resultados = pila.Where(l =>
                l.Numero.Contains(texto) ||
                l.Contacto.IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            if (resultados.Count == 0)
            {
                Console.WriteLine("No se encontraron llamadas.");
                return;
            }

            Console.WriteLine("Resultados:");
            foreach (var l in resultados)
                Console.WriteLine(l);
        }

        // 7. Eliminar una llamada específica por Id (usa una pila auxiliar)
        public void Eliminar(int id)
        {
            if (pila.Count == 0)
            {
                Console.WriteLine("La pila está vacía.");
                return;
            }

            bool eraTope = pila.Peek().Id == id;
            var auxiliar = new Stack<Llamada>();
            Llamada eliminada = null;

            while (pila.Count > 0)
            {
                var actual = pila.Pop();
                if (actual.Id == id)
                {
                    eliminada = actual;
                    break;
                }
                auxiliar.Push(actual);
            }

            // Restaurar el orden original
            while (auxiliar.Count > 0)
                pila.Push(auxiliar.Pop());

            if (eliminada == null)
            {
                Console.WriteLine("No existe una llamada con ese Id.");
                return;
            }

            Console.WriteLine($"Llamada eliminada: {eliminada}");

            // Si se eliminó la llamada activa del tope, se retoma la anterior
            if (eraTope && eliminada.Estado == EstadoLlamada.Activa && pila.Count > 0)
            {
                pila.Peek().Estado = EstadoLlamada.Activa;
                Console.WriteLine($"Se retoma la llamada: {pila.Peek()}");
            }
        }
    }

    class Program
    {
        static void Main()
        {
            var gestor = new GestorLlamadas();
            int opcion;

            do
            {
                Console.WriteLine();
                Console.WriteLine("===== SIMULADOR DE LLAMADAS TELEFÓNICAS =====");
                Console.WriteLine("1. Registrar llamada");
                Console.WriteLine("2. Atender llamada");
                Console.WriteLine("3. Finalizar llamada");
                Console.WriteLine("4. Mostrar llamadas activas");
                Console.WriteLine("5. Recuperar última llamada");
                Console.WriteLine("6. Buscar llamada");
                Console.WriteLine("7. Eliminar llamada");
                Console.WriteLine("0. Salir");
                Console.Write("Elige una opción: ");

                if (!int.TryParse(Console.ReadLine(), out opcion))
                    opcion = -1;

                Console.WriteLine();

                switch (opcion)
                {
                    case 1:
                        Console.Write("Número: ");
                        string numero = Console.ReadLine();
                        Console.Write("Contacto: ");
                        string contacto = Console.ReadLine();
                        gestor.Registrar(numero, contacto);
                        break;
                    case 2:
                        gestor.Atender();
                        break;
                    case 3:
                        gestor.Finalizar();
                        break;
                    case 4:
                        gestor.Mostrar();
                        break;
                    case 5:
                        gestor.RecuperarUltima();
                        break;
                    case 6:
                        Console.Write("Número o nombre a buscar: ");
                        gestor.Buscar(Console.ReadLine());
                        break;
                    case 7:
                        Console.Write("Id de la llamada a eliminar: ");
                        if (int.TryParse(Console.ReadLine(), out int id))
                            gestor.Eliminar(id);
                        else
                            Console.WriteLine("Id inválido.");
                        break;
                    case 0:
                        Console.WriteLine("Saliendo...");
                        break;
                    default:
                        Console.WriteLine("Opción no válida.");
                        break;
                }
            } while (opcion != 0);
        }
    }
}
