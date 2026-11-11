using System;

namespace Proyecto2_Restaurante
{
    public class Cliente
    {
        public string Nombre { get; set; }
        public int Personas { get; set; }

        public Cliente(string nombre, int personas)
        {
            Nombre = nombre;
            Personas = personas;
        }

        public override string ToString()
        {
            return $"Nombre: {Nombre} - Personas: {Personas}";
        }
    }
}

namespace Proyecto2_Restaurante
{
    public class Nodo
    {
        public Cliente Datos;
        public Nodo Siguiente;

        public Nodo(Cliente datos)
        {
            Datos = datos;
            Siguiente = null;
        }
    }
}
namespace Proyecto2_Restaurante
{
    public class Cola
    {
        private Nodo frente;
        private Nodo final;

        public void Encolar(Cliente cliente)
        {
            Nodo nuevo = new Nodo(cliente);

            if(frente == null)
            {
                frente = nuevo;
                final = nuevo;
            }
            else
            {
                final.Siguiente = nuevo;
                final = nuevo;
            }
        }
        public Cliente Desencolar()
        {
            if (frente == null)
                return null;

            Cliente cliente = frente.Datos;
            frente = frente.Siguiente;

            if (frente == null)
                final = null;

            return cliente;
        }

        public void MostrarCola()
        {
            Nodo actual = frente;
            while (actual != null)
            {
                System.Console.WriteLine(actual.Datos);
                actual = actual.Siguiente;
            }
        }
    }
}

namespace Proyecto2_Restaurante
{
    internal class  Program
    {
        static Cola espera = new Cola();
        static void Main()
        {
            int opcion;

            do
            {
                Console.Clear();
                Console.WriteLine("=== Restaurante ===");
                Console.WriteLine("1. Agregar cliente");
                Console.WriteLine("2. Asignar mesa");
                Console.WriteLine("3. Mostrar espera");
                Console.WriteLine("4. Salir");
                Console.Write("Seleccione una opción: ");
                
                if (!int.TryParse(Console.ReadLine(), out opcion))
                {
                    Console.WriteLine("Debe introducir un número válido.");
                    Console.ReadKey();
                    continue;
                }

                switch (opcion)
                {
                    case 1:
                        AgregarCliente();
                        break;
                    case 2:
                        AsignarMesa();
                        break;
                    case 3:
                        MostrarEspera();
                        break;
                    case 4:
                        Console.WriteLine("Saliendo del programa...");
                        break;
                    default:
                        Console.WriteLine("Opción inválida. Intente nuevamente.");
                        break;
                }
            } while (opcion != 4);
        }

        static void AgregarCliente()
        {
            Console.Write("Nombre del cliente: ");
            string nombre = Console.ReadLine();

            int personas;
            do
            {
                Console.Write("Número de personas: ");
                if (!int.TryParse(Console.ReadLine(), out personas) || personas <= 0)
                {
                    Console.WriteLine("Introduzca un número de personas válido.");
                    personas = 0;
                }
            } while (personas <= 0);

            espera.Encolar(new Cliente(nombre, personas));
            Console.WriteLine("Cliente agregado a la cola de espera.");
            Console.ReadKey();
        }

        static void AsignarMesa()
        {
            Cliente cliente = espera.Desencolar();

            if (cliente == null)
            {
                Console.WriteLine("No hay clientes en espera.");
            }
            else
            {
                Console.WriteLine("Mesa asignada a: " + cliente);
            }

            Console.ReadKey();
        }

        static void MostrarEspera()
        {
            Console.WriteLine("Clientes en espera:");
            espera.MostrarCola();
            Console.ReadKey();
        }
    }
}
