
DECLARE
    tableName VARCHAR2(255);
    constraintName VARCHAR2(255);
BEGIN
    FOR constraintRec IN (SELECT table_name, constraint_name
                        FROM user_constraints
                        WHERE constraint_type = 'R') LOOP
    EXECUTE IMMEDIATE 'ALTER TABLE ' || constraintRec.table_name || ' DROP CONSTRAINT ' || constraintRec.constraint_name;
    END LOOP;

    FOR tableRec IN (SELECT table_name FROM user_tables) LOOP
    EXECUTE IMMEDIATE 'DROP TABLE ""' || tableRec.table_name || '""';
    END LOOP;
END;
