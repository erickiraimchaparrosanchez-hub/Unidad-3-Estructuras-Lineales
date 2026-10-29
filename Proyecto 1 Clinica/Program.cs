using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace Proyecto1_Clinica
{
    // ===================== MODELO =====================
    public class Paciente
    {
        public int Expediente { get; set; }
        public string Nombre { get; set; }
        public int Edad { get; set; }
        public int Prioridad { get; set; }

        public Paciente(int expediente, string nombre, int edad, int prioridad)
        {
            Expediente = expediente;
            Nombre = nombre;
            Edad = edad;
            Prioridad = prioridad;
        }

        public override string ToString()
        {
            return $"Expediente: {Expediente}|" + $"Nombre: {Nombre}|" +
                   $"Edad: {Edad}|" +
                   $"Prioridad: {Prioridad}";
        }
    }

    // ===================== NODO =====================
    // Nodo de la lista enlazada: guarda un paciente y apunta al siguiente nodo.
    public class Nodo
    {
        public Paciente Dato { get; set; }
        public Nodo Siguiente { get; set; }

        public Nodo(Paciente dato)
        {
            Dato = dato;
            Siguiente = null;
        }
    }

    // ===================== LISTA ENLAZADA =====================
    public class ListaEnlazada : IEnumerable<Paciente>
    {
        private Nodo cabeza;
        private Nodo cola;
        public int Cantidad { get; private set; }

        public void AgregarAlFinal(Paciente paciente)
        {
            Nodo nuevo = new Nodo(paciente);
            if (cabeza == null)
            {
                cabeza = nuevo;
                cola = nuevo;
            }
            else
            {
                cola.Siguiente = nuevo;
                cola = nuevo;
            }
            Cantidad++;
        }

        public Paciente RemoverPrimero()
        {
            if (cabeza == null) return null;

            Paciente paciente = cabeza.Dato;
            cabeza = cabeza.Siguiente;
            if (cabeza == null) cola = null;
            Cantidad--;
            return paciente;
        }

        public Paciente Eliminar(int expediente)
        {
            Nodo previo = null;
            Nodo actual = cabeza;

            while (actual != null)
            {
                if (actual.Dato.Expediente == expediente)
                {
                    if (previo == null) cabeza = actual.Siguiente;
                    else previo.Siguiente = actual.Siguiente;

                    if (actual == cola) cola = previo;

                    Cantidad--;
                    return actual.Dato;
                }
                previo = actual;
                actual = actual.Siguiente;
            }
            return null;
        }

        public Paciente Buscar(int expediente)
        {
            for (Nodo n = cabeza; n != null; n = n.Siguiente)
                if (n.Dato.Expediente == expediente)
                    return n.Dato;
            return null;
        }

        public IEnumerator<Paciente> GetEnumerator()
        {
            for (Nodo n = cabeza; n != null; n = n.Siguiente)
                yield return n.Dato;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    // ===================== COLA (FIFO) =====================
    // Cola FIFO construida sobre la lista enlazada.
    public class Cola : IEnumerable<Paciente>
    {
        private readonly ListaEnlazada lista = new ListaEnlazada();

        public int Cantidad => lista.Cantidad;
        public bool EstaVacia => lista.Cantidad == 0;

        public void Encolar(Paciente paciente) => lista.AgregarAlFinal(paciente);
        public Paciente Desencolar() => lista.RemoverPrimero();
        public Paciente Eliminar(int expediente) => lista.Eliminar(expediente);
        public Paciente Buscar(int expediente) => lista.Buscar(expediente);

        public IEnumerator<Paciente> GetEnumerator() => lista.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    // ===================== COLA DE PRIORIDAD =====================
    // Una cola FIFO por nivel (1 = Emergencia ... 4 = Cita Programada).
    // Siempre se atiende el nivel más urgente que tenga pacientes;
    // dentro del mismo nivel, por orden de llegada.
    public class ColaPrioridad : IEnumerable<Paciente>
    {
        public const int NivelMinimo = 1;
        public const int NivelMaximo = 4;

        private readonly Cola[] colas = new Cola[NivelMaximo];

        public ColaPrioridad()
        {
            for (int i = 0; i < colas.Length; i++)
                colas[i] = new Cola();
        }

        public int Cantidad
        {
            get
            {
                int total = 0;
                foreach (Cola c in colas) total += c.Cantidad;
                return total;
            }
        }

        public int CantidadPorNivel(int nivel) => colas[nivel - 1].Cantidad;

        public void Encolar(Paciente paciente) => colas[paciente.Prioridad - 1].Encolar(paciente);

        public Paciente Desencolar()
        {
            foreach (Cola c in colas)
                if (!c.EstaVacia) return c.Desencolar();
            return null;
        }

        public Paciente Eliminar(int expediente)
        {
            foreach (Cola c in colas)
            {
                Paciente p = c.Eliminar(expediente);
                if (p != null) return p;
            }
            return null;
        }

        public Paciente Buscar(int expediente)
        {
            foreach (Cola c in colas)
            {
                Paciente p = c.Buscar(expediente);
                if (p != null) return p;
            }
            return null;
        }

        public IEnumerator<Paciente> GetEnumerator()
        {
            foreach (Cola c in colas)
                foreach (Paciente p in c)
                    yield return p;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    // ===================== LÓGICA DE LA CLÍNICA =====================
    public class Clinica
    {
        private readonly ColaPrioridad espera = new ColaPrioridad();
        private readonly ListaEnlazada historial = new ListaEnlazada();

        public int CantidadEnEspera => espera.Cantidad;

        public static string NombrePrioridad(int prioridad)
        {
            switch (prioridad)
            {
                case 1: return "Emergencia";
                case 2: return "Urgencia";
                case 3: return "Consulta General";
                case 4: return "Cita Programada";
                default: return "Desconocida";
            }
        }

        public bool Registrar(int expediente, string nombre, int edad, int prioridad)
        {
            if (prioridad < ColaPrioridad.NivelMinimo || prioridad > ColaPrioridad.NivelMaximo) return false;
            if (espera.Buscar(expediente) != null || historial.Buscar(expediente) != null) return false;

            espera.Encolar(new Paciente(expediente, nombre, edad, prioridad));
            return true;
        }

        public bool Eliminar(int expediente) => espera.Eliminar(expediente) != null;

        public Paciente BuscarEnEspera(int expediente) => espera.Buscar(expediente);
        public Paciente BuscarEnHistorial(int expediente) => historial.Buscar(expediente);

        public Paciente AtenderSiguiente()
        {
            Paciente p = espera.Desencolar();
            if (p != null) historial.AgregarAlFinal(p);
            return p;
        }

        // Se saca de su cola actual y entra al final de la cola del nuevo nivel.
        public bool AsignarPrioridad(int expediente, int nuevaPrioridad)
        {
            if (nuevaPrioridad < ColaPrioridad.NivelMinimo || nuevaPrioridad > ColaPrioridad.NivelMaximo) return false;

            Paciente p = espera.Eliminar(expediente);
            if (p == null) return false;

            p.Prioridad = nuevaPrioridad;
            espera.Encolar(p);
            return true;
        }

        public int CantidadPorNivel(int nivel) => espera.CantidadPorNivel(nivel);

        public IEnumerable<Paciente> PacientesEnEspera() => espera;
        public IEnumerable<Paciente> PacientesAtendidos() => historial;
    }

    // ===================== INTERFAZ DE CONSOLA =====================
    internal class Program
    {
        private static readonly Clinica clinica = new Clinica();

        private static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            int opcion;
            do
            {
                MostrarMenu();
                opcion = LeerEntero("Opción: ");
                Console.WriteLine();
                switch (opcion)
                {
                    case 1: Registrar(); break;
                    case 2: Eliminar(); break;
                    case 3: Buscar(); break;
                    case 4: MostrarEspera(); break;
                    case 5: Atender(); break;
                    case 6: AsignarPrioridad(); break;
                    case 7: MostrarHistorial(); break;
                    case 8: ConsultarCantidad(); break;
                    case 0: Console.WriteLine("Saliendo del sistema..."); break;
                    default: Console.WriteLine("Opción inválida."); break;
                }
                if (opcion != 0)
                {
                    Console.WriteLine("\nPresione una tecla para continuar...");
                    Console.ReadKey(true);
                }
            } while (opcion != 0);
        }

        private static void MostrarMenu()
        {
            Console.Clear();
            Console.WriteLine("=== CLÍNICA - GESTIÓN DE PACIENTES ===");
            Console.WriteLine("1. Registrar paciente");
            Console.WriteLine("2. Eliminar paciente");
            Console.WriteLine("3. Buscar paciente por expediente");
            Console.WriteLine("4. Mostrar pacientes en espera");
            Console.WriteLine("5. Atender siguiente paciente");
            Console.WriteLine("6. Asignar prioridad");
            Console.WriteLine("7. Historial de pacientes atendidos");
            Console.WriteLine("8. Cantidad de pacientes en espera");
            Console.WriteLine("0. Salir");
            Console.WriteLine("======================================");
        }

        private static void Registrar()
        {
            int exp = LeerEntero("Expediente: ");
            string nombre = LeerTexto("Nombre: ");
            int edad = LeerEntero("Edad: ");
            int prioridad = LeerPrioridad();

            Console.WriteLine(clinica.Registrar(exp, nombre, edad, prioridad)
                ? "Paciente registrado correctamente."
                : "Error: ya existe un paciente con ese expediente.");
        }

        private static void Eliminar()
        {
            int exp = LeerEntero("Expediente a eliminar: ");
            Console.WriteLine(clinica.Eliminar(exp)
                ? "Paciente eliminado de la lista de espera."
                : "No se encontró el paciente en espera.");
        }

        private static void Buscar()
        {
            int exp = LeerEntero("Expediente a buscar: ");

            Paciente p = clinica.BuscarEnEspera(exp);
            if (p != null) { Console.WriteLine("EN ESPERA -> " + Detalle(p)); return; }

            p = clinica.BuscarEnHistorial(exp);
            Console.WriteLine(p != null ? "ATENDIDO -> " + Detalle(p) : "Paciente no encontrado.");
        }

        private static void MostrarEspera()
        {
            if (clinica.CantidadEnEspera == 0) { Console.WriteLine("No hay pacientes en espera."); return; }

            Console.WriteLine("Pacientes en espera (orden de atención):");
            int n = 1;
            foreach (Paciente p in clinica.PacientesEnEspera())
                Console.WriteLine($"{n++}. {Detalle(p)}");
        }

        private static void Atender()
        {
            Paciente p = clinica.AtenderSiguiente();
            Console.WriteLine(p == null ? "No hay pacientes en espera." : "Atendiendo a: " + Detalle(p));
        }

        private static void AsignarPrioridad()
        {
            int exp = LeerEntero("Expediente: ");
            if (clinica.BuscarEnEspera(exp) == null)
            {
                Console.WriteLine("No se encontró el paciente en espera.");
                return;
            }
            clinica.AsignarPrioridad(exp, LeerPrioridad());
            Console.WriteLine("Prioridad actualizada.");
        }

        private static void MostrarHistorial()
        {
            Console.WriteLine("Historial de pacientes atendidos:");
            bool hay = false;
            foreach (Paciente p in clinica.PacientesAtendidos())
            {
                hay = true;
                Console.WriteLine("- " + Detalle(p));
            }
            if (!hay) Console.WriteLine("(vacío)");
        }

        private static void ConsultarCantidad()
        {
            Console.WriteLine($"Total en espera: {clinica.CantidadEnEspera}");
            for (int nivel = ColaPrioridad.NivelMinimo; nivel <= ColaPrioridad.NivelMaximo; nivel++)
                Console.WriteLine($"  {Clinica.NombrePrioridad(nivel)}: {clinica.CantidadPorNivel(nivel)}");
        }

        // ---------- Helpers ----------
        private static string Detalle(Paciente p) =>
            p + $" ({Clinica.NombrePrioridad(p.Prioridad)})";

        private static string LeerTexto(string mensaje)
        {
            string s;
            do
            {
                Console.Write(mensaje);
                s = Console.ReadLine()?.Trim();
            } while (string.IsNullOrEmpty(s));
            return s;
        }

        private static int LeerEntero(string mensaje)
        {
            int valor;
            Console.Write(mensaje);
            while (!int.TryParse(Console.ReadLine(), out valor) || valor < 0)
                Console.Write("Valor inválido. " + mensaje);
            return valor;
        }

        private static int LeerPrioridad()
        {
            Console.WriteLine("Prioridad:");
            Console.WriteLine("  1. Emergencia");
            Console.WriteLine("  2. Urgencia");
            Console.WriteLine("  3. Consulta General");
            Console.WriteLine("  4. Cita Programada");
            int op;
            do { op = LeerEntero("Seleccione (1-4): "); } while (op < 1 || op > 4);
            return op;
        }
    }
}
