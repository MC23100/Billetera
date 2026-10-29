-- =====================================================
-- BASE DE DATOS: billetera  (app de finanzas personales)
-- Ejecutar en phpMyAdmin > pestaña SQL
-- OJO: borra y vuelve a crear la base, así se puede ejecutar varias veces
-- =====================================================
DROP DATABASE IF EXISTS billetera;
CREATE DATABASE billetera CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE billetera;

-- -----------------------------------------------------
-- 1. USUARIOS  (el saldo y el ingreso mensual se piden al crear la cuenta)
-- -----------------------------------------------------
CREATE TABLE usuarios (
    id_usuario      INT AUTO_INCREMENT PRIMARY KEY,
    nombre          VARCHAR(100)  NOT NULL,
    correo          VARCHAR(120)  NOT NULL UNIQUE,
    contrasena      CHAR(64)      NOT NULL,                 -- hash SHA-256 en hexadecimal
    saldo_inicial   DECIMAL(12,2) NOT NULL DEFAULT 0,       -- saldo actual que ingresa al registrarse
    ingreso_mensual DECIMAL(12,2) NOT NULL DEFAULT 0,       -- ganancia mensual estimada
    moneda          ENUM('MXN','NIO','USD') NOT NULL DEFAULT 'MXN',
    fecha_registro  DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    activo          BOOLEAN       NOT NULL DEFAULT TRUE
);

-- -----------------------------------------------------
-- 2. CATEGORIAS  (compartidas por todos los usuarios)
-- -----------------------------------------------------
CREATE TABLE categorias (
    id_categoria INT AUTO_INCREMENT PRIMARY KEY,
    nombre       VARCHAR(40) NOT NULL,
    tipo         ENUM('gasto','ingreso') NOT NULL,
    color        CHAR(7)     NOT NULL DEFAULT '#0B6B3A'     -- color hexadecimal para la interfaz
);

-- -----------------------------------------------------
-- 3. MOVIMIENTOS  (ingresos y gastos)
-- -----------------------------------------------------
CREATE TABLE movimientos (
    id_movimiento INT AUTO_INCREMENT PRIMARY KEY,
    id_usuario    INT           NOT NULL,
    id_categoria  INT           NOT NULL,
    tipo          ENUM('gasto','ingreso') NOT NULL,
    monto         DECIMAL(12,2) NOT NULL CHECK (monto > 0),
    fecha         DATE          NOT NULL,
    nota          VARCHAR(150),
    creado_en     DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (id_usuario)   REFERENCES usuarios(id_usuario)     ON DELETE CASCADE,
    FOREIGN KEY (id_categoria) REFERENCES categorias(id_categoria),
    INDEX idx_usuario_fecha (id_usuario, fecha)
);

-- -----------------------------------------------------
-- 4. PRESUPUESTOS  (límite mensual por categoría)
-- -----------------------------------------------------
CREATE TABLE presupuestos (
    id_presupuesto INT AUTO_INCREMENT PRIMARY KEY,
    id_usuario     INT           NOT NULL,
    id_categoria   INT           NOT NULL,
    anio           SMALLINT      NOT NULL,
    mes            TINYINT       NOT NULL CHECK (mes BETWEEN 1 AND 12),
    limite         DECIMAL(12,2) NOT NULL CHECK (limite > 0),
    FOREIGN KEY (id_usuario)   REFERENCES usuarios(id_usuario)     ON DELETE CASCADE,
    FOREIGN KEY (id_categoria) REFERENCES categorias(id_categoria),
    UNIQUE KEY uq_presupuesto (id_usuario, id_categoria, anio, mes)
);

-- -----------------------------------------------------
-- 5. METAS  (ahorro con objetivo)
-- -----------------------------------------------------
CREATE TABLE metas (
    id_meta        INT AUTO_INCREMENT PRIMARY KEY,
    id_usuario     INT           NOT NULL,
    nombre         VARCHAR(60)   NOT NULL,
    monto_objetivo DECIMAL(12,2) NOT NULL CHECK (monto_objetivo > 0),
    monto_actual   DECIMAL(12,2) NOT NULL DEFAULT 0,
    fecha_limite   DATE,
    activa         BOOLEAN       NOT NULL DEFAULT TRUE,
    creada_en      DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario) ON DELETE CASCADE
);

-- =====================================================
-- DATOS DE EJEMPLO: UNA SOLA CUENTA (Ana Ramírez, octubre 2026)
-- Correo: ana@correo.com   Contraseña: Ana12345
-- =====================================================

-- Usuario
INSERT INTO usuarios (nombre, correo, contrasena, saldo_inicial, ingreso_mensual, moneda) VALUES
('Ana Ramírez', 'ana@correo.com', SHA2('Ana12345', 256), 35250.35, 34800.00, 'MXN');

-- Categorías
INSERT INTO categorias (nombre, tipo, color) VALUES
('Alimentación',    'gasto',   '#F59E0B'),   -- 1
('Transporte',      'gasto',   '#3B82F6'),   -- 2
('Hogar',           'gasto',   '#0B6B3A'),   -- 3
('Salud',           'gasto',   '#EF4444'),   -- 4
('Entretenimiento', 'gasto',   '#F43F5E'),   -- 5
('Otros',           'gasto',   '#64748B'),   -- 6
('Nómina',          'ingreso', '#16A34A'),   -- 7
('Freelance',       'ingreso', '#84CC16'),   -- 8
('Ventas',          'ingreso', '#14B8A6');   -- 9

