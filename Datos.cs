#nullable disable
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using MySql.Data.MySqlClient;

namespace Billetera
{
    public static class Conexion
    {
        // XAMPP: usuario root, sin contraseña. Si tu MySQL tiene contraseña, escríbela en Pwd=
        public const string Cadena = "Server=localhost;Database=billetera;Uid=root;Pwd=;CharSet=utf8mb4;SslMode=Disabled;AllowPublicKeyRetrieval=True;";

        public static MySqlConnection Abrir()
        {
            MySqlConnection cn = new MySqlConnection(Cadena);
            cn.Open();
            return cn;
        }
    }

    /// <summary>Todas las consultas a MySQL. Cada método abre la conexión, ejecuta su consulta y la cierra.</summary>
    public static class Datos
    {
        // La contraseña se guarda cifrada (SHA-256), igual que SHA2(..., 256) en MySQL
        public static string Hash(string texto)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] b = sha.ComputeHash(Encoding.UTF8.GetBytes(texto));
                return BitConverter.ToString(b).Replace("-", "").ToLowerInvariant();
            }
        }

        // ================= USUARIOS =================
        public static bool Login(string correo, string clave)
        {
            using (MySqlConnection cn = Conexion.Abrir())
            {
                string consulta = "SELECT id_usuario, nombre, correo, moneda, saldo_inicial, ingreso_mensual FROM usuarios " +
                                  "WHERE correo = @c AND contrasena = @p AND activo = 1";
                MySqlCommand cmd = new MySqlCommand(consulta, cn);
                cmd.Parameters.AddWithValue("@c", correo.Trim());
                cmd.Parameters.AddWithValue("@p", Hash(clave));

                MySqlDataReader r = cmd.ExecuteReader();
                if (!r.Read()) return false;

                Sesion.IdUsuario = r.GetInt32(0);
                Sesion.Nombre = r.GetString(1);
                Sesion.Correo = r.GetString(2);
                Sesion.Moneda = r.GetString(3);
                Sesion.SaldoInicial = r.GetDecimal(4);
                Sesion.IngresoMensual = r.GetDecimal(5);
                return true;
            }
        }

        /// <summary>Devuelve null si todo salió bien, o el mensaje de error.</summary>
        public static string Registrar(string nombre, string correo, string clave, decimal saldo, decimal ingresoMensual)
        {
            using (MySqlConnection cn = Conexion.Abrir())
            {
                // 1. Revisar que el correo no exista
                MySqlCommand buscar = new MySqlCommand("SELECT COUNT(*) FROM usuarios WHERE correo = @c", cn);
                buscar.Parameters.AddWithValue("@c", correo.Trim());
                int existe = Convert.ToInt32(buscar.ExecuteScalar());
                if (existe > 0) return "Ya existe una cuenta con ese correo.";

                // 2. Guardar el usuario nuevo
                string consulta = "INSERT INTO usuarios (nombre, correo, contrasena, saldo_inicial, ingreso_mensual) " +
                                  "VALUES (@n, @c, @p, @s, @i)";
                MySqlCommand cmd = new MySqlCommand(consulta, cn);
                cmd.Parameters.AddWithValue("@n", nombre.Trim());
                cmd.Parameters.AddWithValue("@c", correo.Trim());
                cmd.Parameters.AddWithValue("@p", Hash(clave));
                cmd.Parameters.AddWithValue("@s", saldo);
                cmd.Parameters.AddWithValue("@i", ingresoMensual);
                cmd.ExecuteNonQuery();
            }
            return null;
        }

        public static void ActualizarIngresoMensual(decimal valor)
        {
            using (MySqlConnection cn = Conexion.Abrir())
            {
                MySqlCommand cmd = new MySqlCommand("UPDATE usuarios SET ingreso_mensual = @v WHERE id_usuario = @u", cn);
                cmd.Parameters.AddWithValue("@v", valor);
                cmd.Parameters.AddWithValue("@u", Sesion.IdUsuario);
                cmd.ExecuteNonQuery();
            }
            Sesion.IngresoMensual = valor;
        }

        // ================= SALDO Y RESÚMENES =================
        /// <summary>Saldo = saldo inicial + ingresos - gastos (hasta antesDe, sin incluirlo; null = todo).</summary>
        public static decimal SaldoAl(DateTime? antesDe)
        {
            using (MySqlConnection cn = Conexion.Abrir())
            {
                string consulta = "SELECT COALESCE(SUM(CASE WHEN tipo = 'ingreso' THEN monto ELSE -monto END), 0) " +
                                  "FROM movimientos WHERE id_usuario = @u";
                if (antesDe.HasValue) consulta += " AND fecha < @f";

                MySqlCommand cmd = new MySqlCommand(consulta, cn);
                cmd.Parameters.AddWithValue("@u", Sesion.IdUsuario);
                if (antesDe.HasValue) cmd.Parameters.AddWithValue("@f", antesDe.Value.Date);

                decimal neto = Convert.ToDecimal(cmd.ExecuteScalar());
                return Sesion.SaldoInicial + neto;
            }
        }

        public static Resumen ResumenMes(DateTime mes)
        {
            DateTime desde = new DateTime(mes.Year, mes.Month, 1);
            Resumen res = new Resumen();
            using (MySqlConnection cn = Conexion.Abrir())
            {
                string consulta = "SELECT COALESCE(SUM(CASE WHEN tipo = 'ingreso' THEN monto ELSE 0 END), 0), " +
                                  "       COALESCE(SUM(CASE WHEN tipo = 'gasto' THEN monto ELSE 0 END), 0) " +
                                  "FROM movimientos WHERE id_usuario = @u AND fecha >= @d AND fecha < @h";
                MySqlCommand cmd = new MySqlCommand(consulta, cn);
                cmd.Parameters.AddWithValue("@u", Sesion.IdUsuario);
                cmd.Parameters.AddWithValue("@d", desde);
                cmd.Parameters.AddWithValue("@h", desde.AddMonths(1));

                MySqlDataReader r = cmd.ExecuteReader();
                if (r.Read())
                {
                    res.Ingresos = r.GetDecimal(0);
                    res.Gastos = r.GetDecimal(1);
                }
            }
            return res;
        }

        // ================= CATEGORÍAS =================
        public static List<CategoriaItem> Categorias(string tipo)
        {
            List<CategoriaItem> lista = new List<CategoriaItem>();
            using (MySqlConnection cn = Conexion.Abrir())
            {
                string consulta = "SELECT id_categoria, nombre, tipo, color FROM categorias";
                if (!string.IsNullOrEmpty(tipo)) consulta += " WHERE tipo = @t";
                consulta += " ORDER BY id_categoria";

                MySqlCommand cmd = new MySqlCommand(consulta, cn);
                if (!string.IsNullOrEmpty(tipo)) cmd.Parameters.AddWithValue("@t", tipo);

                MySqlDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    CategoriaItem c = new CategoriaItem();
                    c.Id = r.GetInt32(0);
                    c.Nombre = r.GetString(1);
                    c.Tipo = r.GetString(2);
                    c.Color = r.GetString(3);
                    lista.Add(c);
                }
            }
            return lista;
        }

        // ================= MOVIMIENTOS =================
        /// <summary>mes = null trae de todos los meses; tipo = null trae gastos e ingresos; idCategoria = 0 trae todas; limite = 0 sin límite.</summary>
        public static List<MovimientoItem> Movimientos(DateTime? mes, string tipo, int idCategoria, int limite)
        {
            List<MovimientoItem> lista = new List<MovimientoItem>();
            using (MySqlConnection cn = Conexion.Abrir())
            {
                // 1. Armar la consulta según los filtros que se pidieron
                string consulta = "SELECT m.id_movimiento, m.tipo, m.monto, m.fecha, m.nota, c.nombre, c.color " +
                                  "FROM movimientos m JOIN categorias c ON c.id_categoria = m.id_categoria " +
                                  "WHERE m.id_usuario = @u";
                if (mes.HasValue) consulta += " AND m.fecha >= @d AND m.fecha < @h";
                if (!string.IsNullOrEmpty(tipo)) consulta += " AND m.tipo = @t";
                if (idCategoria > 0) consulta += " AND m.id_categoria = @c";
                consulta += " ORDER BY m.fecha DESC, m.id_movimiento DESC";
                if (limite > 0) consulta += " LIMIT " + limite;

                // 2. Pasar los valores de los filtros
                MySqlCommand cmd = new MySqlCommand(consulta, cn);
                cmd.Parameters.AddWithValue("@u", Sesion.IdUsuario);
                if (mes.HasValue)
                {
                    DateTime desde = new DateTime(mes.Value.Year, mes.Value.Month, 1);
                    cmd.Parameters.AddWithValue("@d", desde);
                    cmd.Parameters.AddWithValue("@h", desde.AddMonths(1));
                }
                if (!string.IsNullOrEmpty(tipo)) cmd.Parameters.AddWithValue("@t", tipo);
                if (idCategoria > 0) cmd.Parameters.AddWithValue("@c", idCategoria);

                // 3. Leer los resultados
                MySqlDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    MovimientoItem m = new MovimientoItem();
                    m.Id = r.GetInt32(0);
                    m.Tipo = r.GetString(1);
                    m.Monto = r.GetDecimal(2);
                    m.Fecha = r.GetDateTime(3);
                    if (r.IsDBNull(4)) m.Nota = ""; else m.Nota = r.GetString(4);
                    m.Categoria = r.GetString(5);
                    m.Color = r.GetString(6);
                    lista.Add(m);
                }
            }
            return lista;
        }

        public static void InsertarMovimiento(string tipo, int idCategoria, decimal monto, DateTime fecha, string nota)
        {
            using (MySqlConnection cn = Conexion.Abrir())
            {
                string consulta = "INSERT INTO movimientos (id_usuario, id_categoria, tipo, monto, fecha, nota) " +
                                  "VALUES (@u, @c, @t, @m, @f, @n)";
                MySqlCommand cmd = new MySqlCommand(consulta, cn);
                cmd.Parameters.AddWithValue("@u", Sesion.IdUsuario);
                cmd.Parameters.AddWithValue("@c", idCategoria);
                cmd.Parameters.AddWithValue("@t", tipo);
                cmd.Parameters.AddWithValue("@m", monto);
                cmd.Parameters.AddWithValue("@f", fecha.Date);
                cmd.Parameters.AddWithValue("@n", nota);
                cmd.ExecuteNonQuery();
            }
        }

        public static void EliminarMovimiento(int id)
        {
            using (MySqlConnection cn = Conexion.Abrir())
            {
                MySqlCommand cmd = new MySqlCommand("DELETE FROM movimientos WHERE id_movimiento = @i AND id_usuario = @u", cn);
                cmd.Parameters.AddWithValue("@i", id);
                cmd.Parameters.AddWithValue("@u", Sesion.IdUsuario);
                cmd.ExecuteNonQuery();
            }
        }

        // ================= PRESUPUESTOS =================
        public static List<PresupuestoItem> Presupuestos(DateTime mes)
        {
            List<PresupuestoItem> lista = new List<PresupuestoItem>();
            DateTime desde = new DateTime(mes.Year, mes.Month, 1);
            using (MySqlConnection cn = Conexion.Abrir())
            {
                // "gastado" = suma de los gastos de esa categoría en el mes
                string consulta = "SELECT p.id_presupuesto, p.id_categoria, c.nombre, c.color, p.limite, " +
                                  "  COALESCE((SELECT SUM(m.monto) FROM movimientos m " +
                                  "            WHERE m.id_usuario = p.id_usuario AND m.id_categoria = p.id_categoria " +
                                  "              AND m.tipo = 'gasto' AND m.fecha >= @d AND m.fecha < @h), 0) AS gastado " +
                                  "FROM presupuestos p JOIN categorias c ON c.id_categoria = p.id_categoria " +
                                  "WHERE p.id_usuario = @u AND p.anio = @a AND p.mes = @m " +
                                  "ORDER BY c.id_categoria";
                MySqlCommand cmd = new MySqlCommand(consulta, cn);
                cmd.Parameters.AddWithValue("@d", desde);
                cmd.Parameters.AddWithValue("@h", desde.AddMonths(1));
                cmd.Parameters.AddWithValue("@u", Sesion.IdUsuario);
                cmd.Parameters.AddWithValue("@a", desde.Year);
                cmd.Parameters.AddWithValue("@m", desde.Month);

                MySqlDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    PresupuestoItem p = new PresupuestoItem();
                    p.Id = r.GetInt32(0);
                    p.IdCategoria = r.GetInt32(1);
                    p.Categoria = r.GetString(2);
                    p.Color = r.GetString(3);
                    p.Limite = r.GetDecimal(4);
                    p.Gastado = r.GetDecimal(5);
                    lista.Add(p);
                }
            }
            return lista;
        }

        /// <summary>Crea el presupuesto o, si ya existe para esa categoría y mes, cambia el límite.</summary>
        public static void GuardarPresupuesto(int idCategoria, DateTime mes, decimal limite)
        {
            using (MySqlConnection cn = Conexion.Abrir())
            {
                string consulta = "INSERT INTO presupuestos (id_usuario, id_categoria, anio, mes, limite) " +
                                  "VALUES (@u, @c, @a, @m, @l) " +
                                  "ON DUPLICATE KEY UPDATE limite = @l";
                MySqlCommand cmd = new MySqlCommand(consulta, cn);
                cmd.Parameters.AddWithValue("@u", Sesion.IdUsuario);
                cmd.Parameters.AddWithValue("@c", idCategoria);
                cmd.Parameters.AddWithValue("@a", mes.Year);
                cmd.Parameters.AddWithValue("@m", mes.Month);
                cmd.Parameters.AddWithValue("@l", limite);
                cmd.ExecuteNonQuery();
            }
        }

        public static void EliminarPresupuesto(int id)
        {
            using (MySqlConnection cn = Conexion.Abrir())
            {
                MySqlCommand cmd = new MySqlCommand("DELETE FROM presupuestos WHERE id_presupuesto = @i AND id_usuario = @u", cn);
                cmd.Parameters.AddWithValue("@i", id);
                cmd.Parameters.AddWithValue("@u", Sesion.IdUsuario);
                cmd.ExecuteNonQuery();
            }
        }

        // ================= METAS =================
        public static List<MetaItem> Metas()
        {
            List<MetaItem> lista = new List<MetaItem>();
            using (MySqlConnection cn = Conexion.Abrir())
            {
                string consulta = "SELECT id_meta, nombre, monto_objetivo, monto_actual, fecha_limite FROM metas " +
                                  "WHERE id_usuario = @u AND activa = 1 ORDER BY id_meta";
                MySqlCommand cmd = new MySqlCommand(consulta, cn);
                cmd.Parameters.AddWithValue("@u", Sesion.IdUsuario);

                MySqlDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    MetaItem m = new MetaItem();
                    m.Id = r.GetInt32(0);
                    m.Nombre = r.GetString(1);
                    m.Objetivo = r.GetDecimal(2);
                    m.Actual = r.GetDecimal(3);
                    if (r.IsDBNull(4)) m.FechaLimite = null; else m.FechaLimite = r.GetDateTime(4);
                    lista.Add(m);
                }
            }
            return lista;
        }

        public static void CrearMeta(string nombre, decimal objetivo, decimal actual, DateTime? limite)
        {
            using (MySqlConnection cn = Conexion.Abrir())
            {
                string consulta = "INSERT INTO metas (id_usuario, nombre, monto_objetivo, monto_actual, fecha_limite) " +
                                  "VALUES (@u, @n, @o, @a, @f)";
                MySqlCommand cmd = new MySqlCommand(consulta, cn);
                cmd.Parameters.AddWithValue("@u", Sesion.IdUsuario);
                cmd.Parameters.AddWithValue("@n", nombre.Trim());
                cmd.Parameters.AddWithValue("@o", objetivo);
                cmd.Parameters.AddWithValue("@a", actual);
                // La fecha límite es opcional: si no hay, se guarda NULL
                if (limite.HasValue) cmd.Parameters.AddWithValue("@f", limite.Value.Date);
                else cmd.Parameters.AddWithValue("@f", DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        public static void AportarMeta(int id, decimal monto)
        {
            using (MySqlConnection cn = Conexion.Abrir())
            {
                // LEAST evita que lo ahorrado pase del objetivo
                string consulta = "UPDATE metas SET monto_actual = LEAST(monto_actual + @m, monto_objetivo) " +
                                  "WHERE id_meta = @i AND id_usuario = @u";
                MySqlCommand cmd = new MySqlCommand(consulta, cn);
                cmd.Parameters.AddWithValue("@m", monto);
                cmd.Parameters.AddWithValue("@i", id);
                cmd.Parameters.AddWithValue("@u", Sesion.IdUsuario);
                cmd.ExecuteNonQuery();
            }
        }

        public static void EliminarMeta(int id)
        {
            using (MySqlConnection cn = Conexion.Abrir())
            {
                MySqlCommand cmd = new MySqlCommand("DELETE FROM metas WHERE id_meta = @i AND id_usuario = @u", cn);
                cmd.Parameters.AddWithValue("@i", id);
                cmd.Parameters.AddWithValue("@u", Sesion.IdUsuario);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
