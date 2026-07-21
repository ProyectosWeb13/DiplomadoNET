// See https://aka.ms/new-console-template for more information
using System;
using System.IO;

class Program
{
    // Variables globales para el módulo de Estadísticas
    static int totalValidas = 0;
    static int totalInvalidas = 0;
    static int cantidadVisa = 0;
    static int cantidadMastercard = 0;
    static int cantidadAmex = 0;
    static int cantidadDiscover = 0;
    static int cantidadDesconocida = 0;

    static void Main(string[] args)
    {
        string opcion;

        // Menú principal con ciclo do-while
        do
        {
            Console.Clear();
            Console.WriteLine("==== VALIDADOR DE TARJETAS ===");
            Console.WriteLine("1. Validar una tarjeta");
            Console.WriteLine("2. Validar desde archivo");
            Console.WriteLine("3. Generar número válido");
            Console.WriteLine("4. Estadísticas");
            Console.WriteLine("5. Salir");
            Console.Write("\nSeleccione una opción: ");
            
            opcion = Console.ReadLine();
            Console.WriteLine();

            try
            {
                switch (opcion)
                {
                    case "1":
                        ProcesarTarjetaIndividual();
                        break;
                    case "2":
                        Console.Write("Ingrese la ruta del archivo (.txt): ");
                        string ruta = Console.ReadLine();
                        ValidarDesdeArchivo(ruta);
                        break;
                    case "3":
                        GenerarYMostrarTarjetaValida();
                        break;
                    case "4":
                        MostrarEstadisticas();
                        break;
                    case "5":
                        Console.WriteLine("¡Gracias por usar el sistema!");
                        break;
                    default:
                        Console.WriteLine("Opción no válida. Intente de nuevo.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocurrió un error inesperado: {ex.Message}");
            }

            if (opcion != "5")
            {
                Console.WriteLine("\nPresione cualquier tecla para continuar...");
                Console.ReadKey();
            }

        } while (opcion != "5");
    }

    // REQUISITO 1: Algoritmo de Luhn (30%)
    public static bool ValidarTarjeta(string numero)
    {
        if (string.IsNullOrWhiteSpace(numero)) return false;

        int suma = 0;
        bool duplicar = false;

        for (int i = numero.Length - 1; i >= 0; i--)
        {
            if (!char.IsDigit(numero[i])) return false; // descarta caracteres no numéricos

            int digito = (int)char.GetNumericValue(numero[i]);

            if (duplicar)
            {
                digito *= 2;
                if (digito > 9)
                {
                    digito -= 9;
                }
            }

            suma += digito;
            duplicar = !duplicar;
        }

        return (suma % 10 == 0);
    }

    // REQUISITO 2: Identificación de Marcas (15%)
    public static string IdentificarMarca(string numero)
    {
        int longitud = numero.Length;

        // Visa
        if (numero.StartsWith("4") && (longitud == 13 || longitud == 16))
        {
            return "Visa";
        }

        // American Express
        if ((numero.StartsWith("34") || numero.StartsWith("37")) && longitud == 15)
        {
            return "American Express";
        }

        // Mastercard
        if (longitud == 16)
        {
            int prefijo2 = int.Parse(numero.Substring(0, 2));
            if (prefijo2 >= 51 && prefijo2 <= 55)
            {
                return "Mastercard";
            }
        }

        // Discover
        if (longitud >= 16 && longitud <= 19)
        {
            if (numero.StartsWith("6011") || numero.StartsWith("65"))
            {
                return "Discover";
            }

            int prefijo3 = int.Parse(numero.Substring(0, 3));
            if (prefijo3 >= 644 && prefijo3 <= 649)
            {
                return "Discover";
            }

            int prefijo6 = int.Parse(numero.Substring(0, 6));
            if (prefijo6 >= 622126 && prefijo6 <= 622925)
            {
                return "Discover";
            }
        }

        return "Desconocida";
    }

    // REQUISITO 3: Validar desde archivo (10%)
    public static void ValidarDesdeArchivo(string ruta)
    {
        try
        {
            if (!File.Exists(ruta))
            {
                Console.WriteLine("Error: El archivo especificado no existe.");
                return;
            }

            string[] lineas = File.ReadAllLines(ruta);
            Console.WriteLine($"\n--- PROCESANDO ARCHIVO ({lineas.Length} registros) ---");

            foreach (string linea in lineas)
            {
                string tarjeta = linea.Trim();
                if (!string.IsNullOrEmpty(tarjeta))
                {
                    RegistrarYMostrarTarjeta(tarjeta);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al leer el archivo: {ex.Message}");
        }
    }

    // REQUISITO 4: Generar número válido (10%)
    public static string GenerarNumeroValido()
    {
        Random random = new Random();
        string baseTarjeta = "453201511283036"; // Base fija tipo Visa (15 dígitos)

        for (int i = 0; i <= 9; i++)
        {
            string prueba = baseTarjeta + i;
            if (ValidarTarjeta(prueba))
            {
                return prueba; // Retorna la combinación que hace que el dígito de verificación sea correcto
            }
        }

        return "4532015112830366";
    }

    // Método auxiliar opción 1
    private static void ProcesarTarjetaIndividual()
    {
        Console.Write("Número: ");
        string numero = Console.ReadLine()?.Trim();
        RegistrarYMostrarTarjeta(numero);
    }

    // Método auxiliar opción 3
    private static void GenerarYMostrarTarjetaValida()
    {
        string nuevaTarjeta = GenerarNumeroValido();
        Console.WriteLine("¡Tarjeta Generada con Éxito!");
        RegistrarYMostrarTarjeta(nuevaTarjeta);
    }

    // Método auxiliar para imprimir resultado y acumular Estadísticas (10%)
    private static void RegistrarYMostrarTarjeta(string numero)
    {
        bool esValida = ValidarTarjeta(numero);
        string marca = IdentificarMarca(numero);

        // Actualizar contadores
        if (esValida) totalValidas++;
        else totalInvalidas++;

        switch (marca)
        {
            case "Visa": cantidadVisa++; break;
            case "Mastercard": cantidadMastercard++; break;
            case "American Express": cantidadAmex++; break;
            case "Discover": cantidadDiscover++; break;
            default: cantidadDesconocida++; break;
        }

        // Formato de salida según el PDF
        Console.WriteLine($"\nNúmero: {numero}");
        Console.WriteLine($"Marca:  {marca}");
        Console.WriteLine($"Estado: {(esValida ? "✅ VÁLIDA" : "❌ INVÁLIDA")}");
    }

    // REQUISITO 5: Estadísticas acumuladas
    private static void MostrarEstadisticas()
    {
        Console.WriteLine("=== ESTADÍSTICAS DEL SISTEMA ===");
        Console.WriteLine($"Tarjetas Válidas:   {totalValidas}");
        Console.WriteLine($"Tarjetas Inválidas: {totalInvalidas}");
        Console.WriteLine($"Total Procesadas:   {totalValidas + totalInvalidas}");
        Console.WriteLine("--------------------------------");
        Console.WriteLine("Desglose por Marca:");
        Console.WriteLine($" - Visa:             {cantidadVisa}");
        Console.WriteLine($" - Mastercard:       {cantidadMastercard}");
        Console.WriteLine($" - American Express: {cantidadAmex}");
        Console.WriteLine($" - Discover:         {cantidadDiscover}");
        Console.WriteLine($" - Desconocida:      {cantidadDesconocida}");
    }
}

