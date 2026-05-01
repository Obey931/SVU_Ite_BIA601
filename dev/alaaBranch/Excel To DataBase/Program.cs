
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using System.IO;



namespace ConsoleApp79
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    if (IsTableEmpty("users"))
                    {
                        Console.WriteLine("The table is empty, the entry will be made");
                        read_Excel("data\\users.xlsx", 0);
                    }
                    else
                    {
                        Console.WriteLine("The table already contains data");
                    }
                    if (IsTableEmpty("products"))
                    {
                        Console.WriteLine("The table is empty, the entry will be made");
                        read_Excel("data\\products.xlsx", 1);
                    }
                    else
                    {
                        Console.WriteLine("The table already contains data");
                    }
                    if (IsTableEmpty("ratings"))
                    {
                        Console.WriteLine("The table is empty, the entry will be made");
                        read_Excel("data\\ratings.xlsx", 2);
                    }
                    else
                    {
                        Console.WriteLine("The table already contains data");
                    }
                    if (IsTableEmpty("behavior"))
                    {
                        Console.WriteLine("The table is empty, the entry will be made");
                        read_Excel("data\\behavior_15500.xlsx", 3);
                    }
                    else
                    {
                        Console.WriteLine("The table already contains data");
                    }
                    Console.WriteLine("ok");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("no: " + ex.Message);
                }
            }
            var engine = new EvolutionEngine(connectionString);
            while (true)
                engine.Run(int.Parse(Console.ReadLine()));
        }

       
        static string connectionString = "server=localhost;user=root;password=;database=ecommerce;";

        static void read_Excel(string filePath,int x)
        {
            IWorkbook workbook;

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                if (Path.GetExtension(filePath) == ".xlsx")
                    workbook = new XSSFWorkbook(fs);
                else
                    workbook = new HSSFWorkbook(fs);
            }

            ISheet sheet = workbook.GetSheetAt(0);
            int rowCount = sheet.LastRowNum;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                for (int i = 1; i <= rowCount; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null) continue;

                    string col1 = row.GetCell(0)?.ToString();
                    string col2 = row.GetCell(1)?.ToString();
                    string col3 = row.GetCell(2)?.ToString();
                    string col4 = row.GetCell(2)?.ToString();
                    string col5 = row.GetCell(2)?.ToString();
                    string query = "";
                    if (x == 0)
                        query = "INSERT INTO users (userID, age, country) VALUES (@c1, @c2, @c3)";
                    else if (x == 1)
                        query = "INSERT INTO products (productID, category, price) VALUES (@c1, @c2, @c3)";
                    else if (x == 2)
                        query = "INSERT INTO ratings (userID, productID, rating) VALUES (@c1, @c2, @c3)";
                    else
                        query = "INSERT INTO behavior (userID, productID, viewed, clicked, purchased) VALUES (@c1, @c2, @c3, @c4, @c5)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@c1", col1);
                        cmd.Parameters.AddWithValue("@c2", col2);
                        cmd.Parameters.AddWithValue("@c3", col3);
                        cmd.Parameters.AddWithValue("@c4", col4);
                        cmd.Parameters.AddWithValue("@c5", col5);

                        cmd.ExecuteNonQuery();
                    }
                }

            }
        }
        static bool IsTableEmpty(string tableName)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string query = $"SELECT COUNT(*) FROM {tableName}";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count == 0;
                }
            }
        }
    }
}
