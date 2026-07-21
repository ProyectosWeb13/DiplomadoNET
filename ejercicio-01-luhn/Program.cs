// See https://aka.ms/new-console-template for more information
using System;
using System.IO;

using System;
using System.IO;

class Program
{
    // Acumuladores globales para el reporte de estadísticas
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

        // Bucle para mantener el menú activo hasta que el usuario elija salir
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
                // Captura de errores generales para evitar que el programa se cierre
                Console.WriteLine($"Ocurrió un error inesperado: {ex.Message}");
            }

            if (opcion != "5")
            {
                Console.WriteLine("\nPresione cualquier tecla para continuar...");
                Console.ReadKey();
            }

        } while (opcion != "5");
    }

    // Algoritmo de Luhn para validar el número de tarjeta
    public static bool ValidarTarjeta(string numero)
    {
        if (string.IsNullOrWhiteSpace(numero)) return false;

        int suma = 0;
        bool duplicar = false;

        // Recorrido de derecha a izquierda (desde el último dígito)
        for (int i = numero.Length - 1; i >= 0; i--)
        {
            if (!char.IsDigit(numero[i])) return false; // descarta si la cadena trae letras o símbolos

            int digito = (int)char.GetNumericValue(numero[i]);

            // Se duplica uno sí y uno no
            if (duplicar)
            {
                digito *= 2;
                if (digito > 9)
                {
                    digito -= 9; // Si supera 9, resta 9 para obtener la suma de sus dígitos
                }
            }

            suma += digito;
            duplicar = !duplicar; // Alterna la bandera para la siguiente iteración
        }

        // Si la suma total es múltiplo de 10, la tarjeta es válida
        return (suma % 10 == 0);
    }

    // Identifica la franquicia según el prefijo y la cantidad de dígitos
    public static string IdentificarMarca(string numero)
    {
        int longitud = numero.Length;

        // Visa: Empieza en 4 y tiene 13 o 16 dígitos
        if (numero.StartsWith("4") && (longitud == 13 || longitud == 16))
        {
            return "Visa";
        }

        // American Express: Empieza en 34 o 37 y tiene 15 dígitos
        if ((numero.StartsWith("34") || numero.StartsWith("37")) && longitud == 15)
        {
            return "American Express";
        }

        // Mastercard: 16 dígitos y prefijo entre 51 y 55
        if (longitud == 16)
        {
            int prefijo2 = int.Parse(numero.Substring(0, 2));
            if (prefijo2 >= 51 && prefijo2 <= 55)
            {
                return "Mastercard";
            }
        }

        // Discover: Rangos específicos y longitud de 16 a 19 dígitos
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

    // Lee un archivo de texto con un número de tarjeta por línea
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

    // Genera una tarjeta de prueba válida encontrando el último dígito mediante Luhn
    public static string GenerarNumeroValido()
    {
        string baseTarjeta = "453201511283036"; // Base fija tipo Visa (15 dígitos)

        // Prueba del 0 al 9 en el último carácter hasta dar con la combinación válida
        for (int i = 0; i <= 9; i++)
        {
            string prueba = baseTarjeta + i;
            if (ValidarTarjeta(prueba))
            {
                return prueba;
            }
        }

        return "4532015112830366";
    }

    private static void ProcesarTarjetaIndividual()
    {
        Console.Write("Número: ");
        string numero = Console.ReadLine()?.Trim();
        RegistrarYMostrarTarjeta(numero);
    }

    private static void GenerarYMostrarTarjetaValida()
    {
        string nuevaTarjeta = GenerarNumeroValido();
        Console.WriteLine("¡Tarjeta Generada con Éxito!");
        RegistrarYMostrarTarjeta(nuevaTarjeta);
    }

    // Muestra los datos de la tarjeta en consola e incrementa los contadores
    private static void RegistrarYMostrarTarjeta(string numero)
    {
        bool esValida = ValidarTarjeta(numero);
        string marca = IdentificarMarca(numero);

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

        Console.WriteLine($"\nNúmero: {numero}");
        Console.WriteLine($"Marca:  {marca}");
        Console.WriteLine($"Estado: {(esValida ? "✅ VÁLIDA" : "❌ INVÁLIDA")}");
    }

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
