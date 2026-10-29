using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ClinicaPacientes
{
    // ===================== MODELO =====================
    public enum Prioridad
    {
        Emergencia = 1,
        Urgencia = 2,
        ConsultaGeneral = 3,
        CitaProgramada = 4
    }

    public class Paciente
    {
        public string Expediente { get; }
        public string Nombre { get; }
        public int Edad { get; }
        public string Motivo { get; }
        public Prioridad Prioridad { get; set; }
        public DateTime FechaRegistro { get; }
        public DateTime? FechaAtencion { get; set; }

        public Paciente(string expediente, string nombre, int edad, string motivo, Prioridad prioridad)
        {
            Expediente = expediente;
            Nombre = nombre;
            Edad = edad;
            Motivo = motivo;
            Prioridad = prioridad;
            FechaRegistro = DateTime.Now;
        }

        public override string ToString() =>
            $"[{Expediente}] {Nombre} ({Edad} años) - {Prioridad} - Motivo: {Motivo}";
    }

    // ===================== LISTA ENLAZADA =====================
    public class Nodo<T>
    {
        public T Dato;
        public Nodo<T> Siguiente;
        public Nodo(T dato) { Dato = dato; }
    }

    public class ListaEnlazada<T> : IEnumerable<T> where T : class
    {
        private Nodo<T> cabeza;
        private Nodo<T> cola;
        public int Cantidad { get; private set; }

        public void AgregarAlFinal(T dato)
        {
            var nuevo = new Nodo<T>(dato);
            if (cabeza == null) cabeza = cola = nuevo;
            else { cola.Siguiente = nuevo; cola = nuevo; }
            Cantidad++;
        }

        public T RemoverPrimero()
        {
            if (cabeza == null) return null;
            T dato = cabeza.Dato;
            cabeza = cabeza.Siguiente;
            if (cabeza == null) cola = null;
            Cantidad--;
            return dato;
        }

        public bool Eliminar(Predicate<T> criterio, out T eliminado)
        {
            eliminado = null;
            Nodo<T> previo = null, actual = cabeza;
            while (actual != null)
            {
                if (criterio(actual.Dato))
                {
                    if (previo == null) cabeza = actual.Siguiente;
                    else previo.Siguiente = actual.Siguiente;
                    if (actual == cola) cola = previo;
                    Cantidad--;
                    eliminado = actual.Dato;
                    return true;
                }
                previo = actual;
                actual = actual.Siguiente;
            }
            return false;
        }

        public T Buscar(Predicate<T> criterio)
        {
            for (var n = cabeza; n != null; n = n.Siguiente)
                if (criterio(n.Dato)) return n.Dato;
            return null;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (var n = cabeza; n != null; n = n.Siguiente)
                yield return n.Dato;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    // ===================== COLA (FIFO) =====================
    public class Cola<T> : IEnumerable<T> where T : class
    {
        private readonly ListaEnlazada<T> lista = new ListaEnlazada<T>();

        public int Cantidad => lista.Cantidad;
        public bool EstaVacia => lista.Cantidad == 0;

        public void Encolar(T dato) => lista.AgregarAlFinal(dato);
        public T Desencolar() => lista.RemoverPrimero();
        public bool Eliminar(Predicate<T> criterio, out T eliminado) => lista.Eliminar(criterio, out eliminado);
        public T Buscar(Predicate<T> criterio) => lista.Buscar(criterio);

        public IEnumerator<T> GetEnumerator() => lista.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    // ===================== COLA DE PRIORIDAD =====================
    // Una cola FIFO por cada nivel. Se atiende siempre el nivel más urgente
    // que tenga pacientes; dentro del mismo nivel, por orden de llegada.
    public class ColaPrioridad : IEnumerable<Paciente>
    {
        private readonly Cola<Paciente>[] colas;

        public ColaPrioridad()
        {
            int niveles = Enum.GetValues(typeof(Prioridad)).Length;
            colas = new Cola<Paciente>[niveles];
            for (int i = 0; i < niveles; i++) colas[i] = new Cola<Paciente>();
        }

        public int Cantidad
        {
            get { int total = 0; foreach (var c in colas) total += c.Cantidad; return total; }
        }

        public void Encolar(Paciente p) => colas[(int)p.Prioridad - 1].Encolar(p);

        public Paciente Desencolar()
        {
            foreach (var c in colas)
                if (!c.EstaVacia) return c.Desencolar();
            return null;
        }

        public Paciente Eliminar(string expediente)
        {
            foreach (var c in colas)
                if (c.Eliminar(p => p.Expediente == expediente, out Paciente eliminado))
                    return eliminado;
            return null;
        }

        public Paciente Buscar(string expediente)
        {
            foreach (var c in colas)
            {
                var p = c.Buscar(x => x.Expediente == expediente);
                if (p != null) return p;
            }
            return null;
        }

        public int CantidadPorNivel(Prioridad nivel) => colas[(int)nivel - 1].Cantidad;

        public IEnumerator<Paciente> GetEnumerator()
        {
            foreach (var c in colas)
                foreach (var p in c)
                    yield return p;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    // ===================== LÓGICA DE LA CLÍNICA =====================
    public class Clinica
    {
        private readonly ColaPrioridad espera = new ColaPrioridad();
        private readonly ListaEnlazada<Paciente> historial = new ListaEnlazada<Paciente>();

        public int CantidadEnEspera => espera.Cantidad;

        public bool Registrar(string expediente, string nombre, int edad, string motivo, Prioridad prioridad)
        {
            if (ExisteExpediente(expediente)) return false;
            espera.Encolar(new Paciente(expediente, nombre, edad, motivo, prioridad));
            return true;
        }

        public bool Eliminar(string expediente) => espera.Eliminar(expediente) != null;

        public Paciente BuscarEnEspera(string expediente) => espera.Buscar(expediente);

        public Paciente BuscarEnHistorial(string expediente) =>
            historial.Buscar(p => p.Expediente == expediente);

        public Paciente AtenderSiguiente()
        {
            Paciente p = espera.Desencolar();
            if (p == null) return null;
            p.FechaAtencion = DateTime.Now;
            historial.AgregarAlFinal(p);
            return p;
        }

        // Cambia el nivel: se saca de su cola actual y entra al final del nuevo nivel.
        public bool AsignarPrioridad(string expediente, Prioridad nueva)
        {
            Paciente p = espera.Eliminar(expediente);
            if (p == null) return false;
            p.Prioridad = nueva;
            espera.Encolar(p);
            return true;
        }

        public IEnumerable<Paciente> PacientesEnEspera() => espera;
        public IEnumerable<Paciente> PacientesAtendidos() => historial;

        public int CantidadPorNivel(Prioridad nivel) => espera.CantidadPorNivel(nivel);

        private bool ExisteExpediente(string exp) =>
            espera.Buscar(exp) != null || BuscarEnHistorial(exp) != null;
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
            string exp = LeerTexto("Expediente: ");
            string nombre = LeerTexto("Nombre: ");
            int edad = LeerEntero("Edad: ");
            string motivo = LeerTexto("Motivo de consulta: ");
            Prioridad prioridad = LeerPrioridad();

            Console.WriteLine(clinica.Registrar(exp, nombre, edad, motivo, prioridad)
                ? "Paciente registrado correctamente."
                : "Error: ya existe un paciente con ese expediente.");
        }

        private static void Eliminar()
        {
            string exp = LeerTexto("Expediente a eliminar: ");
            Console.WriteLine(clinica.Eliminar(exp)
                ? "Paciente eliminado de la lista de espera."
                : "No se encontró el paciente en espera.");
        }

        private static void Buscar()
        {
            string exp = LeerTexto("Expediente a buscar: ");
            Paciente p = clinica.BuscarEnEspera(exp);
            if (p != null) { Console.WriteLine("EN ESPERA: " + p); return; }

            p = clinica.BuscarEnHistorial(exp);
            if (p != null)
                Console.WriteLine($"ATENDIDO ({p.FechaAtencion:dd/MM/yyyy HH:mm}): {p}");
            else
                Console.WriteLine("Paciente no encontrado.");
        }

        private static void MostrarEspera()
        {
            if (clinica.CantidadEnEspera == 0) { Console.WriteLine("No hay pacientes en espera."); return; }
            Console.WriteLine("Pacientes en espera (orden de atención):");
            int n = 1;
            foreach (var p in clinica.PacientesEnEspera())
                Console.WriteLine($"{n++}. {p}");
        }

        private static void Atender()
        {
            Paciente p = clinica.AtenderSiguiente();
            Console.WriteLine(p == null
                ? "No hay pacientes en espera."
                : "Atendiendo a: " + p);
        }

        private static void AsignarPrioridad()
        {
            string exp = LeerTexto("Expediente: ");
            if (clinica.BuscarEnEspera(exp) == null)
            {
                Console.WriteLine("No se encontró el paciente en espera.");
                return;
            }
            Prioridad nueva = LeerPrioridad();
            clinica.AsignarPrioridad(exp, nueva);
            Console.WriteLine("Prioridad actualizada.");
        }

        private static void MostrarHistorial()
        {
            bool hay = false;
            Console.WriteLine("Historial de pacientes atendidos:");
            foreach (var p in clinica.PacientesAtendidos())
            {
                hay = true;
                Console.WriteLine($"- {p} | Atendido: {p.FechaAtencion:dd/MM/yyyy HH:mm}");
            }
            if (!hay) Console.WriteLine("(vacío)");
        }

        private static void ConsultarCantidad()
        {
            Console.WriteLine($"Total en espera: {clinica.CantidadEnEspera}");
            foreach (Prioridad nivel in Enum.GetValues(typeof(Prioridad)))
                Console.WriteLine($"  {nivel}: {clinica.CantidadPorNivel(nivel)}");
        }

        // ---------- Helpers de entrada ----------
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

        private static Prioridad LeerPrioridad()
        {
            Console.WriteLine("Prioridad:");
            Console.WriteLine("  1. Emergencia");
            Console.WriteLine("  2. Urgencia");
            Console.WriteLine("  3. Consulta General");
            Console.WriteLine("  4. Cita Programada");
            int op;
            do { op = LeerEntero("Seleccione (1-4): "); } while (op < 1 || op > 4);
            return (Prioridad)op;
        }
    }
}
