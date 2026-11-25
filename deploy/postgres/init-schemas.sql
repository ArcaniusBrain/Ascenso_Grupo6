-- Inicialización de schemas y roles aislados por microservicio (ADR-05, v1.3 §7.1).
-- Ningún servicio puede leer ni escribir tablas en el schema de otro servicio.
-- IMPORTANTE: para entornos de servidor/producción, rotar las contraseñas fuera de Git.

REVOKE CREATE ON SCHEMA public FROM PUBLIC;

-- Función trigger compartida para garantizar inmutabilidad append-only en registro_decisiones (RF-35, HU-35.1, ADR-12).
-- Cada servicio (S1..S6) vincula esta función mediante trigger BEFORE UPDATE OR DELETE en su migración inicial.
CREATE OR REPLACE FUNCTION public.impedir_modificacion_registro_decisiones()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    RAISE EXCEPTION 'El registro de decisiones no se puede modificar ni eliminar.'
        USING ERRCODE = '55000';
END;
$$;
GRANT EXECUTE ON FUNCTION public.impedir_modificacion_registro_decisiones() TO PUBLIC;

DO $$
DECLARE
    servicios TEXT[] := ARRAY[
        's1_identidad',
        's2_catalogo_fondos',
        's3_postulacion_evaluacion',
        's4_becas',
        's5_seguimiento',
        's6_comunicaciones'
    ];
    s TEXT;
    rol_migracion TEXT;
    rol_runtime TEXT;
BEGIN
    FOREACH s IN ARRAY servicios LOOP
        rol_migracion := s || '_migrator';
        rol_runtime := s || '_app';

        IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = rol_migracion) THEN
            EXECUTE format('CREATE ROLE %I LOGIN PASSWORD %L', rol_migracion, 'dev_' || rol_migracion);
        END IF;

        IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = rol_runtime) THEN
            EXECUTE format('CREATE ROLE %I LOGIN PASSWORD %L', rol_runtime, 'dev_' || rol_runtime);
        END IF;

        EXECUTE format('CREATE SCHEMA IF NOT EXISTS %I AUTHORIZATION %I', s, rol_migracion);
        EXECUTE format('REVOKE ALL ON SCHEMA %I FROM PUBLIC', s);
        EXECUTE format('GRANT USAGE ON SCHEMA %I TO %I', s, rol_runtime);
        EXECUTE format('ALTER DEFAULT PRIVILEGES FOR ROLE %I IN SCHEMA %I GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO %I', rol_migracion, s, rol_runtime);
        EXECUTE format('ALTER DEFAULT PRIVILEGES FOR ROLE %I IN SCHEMA %I GRANT USAGE, SELECT ON SEQUENCES TO %I', rol_migracion, s, rol_runtime);
        EXECUTE format('ALTER ROLE %I SET search_path TO %I', rol_migracion, s);
        EXECUTE format('ALTER ROLE %I SET search_path TO %I', rol_runtime, s);
    END LOOP;
END $$;
