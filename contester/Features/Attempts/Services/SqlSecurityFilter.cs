using System;
using System.Text.RegularExpressions;

public class SqlSecurityFilter
{
    // Список запрещенных ключевых слов
    private static readonly string[] ForbiddenKeywords = new string[]
    {
        // DDL
        "CREATE", "ALTER", "DROP", "TRUNCATE", "RENAME",
        // DML
        "INSERT", "UPDATE", "DELETE", "MERGE",
        // Transaction
        "COMMIT", "ROLLBACK", "BEGIN", "TRANSACTION", "SAVEPOINT",
        // Execution & Admin
        "EXEC", "EXECUTE", "CALL", "GRANT", "REVOKE", "DENY",
        // PG Specific
        "COPY", "DO", "LISTEN", "NOTIFY", "LOAD",
        // Oracle/General Dangerous Prefixes (handled via regex logic mostly, but keywords here)
        "SHUTDOWN" 
    };

    // Регулярное выражение для поиска целых слов (case-insensitive)
    // \b означает границу слова
    private static Regex _forbiddenRegex;

    static SqlSecurityFilter()
    {
        // Создаем паттерн вида: \b(CREATE|ALTER|...)\b
        string pattern = @"\b(" + string.Join("|", ForbiddenKeywords) + @")\b";
        _forbiddenRegex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    /// <summary>
    /// Проверяет SQL запрос на наличие запрещенных операций.
    /// Возвращает true, если запрос БЕЗОПАСЕН (не содержит запрещенных слов).
    /// </summary>
    public static bool IsSafe(string sqlQuery)
    {
        if (string.IsNullOrWhiteSpace(sqlQuery))
            return false;

        // Дополнительная проверка на специфичные опасные префиксы, которые сложно ловить словами
        // Например, вызовы системных пакетов в Oracle или XP в MSSQL
        
        // Проверка на dbms_, utl_, sys. (Oracle)
        if (Regex.IsMatch(sqlQuery, @"\b(DBMS|UTL|SYS)\b\.", RegexOptions.IgnoreCase))
            return false;

        // Проверка на xp_ (SQL Server)
        if (Regex.IsMatch(sqlQuery, @"\bxp_\w+", RegexOptions.IgnoreCase))
            return false;
            
        // Проверка на многострочные комментарии, которые могут скрывать код (опционально)
        // Иногда разрешают комментарии, но если там спрятан DROP TABLE, простой фильтр слов может пропустить,
        // если он не учитывает вложенность. Но для базовой защиты достаточно проверки ключевых слов.
        
        return !_forbiddenRegex.IsMatch(sqlQuery);
    }
}