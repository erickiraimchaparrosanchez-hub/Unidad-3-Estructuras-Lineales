using System;

namespace Proyecto8_Banco
{
    public class Cliente
    {
        public int Turno { get; set; }
        public string Nombre { get; set; }
        public int Prioridad { get; set; }

        public Cliente(int turno, string nombre, int prioridad)
        {
            Turno = turno;
            Nombre = nombre;
            Prioridad = prioridad;
        }

        public override string ToString()
        {
            return $"Turno: {Turno}, Nombre: {Nombre}, Prioridad: {Prioridad}";
        }
    }

    internal class Program
    {
        public static void Main()
        {
            ColaPrioridad cola = new ColaPrioridad();
            int siguienteTurno = 1;
            bool salir = false;

            while (!salir)
            {
                Console.Clear();
                Console.WriteLine("====================================");
                Console.WriteLine("     SISTEMA DE ATENCION BANCARIA");
                Console.WriteLine("====================================");
                Console.WriteLine("1. Registrar cliente");
                Console.WriteLine("2. Mostrar cola");
                Console.WriteLine("3. Atender cliente");
                Console.WriteLine("4. Salir");
                Console.Write("Seleccione una opcion: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        Console.Write("Nombre del cliente: ");
                        string nombre = Console.ReadLine();

                        if (string.IsNullOrWhiteSpace(nombre))
                        {
                            Console.WriteLine("El nombre no puede estar vacio.");
                            Pausar();
                            break;
                        }

                        Console.Write("Prioridad (1 = preferente, 2 = normal): ");
                        int prioridad;
                        if (!int.TryParse(Console.ReadLine(), out prioridad) || prioridad < 1)
                        {
                            Console.WriteLine("La prioridad debe ser un numero mayor o igual que 1.");
                            Pausar();
                            break;
                        }

                        cola.Insertar(new Cliente(siguienteTurno, nombre, prioridad));
                        Console.WriteLine("Cliente registrado con el turno " + siguienteTurno + ".");
                        siguienteTurno++;
                        Pausar();
                        break;

                    case "2":
                        Console.WriteLine("\nClientes en espera:");
                        if (cola.EstaVacia())
                            Console.WriteLine("No hay clientes en espera.");
                        else
                            cola.Mostrar();
                        Pausar();
                        break;

                    case "3":
                        Cliente atendido = cola.Atender();
                        Console.WriteLine(atendido == null
                            ? "No hay clientes para atender."
                            : "Atendiendo a: " + atendido);
                        Pausar();
                        break;

                    case "4":
                        salir = true;
                        break;

                    default:
                        Console.WriteLine("Opcion no valida.");
                        Pausar();
                        break;
                }
            }
        }

        private static void Pausar()
        {
            Console.WriteLine("\nPulse una tecla para continuar...");
            Console.ReadKey();
        }
    }
}
