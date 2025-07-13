do language plpgsql $$
declare
    table_name text;
begin
    for table_name in (
        select relname
        from pg_class
            join pg_namespace on pg_class.relnamespace = pg_namespace.oid
        where nspname = 'public' and relkind = 'r'
    ) loop
        execute 'drop table if exists public.' || quote_ident(table_name) || ' cascade';
    end loop;
end;
$$;
