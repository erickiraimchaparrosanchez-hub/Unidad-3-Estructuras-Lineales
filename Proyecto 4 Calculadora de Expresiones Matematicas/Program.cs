using System;
using System.Collections.Generic;
using System.Globalization;

namespace CalculadoraExpresiones
{
    // ===== Pila genérica con nodos enlazados =====
    public class Pila<T>
    {
        private class Nodo
        {
            public T Dato;
            public Nodo Siguiente;
        }

        private Nodo cima;
        public int Cantidad { get; private set; }
        public bool EstaVacia => cima == null;

        public void Push(T dato)
        {
            cima = new Nodo { Dato = dato, Siguiente = cima };
            Cantidad++;
        }

        public T Pop()
        {
            if (cima == null)
                throw new InvalidOperationException("La pila está vacía.");
            T dato = cima.Dato;
            cima = cima.Siguiente;
            Cantidad--;
            return dato;
        }

        public T Peek()
        {
            if (cima == null)
                throw new InvalidOperationException("La pila está vacía.");
            return cima.Dato;
        }
    }

    // ===== Calculadora =====
    public class Calculadora
    {
        private const string Operadores = "+-*/%^";

        private static bool EsOperadorBinario(string t)
        {
            return t.Length == 1 && Operadores.IndexOf(t[0]) >= 0;
        }

        private static bool EsNumero(string t)
        {
            return char.IsDigit(t[0]) || t[0] == '.';
        }

        // Precedencia: + - (1) < * / % (2) < negativo unario ~ (3) < ^ (4)
        private static int Precedencia(string op)
        {
            switch (op)
            {
                case "+":
                case "-": return 1;
                case "*":
                case "/":
                case "%": return 2;
                case "~": return 3;
                case "^": return 4;
                default: return 0;
            }
        }

        private static bool EsAsociativoDerecha(string op)
        {
            return op == "^" || op == "~";
        }

        // 1) Separar la expresión en tokens
        private bool Tokenizar(string expr, out List<string> tokens, out string error)
        {
            tokens = new List<string>();
            error = null;

            if (string.IsNullOrWhiteSpace(expr))
            {
                error = "La expresión está vacía.";
                return false;
            }

            int i = 0;
            while (i < expr.Length)
            {
                char c = expr[i];

                if (char.IsWhiteSpace(c)) { i++; continue; }

                if (char.IsDigit(c) || c == '.')
                {
                    int inicio = i;
                    while (i < expr.Length && (char.IsDigit(expr[i]) || expr[i] == '.')) i++;
                    string num = expr.Substring(inicio, i - inicio);
                    double tmp;
                    if (!double.TryParse(num, NumberStyles.AllowDecimalPoint,
                                         CultureInfo.InvariantCulture, out tmp))
                    {
                        error = "Número inválido: '" + num + "'.";
                        return false;
                    }
                    tokens.Add(num);
                    continue;
                }

                if (c == '(' || c == ')')
                {
                    tokens.Add(c.ToString());
                    i++;
                    continue;
                }

                if (Operadores.IndexOf(c) >= 0)
                {
                    string ultimo = tokens.Count > 0 ? tokens[tokens.Count - 1] : null;
                    bool esUnario = ultimo == null || ultimo == "(" || ultimo == "~"
                                    || EsOperadorBinario(ultimo);

                    if (esUnario && c == '-') tokens.Add("~");   // negativo unario
                    else if (esUnario && c == '+') { }           // positivo unario: se ignora
                    else tokens.Add(c.ToString());
                    i++;
                    continue;
                }

                error = "Carácter no válido: '" + c + "'.";
                return false;
            }
            return true;
        }

        // 2) VALIDAR EXPRESIÓN (caracteres, paréntesis, operadores y operandos)
        public bool Validar(string expr, out List<string> tokens, out string error)
        {
            if (!Tokenizar(expr, out tokens, out error))
                return false;

            bool esperaOperando = true;
            int profundidad = 0;

            foreach (string t in tokens)
            {
                if (t == "~")
                {
                    // prefijo: sigue esperando operando
                }
                else if (EsNumero(t))
                {
                    if (!esperaOperando)
                    {
                        error = "Falta un operador antes de '" + t + "'.";
                        return false;
                    }
                    esperaOperando = false;
                }
                else if (t == "(")
                {
                    if (!esperaOperando)
                    {
                        error = "Falta un operador antes de '('.";
                        return false;
                    }
                    profundidad++;
                }
                else if (t == ")")
                {
                    if (esperaOperando)
                    {
                        error = "Expresión incompleta antes de ')' (paréntesis vacío u operador sin operando).";
                        return false;
                    }
                    profundidad--;
                    if (profundidad < 0)
                    {
                        error = "Paréntesis de cierre sin su apertura.";
                        return false;
                    }
                }
                else // operador binario
                {
                    if (esperaOperando)
                    {
                        error = "El operador '" + t + "' no tiene operando izquierdo.";
                        return false;
                    }
                    esperaOperando = true;
                }
            }

            if (profundidad != 0)
            {
                error = "Paréntesis sin cerrar.";
                return false;
            }
            if (esperaOperando)
            {
                error = "La expresión termina en un operador.";
                return false;
            }
            return true;
        }

