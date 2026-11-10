using System;
using System.Text.Json;
/*
 * Code fourni par Gemini
 * Voici une classe utilitaire JsonHelper.cs propre, réutilisable et compatible avec ton environnement .NET Native AOT grâce à l'utilisation de System.Text.Json.
 */

namespace PureJNI
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        // Encode n'importe quel objet en JSON
        public static string JsonEncode<T>(T obj)
        {
            try
            {
                return JsonSerializer.Serialize(obj, _options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JsonHelper] Encode error: {ex.Message}");
                return string.Empty;
            }
        }

        // Décode un JSON en objet typé
        public static T? JsonDecode<T>(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(json, _options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JsonHelper] Decode error: {ex.Message}");
                return default;
            }
        }

        // SUPER SIMPLE : Récupère directement la valeur d'une clé (ex: "timer") depuis un JSON
        public static T? GetField<T>(string json, string key)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty(key, out var element))
                {
                    // Convertit automatiquement dans le type demandé (ex: string, int, float)
                    return element.Deserialize<T>(_options);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JsonHelper] GetField error: {ex.Message}");
            }
            return default;
        }
    }
}