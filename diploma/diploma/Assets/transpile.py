import sys
import sqlglot


def get_parameters():
    parameters = {}
    for i in range(1, len(sys.argv)):
        if sys.argv[i] == "--sourceDialect":
            parameters["sourceDialect"] = sys.argv[i + 1]
        elif sys.argv[i] == "--targetDialect":
            parameters["targetDialect"] = sys.argv[i + 1]
        elif sys.argv[i] == "--sql":
            parameters["sql"] = sys.argv[i + 1]

    if "sql" not in parameters:
        sql = ""
        for line in sys.stdin:
            sql += line
        parameters["sql"] = sql
    return parameters


def main():
    parameters = get_parameters()
    sql = parameters["sql"]
    source_dialect = parameters["sourceDialect"]
    target_dialect = parameters["targetDialect"]

    for line in sqlglot.transpile(sql, read=source_dialect, write=target_dialect, pretty=True):
        print(line, end=";\n")


if __name__ == "__main__":
    main()
