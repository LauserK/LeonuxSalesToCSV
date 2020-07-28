using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace SalesToCSV
{
    public partial class Form1 : Form
    {
        Connection connection = new Connection();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            infoLabel.Text = "";
            ComboboxItem item = new ComboboxItem();
            item.Text = "Panadería";
            item.Value = "00000001";
            comboBox1.Items.Add(item);
            ComboboxItem item2 = new ComboboxItem();
            item2.Text = "Restaurante";
            item2.Value = "00000002";
            comboBox1.Items.Add(item2);
            comboBox1.SelectedIndex = 0;
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            infoLabel.Text = "Generando...";
            string fromDate = desdeDate.Value.ToString("yyyy-MM-dd");
            string toDate = hastaDate.Value.ToString("yyyy-MM-dd");

            var selectQuery = "";
            var server = "10.10.0.199";
            var database = (comboBox1.SelectedItem as ComboboxItem).Value.ToString();
            var username = "root";
            var password = "123";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + username + ";" + "PASSWORD=" + password + ";";

            //selectQuery = "SELECT ventas_detalle.auto_producto AS auto, productos.codigo, productos.nombre FROM ventas_detalle INNER JOIN productos ON ventas_detalle.auto_producto = productos.auto WHERE productos.estatus = 'Activo' GROUP BY auto ORDER BY productos.nombre";            
            selectQuery = "SELECT auto, codigo, nombre FROM productos WHERE estatus = 'Activo'";
            if (connection.OpenConnection((comboBox1.SelectedItem as ComboboxItem).Value.ToString()))
            {
                try
                {
                    var table = ReadTable(connectionString, selectQuery);

                    //WriteToFile(table, @"C:\temp\outputfile.csv", false, ",");

                    StringBuilder sb = new StringBuilder();

                    /*IEnumerable<string> columnNames = table.Columns.Cast<DataColumn>().
                                                      Select(column => column.ColumnName);
                    
                    sb.AppendLine(string.Join(",", columnNames));
                    */
                    sb.AppendLine("Codigo, Nombre, Cantidad vendidos, Cantidad por divisa, Cantidad sin pago, Existencia en inventario");
                    foreach (DataRow row in table.Rows)
                    {
                        MySqlCommand cmd = new MySqlCommand("", connection.connection);
                        cmd.CommandText = "SELECT SUM(ventas_detalle.cantidad) as cantidad, SUM(ventas_detalle.total) AS total FROM ventas_detalle INNER JOIN ventas ON ventas.auto = ventas_detalle.auto_documento WHERE ventas.tipo = '01' AND ventas_detalle.auto_producto = '"+row[0]+ "' AND ventas_detalle.fecha >= '" + fromDate + "' AND ventas_detalle.fecha <= '" + toDate + "' GROUP BY auto_producto";
                        MySqlDataReader dataReader = cmd.ExecuteReader();
                        float cantidad = 0f;
                        float cantidadDolares = 0f;
                        float cantidadSinPagar = 0f;
                        if (dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                cantidad = float.Parse(dataReader["cantidad"].ToString());
                                
                            }
                        }
                        dataReader.Close();
                        cmd.CommandText = "SELECT SUM(ventas_detalle.cantidad) as cantidad, SUM(ventas_detalle.total) AS total FROM ventas_detalle INNER JOIN ventas ON ventas.auto = ventas_detalle.auto_documento WHERE ventas.tipo = '10' AND ventas_detalle.auto_producto = '" + row[0] + "' AND ventas_detalle.fecha >= '" + fromDate + "' AND ventas_detalle.fecha <= '" + toDate + "' GROUP BY auto_producto";
                        dataReader = cmd.ExecuteReader();
                        if (dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                cantidadDolares = float.Parse(dataReader["cantidad"].ToString());
                            }
                        }
                        dataReader.Close();
                        cmd.CommandText = "SELECT SUM(ventas_detalle.cantidad) as cantidad, SUM(ventas_detalle.total) AS total FROM ventas_detalle INNER JOIN ventas ON ventas.auto = ventas_detalle.auto_documento WHERE ventas.tipo = '11' AND ventas_detalle.auto_producto = '" + row[0] + "' AND ventas_detalle.fecha >= '" + fromDate + "' AND ventas_detalle.fecha <= '" + toDate + "' GROUP BY auto_producto";
                        dataReader = cmd.ExecuteReader();
                        if (dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                cantidadSinPagar = float.Parse(dataReader["cantidad"].ToString());
                            }
                        }
                        dataReader.Close();
                        sb.AppendLine("" + row[1] + "," + row[2].ToString().Replace(",",".") + "," + cantidad + ","+ cantidadDolares+","+ cantidadSinPagar+", 0");

                        /*
                         ", [articulo["auto"], desde, hasta])
                        IEnumerable<string> fields = row.ItemArray.Select(field =>
                          string.Concat("\"", field.ToString().Replace("\"", "\"\""), "\""));
                        sb.AppendLine(string.Join(",", fields));*/
                    }
                    var time = DateTime.Now;
                    string formattedTime = time.ToString("yyyyMMddhhmmss");
                    File.WriteAllText(@"C:\temp\ventas-" + formattedTime + ".csv", sb.ToString());
                    MessageBox.Show(@"ARCHIVO GUARDADO EN C:\temp\ventas-" + formattedTime + ".csv", "MENSAJE", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception _)
                {
                    MessageBox.Show("ERROR AL GENERAR EL ARCHIVO", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            infoLabel.Text = "Archivo generado...";
            button1.Enabled = true;
        }

        public static DataTable ReadTable(string connectionString, string selectQuery)
        {
            var returnValue = new DataTable();

            var conn = new MySqlConnection(connectionString);

            try
            {
                conn.Open();
                var command = new MySqlCommand(selectQuery, conn);

                using (var adapter = new MySqlDataAdapter(command))
                {
                    adapter.Fill(returnValue);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }

            return returnValue;
        }
    }

    public class ComboboxItem
    {
        public string Text { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}