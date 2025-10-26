using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

namespace EquipmentTracker
{
    public class EquipmentRepository
    {
        private readonly string _connectionString;
        private readonly string _dbPath;

        public EquipmentRepository(string dbPath = "equipment.db")
        {
            _dbPath = dbPath;
            _connectionString = $"Data Source={_dbPath};Version=3;";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            if (!File.Exists(_dbPath))
            {
                SQLiteConnection.CreateFile(_dbPath);
            }

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                CreateTables(connection);
            }
        }

        private void CreateTables(SQLiteConnection connection)
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
                    Notes TEXT
                )";

            using (var cmd = new SQLiteCommand(createEquipmentTable, connection))
            {
                cmd.ExecuteNonQuery();
            }

            using (var cmd = new SQLiteCommand(createTransactionsTable, connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public async Task<List<Equipment>> GetAllEquipmentAsync()
        {
            var equipmentList = new List<Equipment>();
            var query = "SELECT * FROM Equipment ORDER BY Name";

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var cmd = new SQLiteCommand(query, connection))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        equipmentList.Add(MapReaderToEquipment(reader));
                    }
                }
            }
            return equipmentList;
        }

        public async Task AddEquipmentAsync(Equipment equipment)
        {
            equipment.Id = Guid.NewGuid().ToString();
            equipment.LastUpdated = DateTime.Now;

            var query = @"INSERT INTO Equipment (Id, Name, Quantity, Category, MinStockLevel, LastUpdated)
                         VALUES (@Id, @Name, @Quantity, @Category, @MinStockLevel, @LastUpdated)";

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    using (var cmd = new SQLiteCommand(query, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@Id", equipment.Id);
                        cmd.Parameters.AddWithValue("@Name", equipment.Name);
                        cmd.Parameters.AddWithValue("@Quantity", equipment.Quantity);
                        cmd.Parameters.AddWithValue("@Category", equipment.Category ?? "");
                        cmd.Parameters.AddWithValue("@MinStockLevel", equipment.MinStockLevel);
                        cmd.Parameters.AddWithValue("@LastUpdated", equipment.LastUpdated);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    await LogTransactionAsync(connection, transaction, equipment.Id, equipment.Name, "Create", 0, equipment.Quantity, "Equipment created");
                    transaction.Commit();
                }
            }
        }

        public async Task UpdateEquipmentAsync(Equipment equipment)
        {
            equipment.LastUpdated = DateTime.Now;

            var query = @"UPDATE Equipment
                         SET Name = @Name, Category = @Category, MinStockLevel = @MinStockLevel, LastUpdated = @LastUpdated
                         WHERE Id = @Id";

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", equipment.Id);
                    cmd.Parameters.AddWithValue("@Name", equipment.Name);
                    cmd.Parameters.AddWithValue("@Category", equipment.Category ?? "");
                    cmd.Parameters.AddWithValue("@MinStockLevel", equipment.MinStockLevel);
                    cmd.Parameters.AddWithValue("@LastUpdated", equipment.LastUpdated);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task UpdateQuantityAsync(string equipmentId, string equipmentName, int oldQuantity, int newQuantity, string changeType, string notes = "")
        {
            var query = "UPDATE Equipment SET Quantity = @Quantity, LastUpdated = @LastUpdated WHERE Id = @Id";

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    using (var cmd = new SQLiteCommand(query, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@Quantity", newQuantity);
                        cmd.Parameters.AddWithValue("@LastUpdated", DateTime.Now);
                        cmd.Parameters.AddWithValue("@Id", equipmentId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    await LogTransactionAsync(connection, transaction, equipmentId, equipmentName, changeType, oldQuantity, newQuantity, notes);
                    transaction.Commit();
                }
            }
        }

        public async Task DeleteEquipmentAsync(Equipment equipment)
        {
            var query = "DELETE FROM Equipment WHERE Id = @Id";

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    using (var cmd = new SQLiteCommand(query, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@Id", equipment.Id);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    await LogTransactionAsync(connection, transaction, equipment.Id, equipment.Name, "Delete", equipment.Quantity, 0, "Equipment deleted");
                    transaction.Commit();
                }
            }
        }

        public async Task<List<Transaction>> GetTransactionsAsync(int page = 1, int pageSize = 100)
        {
            var transactions = new List<Transaction>();
            var query = $"SELECT * FROM Transactions ORDER BY Timestamp DESC LIMIT {pageSize} OFFSET {(page - 1) * pageSize}";

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var cmd = new SQLiteCommand(query, connection))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        transactions.Add(MapReaderToTransaction(reader));
                    }
                }
            }
            return transactions;
        }

        private async Task LogTransactionAsync(SQLiteConnection connection, SQLiteTransaction transaction, string equipmentId, string equipmentName, string changeType, int oldQuantity, int newQuantity, string notes)
        {
            var query = @"INSERT INTO Transactions (EquipmentId, EquipmentName, Timestamp, ChangeType, OldQuantity, NewQuantity, Notes)
                         VALUES (@EquipmentId, @EquipmentName, @Timestamp, @ChangeType, @OldQuantity, @NewQuantity, @Notes)";

            using (var cmd = new SQLiteCommand(query, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@EquipmentId", equipmentId);
                cmd.Parameters.AddWithValue("@EquipmentName", equipmentName);
                cmd.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                cmd.Parameters.AddWithValue("@ChangeType", changeType);
                cmd.Parameters.AddWithValue("@OldQuantity", oldQuantity);
                cmd.Parameters.AddWithValue("@NewQuantity", newQuantity);
                cmd.Parameters.AddWithValue("@Notes", notes ?? "");
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task BackupDatabaseAsync(string backupPath)
        {
            if (File.Exists(_dbPath))
            {
                using (Stream source = new FileStream(_dbPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (Stream destination = new FileStream(backupPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await source.CopyToAsync(destination);
                }
            }
        }

        // Legacy synchronous methods for backward compatibility
        public List<Equipment> GetAllEquipment()
        {
            return GetAllEquipmentAsync().GetAwaiter().GetResult();
        }

        public void AddEquipment(Equipment equipment)
        {
            AddEquipmentAsync(equipment).GetAwaiter().GetResult();
        }

        public void UpdateEquipment(Equipment equipment)
        {
            UpdateEquipmentAsync(equipment).GetAwaiter().GetResult();
        }

        public void UpdateQuantity(string equipmentId, int newQuantity, string changeType, string notes = "")
        {
            var equipment = GetEquipmentById(equipmentId);
            if (equipment != null)
            {
                UpdateQuantityAsync(equipmentId, equipment.Name, equipment.Quantity, newQuantity, changeType, notes).GetAwaiter().GetResult();
            }
        }

        public void DeleteEquipment(string equipmentId)
        {
            var equipment = GetEquipmentById(equipmentId);
            if (equipment != null)
            {
                DeleteEquipmentAsync(equipment).GetAwaiter().GetResult();
            }
        }

        public List<Transaction> GetTransactions(int limit = 100)
        {
            return GetTransactionsAsync(1, limit).GetAwaiter().GetResult();
        }

        public void BackupDatabase(string backupPath)
        {
            BackupDatabaseAsync(backupPath).GetAwaiter().GetResult();
        }

        public Equipment GetEquipmentById(string id)
        {
            var query = "SELECT * FROM Equipment WHERE Id = @Id";

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapReaderToEquipment(reader);
                        }
                    }
                }
            }
            return null;
        }

        private Equipment MapReaderToEquipment(SQLiteDataReader reader)
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

        private Transaction MapReaderToTransaction(SQLiteDataReader reader)
        {
            return new Transaction
            {
                Id = Convert.ToInt32(reader["Id"]),
                EquipmentId = reader["EquipmentId"].ToString(),
                EquipmentName = reader["EquipmentName"].ToString(),
                Timestamp = Convert.ToDateTime(reader["Timestamp"]),
                ChangeType = reader["ChangeType"].ToString(),
                OldQuantity = Convert.ToInt32(reader["OldQuantity"]),
                NewQuantity = Convert.ToInt32(reader["NewQuantity"]),
                Notes = reader["Notes"].ToString()
            };
        }

        public void Dispose()
        {
            // No persistent connection to dispose
        }
    }
}