-- Ingresos de octubre (total 34,800.00)
INSERT INTO movimientos (id_usuario, id_categoria, tipo, monto, fecha, nota) VALUES
(1, 7, 'ingreso', 18500.00, '2026-10-01', 'Nómina quincenal'),
(1, 8, 'ingreso',  9800.00, '2026-10-12', 'Proyecto de diseño web'),
(1, 9, 'ingreso',  6500.00, '2026-10-15', 'Venta de equipo usado');

-- Gastos de octubre (total 21,430.00)
INSERT INTO movimientos (id_usuario, id_categoria, tipo, monto, fecha, nota) VALUES
-- Alimentación (4,620.00)
(1, 1, 'gasto', 1245.80, '2026-10-03', 'Supermercado La Comer'),
(1, 1, 'gasto',  980.20, '2026-10-10', 'Mercado semanal'),
(1, 1, 'gasto',  640.00, '2026-10-17', 'Cena con amigos'),
(1, 1, 'gasto', 1754.00, '2026-10-20', 'Compra del mes'),
-- Transporte (2,850.00)
(1, 2, 'gasto', 1200.00, '2026-10-05', 'Gasolina'),
(1, 2, 'gasto',  450.00, '2026-10-08', 'Transporte público'),
(1, 2, 'gasto', 1200.00, '2026-10-19', 'Gasolina'),
-- Entretenimiento (2,240.00)
(1, 5, 'gasto',  350.00, '2026-10-05', 'Suscripciones de streaming'),
(1, 5, 'gasto',  890.00, '2026-10-11', 'Cine y cena'),
(1, 5, 'gasto', 1000.00, '2026-10-18', 'Concierto'),
-- Hogar (8,500.00)
(1, 3, 'gasto', 7000.00, '2026-10-01', 'Renta'),
(1, 3, 'gasto', 1500.00, '2026-10-06', 'Luz e internet'),
-- Salud (2,000.00)
(1, 4, 'gasto', 1200.00, '2026-10-09', 'Consulta médica'),
(1, 4, 'gasto',  800.00, '2026-10-14', 'Farmacia'),
-- Otros (1,220.00)
(1, 6, 'gasto', 1220.00, '2026-10-16', 'Ropa');

-- Presupuesto de octubre (límite total 30,000.00)
INSERT INTO presupuestos (id_usuario, id_categoria, anio, mes, limite) VALUES
(1, 1, 2026, 10,  6000.00),
(1, 2, 2026, 10,  3500.00),
(1, 3, 2026, 10, 12000.00),
(1, 4, 2026, 10,  3500.00),
(1, 5, 2026, 10,  2000.00),
(1, 6, 2026, 10,  3000.00);

-- Metas
INSERT INTO metas (id_usuario, nombre, monto_objetivo, monto_actual, fecha_limite) VALUES
(1, 'Fondo de emergencia', 60000.00, 38400.00, '2027-06-30'),
(1, 'Viaje a Oaxaca',      25000.00,  9750.00, '2027-03-15');

-- =====================================================
-- CONSULTAS DE COMPROBACIÓN (los cálculos que usará C#)
-- =====================================================

-- 1) Saldo actual = saldo inicial + ingresos - gastos   (debe dar 48620.35)
SELECT u.nombre,
       u.saldo_inicial
         + COALESCE(SUM(CASE WHEN m.tipo = 'ingreso' THEN m.monto END), 0)
         - COALESCE(SUM(CASE WHEN m.tipo = 'gasto'   THEN m.monto END), 0) AS saldo_actual
FROM usuarios u
LEFT JOIN movimientos m ON m.id_usuario = u.id_usuario
WHERE u.id_usuario = 1
GROUP BY u.id_usuario;

-- 2) Ingresos, gastos y flujo neto de octubre   (34800.00 / 21430.00 / 13370.00)
SELECT SUM(CASE WHEN tipo = 'ingreso' THEN monto ELSE 0 END) AS ingresos,
       SUM(CASE WHEN tipo = 'gasto'   THEN monto ELSE 0 END) AS gastos,
       SUM(CASE WHEN tipo = 'ingreso' THEN monto ELSE -monto END) AS flujo_neto
FROM movimientos
WHERE id_usuario = 1 AND YEAR(fecha) = 2026 AND MONTH(fecha) = 10;

-- 3) Presupuesto vs gastado por categoría, con porcentaje y estado
SELECT c.nombre AS categoria,
       p.limite,
       COALESCE(SUM(m.monto), 0) AS gastado,
       p.limite - COALESCE(SUM(m.monto), 0) AS restante,
       ROUND(COALESCE(SUM(m.monto), 0) / p.limite * 100) AS porcentaje,
       CASE
         WHEN COALESCE(SUM(m.monto), 0) > p.limite            THEN 'Excedido'
         WHEN COALESCE(SUM(m.monto), 0) >= p.limite * 0.9     THEN 'Cerca del límite'
         ELSE 'En ruta'
       END AS estado
FROM presupuestos p
JOIN categorias c ON c.id_categoria = p.id_categoria
LEFT JOIN movimientos m ON m.id_usuario = p.id_usuario
                       AND m.id_categoria = p.id_categoria
                       AND m.tipo = 'gasto'
                       AND YEAR(m.fecha) = p.anio AND MONTH(m.fecha) = p.mes
WHERE p.id_usuario = 1 AND p.anio = 2026 AND p.mes = 10
GROUP BY p.id_presupuesto, c.nombre, p.limite;

-- 4) Avance de las metas
SELECT nombre, monto_actual, monto_objetivo,
       monto_objetivo - monto_actual AS falta,
       ROUND(monto_actual / monto_objetivo * 100, 1) AS porcentaje_avance
FROM metas
WHERE id_usuario = 1 AND activa = TRUE;
