using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace EquipmentTracker
{
    public class EquipmentRepository : IDisposable
    {
        private readonly string _connectionString;
        private SQLiteConnection _connection;

        public EquipmentRepository(string dbPath = "equipment.db")
        {
            _connectionString = $"Data Source={dbPath};Version=3;";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            _connection = new SQLiteConnection(_connectionString);
            _connection.Open();
            CreateTables();
        }

        private void CreateTables()
        {
            var createEquipmentTable = @"
                CREATE TABLE IF NOT EXISTS Equipment (
                    Id TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Quantity INTEGER DEFAULT 0,
                    Category TEXT,
                    MinStockLevel INTEGER DEFAULT 0,
                    LastUpdated DATETIME DEFAULT CURRENT_TIMESTAMP
                )";

            var createTransactionsTable = @"
                CREATE TABLE IF NOT EXISTS Transactions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    EquipmentId TEXT,
                    EquipmentName TEXT,
                    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
                    ChangeType TEXT,
                    OldQuantity INTEGER,
                    NewQuantity INTEGER,
                    Notes TEXT,
                    FOREIGN KEY(EquipmentId) REFERENCES Equipment(Id)
                )";

            using (var cmd = new SQLiteCommand(createEquipmentTable, _connection))
            {
                cmd.ExecuteNonQuery();
            }

            using (var cmd = new SQLiteCommand(createTransactionsTable, _connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public List<Equipment> GetAllEquipment()
        {
            var equipment = new List<Equipment>();
            var query = "SELECT * FROM Equipment ORDER BY Name";

            using (var cmd = new SQLiteCommand(query, _connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    equipment.Add(new Equipment
                    {
                        Id = reader["Id"].ToString(),
                        Name = reader["Name"].ToString(),
                        Quantity = Convert.ToInt32(reader["Quantity"]),
                        Category = reader["Category"].ToString(),
                        MinStockLevel = Convert.ToInt32(reader["MinStockLevel"]),
                        LastUpdated = Convert.ToDateTime(reader["LastUpdated"])
                    });
                }
            }
            return equipment;
        }

        public void AddEquipment(Equipment equipment)
        {
            equipment.Id = Guid.NewGuid().ToString();
            equipment.LastUpdated = DateTime.Now;

            var query = @"INSERT INTO Equipment (Id, Name, Quantity, Category, MinStockLevel, LastUpdated) 
                         VALUES (@Id, @Name, @Quantity, @Category, @MinStockLevel, @LastUpdated)";

            using (var cmd = new SQLiteCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@Id", equipment.Id);
                cmd.Parameters.AddWithValue("@Name", equipment.Name);
                cmd.Parameters.AddWithValue("@Quantity", equipment.Quantity);
                cmd.Parameters.AddWithValue("@Category", equipment.Category ?? "");
                cmd.Parameters.AddWithValue("@MinStockLevel", equipment.MinStockLevel);
                cmd.Parameters.AddWithValue("@LastUpdated", equipment.LastUpdated);
                cmd.ExecuteNonQuery();
            }

            LogTransaction(equipment.Id, equipment.Name, "Create", 0, equipment.Quantity, "Equipment created");
        }

        public void UpdateEquipment(Equipment equipment)
        {
            equipment.LastUpdated = DateTime.Now;

            var query = @"UPDATE Equipment 
                         SET Name = @Name, Quantity = @Quantity, Category = @Category, 
                             MinStockLevel = @MinStockLevel, LastUpdated = @LastUpdated 
                         WHERE Id = @Id";

            using (var cmd = new SQLiteCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@Id", equipment.Id);
                cmd.Parameters.AddWithValue("@Name", equipment.Name);
                cmd.Parameters.AddWithValue("@Quantity", equipment.Quantity);
                cmd.Parameters.AddWithValue("@Category", equipment.Category ?? "");
                cmd.Parameters.AddWithValue("@MinStockLevel", equipment.MinStockLevel);
                cmd.Parameters.AddWithValue("@LastUpdated", equipment.LastUpdated);
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateQuantity(string equipmentId, int newQuantity, string changeType, string notes = "")
        {
            var oldQuantity = GetEquipmentQuantity(equipmentId);
            var equipmentName = GetEquipmentName(equipmentId);

            var query = "UPDATE Equipment SET Quantity = @Quantity, LastUpdated = @LastUpdated WHERE Id = @Id";

            using (var cmd = new SQLiteCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@Quantity", newQuantity);
                cmd.Parameters.AddWithValue("@LastUpdated", DateTime.Now);
                cmd.Parameters.AddWithValue("@Id", equipmentId);
                cmd.ExecuteNonQuery();
            }

            LogTransaction(equipmentId, equipmentName, changeType, oldQuantity, newQuantity, notes);
        }

        public void DeleteEquipment(string equipmentId)
        {
            var equipment = GetEquipmentById(equipmentId);
            if (equipment == null) return;

            var query = "DELETE FROM Equipment WHERE Id = @Id";

            using (var cmd = new SQLiteCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@Id", equipmentId);
                cmd.ExecuteNonQuery();
            }

            LogTransaction(equipmentId, equipment.Name, "Delete", equipment.Quantity, 0, "Equipment deleted");
        }

        public Equipment GetEquipmentById(string id)
        {
            var query = "SELECT * FROM Equipment WHERE Id = @Id";

            using (var cmd = new SQLiteCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Equipment
                        {
                            Id = reader["Id"].ToString(),
                            Name = reader["Name"].ToString(),
                            Quantity = Convert.ToInt32(reader["Quantity"]),
                            Category = reader["Category"].ToString(),
                            MinStockLevel = Convert.ToInt32(reader["MinStockLevel"]),
                            LastUpdated = Convert.ToDateTime(reader["LastUpdated"])
                        };
                    }
                }
            }
            return null;
        }

        private int GetEquipmentQuantity(string equipmentId)
        {
            var query = "SELECT Quantity FROM Equipment WHERE Id = @Id";
            using (var cmd = new SQLiteCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@Id", equipmentId);
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        private string GetEquipmentName(string equipmentId)
        {
            var query = "SELECT Name FROM Equipment WHERE Id = @Id";
            using (var cmd = new SQLiteCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@Id", equipmentId);
                var result = cmd.ExecuteScalar();
                return result?.ToString() ?? "";
            }
        }

        private void LogTransaction(string equipmentId, string equipmentName, string changeType, int oldQuantity, int newQuantity, string notes)
        {
            var query = @"INSERT INTO Transactions (EquipmentId, EquipmentName, Timestamp, ChangeType, OldQuantity, NewQuantity, Notes)
                         VALUES (@EquipmentId, @EquipmentName, @Timestamp, @ChangeType, @OldQuantity, @NewQuantity, @Notes)";

            using (var cmd = new SQLiteCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@EquipmentId", equipmentId);
                cmd.Parameters.AddWithValue("@EquipmentName", equipmentName);
                cmd.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                cmd.Parameters.AddWithValue("@ChangeType", changeType);
                cmd.Parameters.AddWithValue("@OldQuantity", oldQuantity);
                cmd.Parameters.AddWithValue("@NewQuantity", newQuantity);
                cmd.Parameters.AddWithValue("@Notes", notes ?? "");
                cmd.ExecuteNonQuery();
            }
        }

        public List<Transaction> GetTransactions(int limit = 100)
        {
            var transactions = new List<Transaction>();
            var query = "SELECT * FROM Transactions ORDER BY Timestamp DESC LIMIT @Limit";

            using (var cmd = new SQLiteCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@Limit", limit);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        transactions.Add(new Transaction
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            EquipmentId = reader["EquipmentId"].ToString(),
                            EquipmentName = reader["EquipmentName"].ToString(),
                            Timestamp = Convert.ToDateTime(reader["Timestamp"]),
                            ChangeType = reader["ChangeType"].ToString(),
                            OldQuantity = Convert.ToInt32(reader["OldQuantity"]),
                            NewQuantity = Convert.ToInt32(reader["NewQuantity"]),
                            Notes = reader["Notes"].ToString()
                        });
                    }
                }
            }
            return transactions;
        }

        public void BackupDatabase(string backupPath)
        {
            var dbFile = _connectionString.Split('=')[1].Split(';')[0];
            if (File.Exists(dbFile))
            {
                File.Copy(dbFile, backupPath, true);
            }
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}