        // 3) CONVERTIR INFIJA -> POSTFIJA (algoritmo Shunting-Yard con una Pila)
        public List<string> ConvertirAPostfija(List<string> tokens)
        {
            var salida = new List<string>();
            var pila = new Pila<string>();

            foreach (string t in tokens)
            {
                if (EsNumero(t))
                {
                    salida.Add(t);
                }
                else if (t == "(" || t == "~")
                {
                    pila.Push(t);
                }
                else if (t == ")")
                {
                    while (!pila.EstaVacia && pila.Peek() != "(")
                        salida.Add(pila.Pop());
                    pila.Pop(); // descarta "("
                }
                else // operador binario
                {
                    while (!pila.EstaVacia && pila.Peek() != "(" &&
                           (Precedencia(pila.Peek()) > Precedencia(t) ||
                            (Precedencia(pila.Peek()) == Precedencia(t) && !EsAsociativoDerecha(t))))
                    {
                        salida.Add(pila.Pop());
                    }
                    pila.Push(t);
                }
            }

            while (!pila.EstaVacia)
                salida.Add(pila.Pop());

            return salida;
        }

        // 4) EVALUAR POSTFIJA (con una Pila de números)
        public double EvaluarPostfija(List<string> postfija)
        {
            var pila = new Pila<double>();

            foreach (string t in postfija)
            {
                if (t == "~")
                {
                    pila.Push(-pila.Pop());
                }
                else if (EsOperadorBinario(t))
                {
                    double b = pila.Pop();
                    double a = pila.Pop();
                    pila.Push(Aplicar(a, b, t[0]));
                }
                else
                {
                    pila.Push(double.Parse(t, NumberStyles.AllowDecimalPoint,
                                           CultureInfo.InvariantCulture));
                }
            }

            if (pila.Cantidad != 1)
                throw new InvalidOperationException("Expresión postfija mal formada.");

            return pila.Pop();
        }

        // 5) Operaciones + detección de errores matemáticos
        private double Aplicar(double a, double b, char op)
        {
            double r;
            switch (op)
            {
                case '+': r = a + b; break;
                case '-': r = a - b; break;
                case '*': r = a * b; break;
                case '/':
                    if (b == 0) throw new DivideByZeroException("División entre cero.");
                    r = a / b;
                    break;
                case '%':
                    if (b == 0) throw new DivideByZeroException("Módulo entre cero.");
                    r = a % b;
                    break;
                case '^': r = Math.Pow(a, b); break;
                default: throw new InvalidOperationException("Operador desconocido: " + op);
            }

            if (double.IsNaN(r) || double.IsInfinity(r))
                throw new ArithmeticException("El resultado no es un número real o es demasiado grande.");

            return r;
        }
    }

    // ===== Programa principal =====
    class Program
    {
        static void Main()
        {
            var calc = new Calculadora();

            Console.WriteLine("=== CALCULADORA DE EXPRESIONES MATEMÁTICAS (Pilas) ===");
            Console.WriteLine("Operadores: + - * / % ^    Paréntesis: ( )");
            Console.WriteLine("Ejemplo: (5+3)*2   |   Escribe 'salir' para terminar.");
            Console.WriteLine("(En la postfija, '~' representa el negativo unario)\n");

            while (true)
            {
                Console.Write("Expresión infija: ");
                string entrada = Console.ReadLine();

                if (entrada == null || entrada.Trim().ToLower() == "salir")
                    break;

                try
                {
                    List<string> tokens;
                    string error;

                    if (!calc.Validar(entrada, out tokens, out error))
                    {
                        Console.WriteLine("  Error: " + error + "\n");
                        continue;
                    }

                    List<string> postfija = calc.ConvertirAPostfija(tokens);
                    Console.WriteLine("  Postfija: " + string.Join(" ", postfija));

                    double resultado = calc.EvaluarPostfija(postfija);
                    Console.WriteLine("  Resultado = " +
                        resultado.ToString("G15", CultureInfo.InvariantCulture) + "\n");
                }
                catch (DivideByZeroException ex)
                {
                    Console.WriteLine("  Error matemático: " + ex.Message + "\n");
                }
                catch (ArithmeticException ex)
                {
                    Console.WriteLine("  Error matemático: " + ex.Message + "\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("  Error: " + ex.Message + "\n");
                }
            }

            Console.WriteLine("¡Hasta luego!");
        }
    }
}
